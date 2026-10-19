"""Run synthetic HTTP regression checks against the normal SQLite application.

Uses only Python's standard library. Build the Release solution first. Every run
requires a new evidence directory; the database and process logs are retained.
"""
from __future__ import annotations

import argparse
import csv
import hashlib
import http.cookiejar
import io
import json
import os
from pathlib import Path
import re
import socket
import sqlite3
import subprocess
import sys
import threading
import time
from datetime import datetime, timezone
from html.parser import HTMLParser
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, urljoin
from urllib.request import HTTPRedirectHandler, HTTPCookieProcessor, ProxyHandler, Request, build_opener


def utc():
    return datetime.now(timezone.utc).isoformat()


class HiddenInputs(HTMLParser):
    def __init__(self):
        super().__init__()
        self.token = None

    def handle_starttag(self, tag, attrs):
        attr = dict(attrs)
        if tag == "input" and attr.get("name") == "__RequestVerificationToken":
            self.token = attr.get("value")


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--app-port", type=int, default=51921)
    parser.add_argument("--fixture-port", type=int, default=51922)
    parser.add_argument("--configuration", default="Release")
    args = parser.parse_args()
    repo = Path(__file__).resolve().parents[2]
    output = args.output.resolve()
    if output.exists() and any(output.iterdir()):
        parser.error("Output must be empty so previous execution evidence is preserved.")
    output.mkdir(parents=True, exist_ok=True)
    app_dll = repo / "SecureApiQualityPlatform.Web/bin" / args.configuration / "net8.0/SecureApiQualityPlatform.Web.dll"
    if not app_dll.is_file():
        parser.error(f"Build the solution first: missing {app_dll}")
    for port in (args.app_port, args.fixture_port):
        with socket.socket() as probe:
            probe.bind(("127.0.0.1", port))

    checks, requests, fixture_requests, findings, processes = [], [], [], [], []
    database = output / "isolated-app.db"
    base = f"http://127.0.0.1:{args.app_port}"
    fixture_base = f"http://127.0.0.1:{args.fixture_port}"
    fixture_state = {"status": 503}
    app_process = None
    log_handles = []
    fixture = None
    started = utc()

    def save(name, value):
        (output / name).write_text(json.dumps(value, indent=2, ensure_ascii=False), encoding="utf-8")

    def check(name, condition, actual, expected, fatal=True):
        entry = {"name": name, "status": "PASS" if condition else "FAIL", "actual": actual, "expected": expected, "atUtc": utc()}
        checks.append(entry)
        print(f"{entry['status']}: {name}", flush=True)
        if not condition and fatal:
            raise AssertionError(f"{name}: actual={actual!r}, expected={expected!r}")

    class FixtureHandler(BaseHTTPRequestHandler):
        def do_GET(self):
            fixture_requests.append({"atUtc": utc(), "path": self.path, "status": fixture_state["status"]})
            body = json.dumps({"synthetic": True, "status": fixture_state["status"]}).encode()
            self.send_response_only(fixture_state["status"])
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(body)))
            self.end_headers()
            self.wfile.write(body)

        def log_message(self, format, *values):
            pass

    def client():
        return build_opener(ProxyHandler({}), HTTPCookieProcessor(http.cookiejar.CookieJar()), NoRedirect())

    anonymous, admin, tester = client(), client(), client()

    def request(session, path, fields=None, label=None):
        data = urlencode(fields).encode() if fields is not None else None
        req = Request(urljoin(base, path), data=data)
        at = utc()
        try:
            response = session.open(req, timeout=30)
        except HTTPError as response_error:
            response = response_error
        body = response.read().decode("utf-8")
        index = len(requests) + 1
        artifact = f"http-{index:03d}-{label or 'response'}.txt"
        (output / artifact).write_text(body, encoding="utf-8")
        requests.append({"atUtc": at, "method": "POST" if data is not None else "GET", "path": path,
                         "status": response.code, "location": response.headers.get("Location"),
                         "contentType": response.headers.get("Content-Type"), "bodyArtifact": artifact,
                         "submittedFields": {k: v for k, v in (fields or {}).items() if k != "__RequestVerificationToken"}})
        return response.code, body, response.headers

    def post(session, path, fields, form_path="/", label=None):
        status, html, _ = request(session, form_path, label="form")
        if status != 200:
            raise AssertionError(f"Cannot get form token from {form_path}: {status}")
        parsed = HiddenInputs()
        parsed.feed(html)
        if not parsed.token:
            raise AssertionError(f"No real antiforgery token on {form_path}")
        return request(session, path, {**fields, "__RequestVerificationToken": parsed.token}, label=label)

    def role(session, value):
        status, _, headers = post(session, "/Role/SetRole", {"role": value, "returnUrl": "/"}, label=f"role-{value}")
        check(f"Set {value} role through real antiforgery form", status == 302 and headers.get("Location") == "/", status, "302 to /")

    def rows(table):
        with sqlite3.connect(database) as db:
            db.row_factory = sqlite3.Row
            return [dict(row) for row in db.execute(f'SELECT * FROM "{table}" ORDER BY "Id"')]

    def snapshot():
        return {table: rows(table) for table in ("Apis", "Endpoints", "CheckResults", "Defects")}

    def start_app(suffix, allow_private=True):
        nonlocal app_process
        env = os.environ.copy()
        env.update({"ASPNETCORE_ENVIRONMENT": "Development", "ConnectionStrings__PlatformDatabase": f"Data Source={database};Pooling=False",
                    "Safety__AllowPrivateNetworkTargets": str(allow_private).lower()})
        stdout = (output / f"app-{suffix}-stdout.txt").open("w", encoding="utf-8")
        stderr = (output / f"app-{suffix}-stderr.txt").open("w", encoding="utf-8")
        log_handles.extend([stdout, stderr])
        command = ["dotnet", str(app_dll), "--urls", base, "--contentRoot", str(repo / "SecureApiQualityPlatform.Web")]
        app_process = subprocess.Popen(command, cwd=repo / "SecureApiQualityPlatform.Web", env=env, stdout=stdout, stderr=stderr,
                                       creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0))
        processes.append({"pid": app_process.pid, "startedUtc": utc(), "command": command, "database": str(database),
                          "privateTargetsEnabledForOwnedLoopbackFixtureOnly": allow_private})
        for _ in range(100):
            if app_process.poll() is not None:
                raise RuntimeError(f"Application stopped on startup: {app_process.returncode}")
            try:
                with client().open(base, timeout=1) as response:
                    if response.code == 200:
                        return
            except (URLError, TimeoutError):
                pass
            time.sleep(0.1)
        raise RuntimeError("Application did not become ready within 10 seconds.")

    def stop_app():
        nonlocal app_process
        if app_process is not None:
            if app_process.poll() is None:
                app_process.terminate()
                try:
                    app_process.wait(timeout=10)
                except subprocess.TimeoutExpired:
                    app_process.kill()
                    app_process.wait(timeout=5)
            processes[-1].update({"stoppedUtc": utc(), "exitCode": app_process.returncode})
            app_process = None

    error = None
    try:
        fixture = ThreadingHTTPServer(("127.0.0.1", args.fixture_port), FixtureHandler)
        threading.Thread(target=fixture.serve_forever, daemon=True).start()
        start_app("first", allow_private=False)
        status, html, _ = request(anonymous, "/", label="empty-dashboard")
        check("Empty persisted dashboard returns N/A", status == 200 and html.count("N/A") >= 2 and not rows("Apis"), {"http": status, "naCount": html.count("N/A")}, "200, >=2 N/A, no APIs")
        status, _, _ = request(anonymous, "/Apis/Create", label="viewer-create-denied")
        check("Viewer denied registration GET", status == 302, status, 302)
        status, _, _ = post(anonymous, "/Apis/Create", {"Name": "Denied", "BaseUrl": fixture_base}, label="viewer-post-denied")
        check("Viewer denied registration POST without mutation", status == 302 and not rows("Apis"), {"http": status, "apis": len(rows("Apis"))}, "302, 0 APIs")
        role(admin, "Admin")
        role(tester, "Tester")
        status, _, _ = request(tester, "/Apis/Create", label="tester-create-denied")
        check("Tester denied Admin registration action", status == 302, status, 302)
        for label, value, explanation in (("invalid-scheme", "ftp://fixture.invalid", "absolute http:// or https://"),
                                           ("credentials", f"http://synthetic:secret@127.0.0.1:{args.fixture_port}", "credentials")):
            status, html, _ = post(admin, "/Apis/Create", {"Name": "Invalid synthetic API", "BaseUrl": value}, "/Apis/Create", label)
            check(f"Registration rejects {label}", status == 200 and explanation in html and not rows("Apis"), {"http": status, "apis": len(rows("Apis"))}, "validation view, no persistence")
        status, html, _ = post(admin, "/Apis/Create", {"Name": "Blocked loopback", "BaseUrl": fixture_base}, "/Apis/Create", "private-target-rejected")
        check("Default safety rejects loopback registration", status == 200 and "blocked by default" in html and not rows("Apis") and not fixture_requests,
              {"http": status, "apis": len(rows("Apis")), "outboundRequests": len(fixture_requests)}, "200 validation guidance, no registration/outbound request")
        stop_app()
        start_app("fixture-enabled", allow_private=True)
        api_name = 'PostMerge, "Synthetic API"'
        status, _, headers = post(admin, "/Apis/Create", {"Name": api_name, "BaseUrl": fixture_base, "Description": "Owned loopback fixture only"}, "/Apis/Create", "api-created")
        check("Admin API registration persists to SQLite", status == 302 and len(rows("Apis")) == 1, {"http": status, "apis": len(rows("Apis"))}, "302, 1 API")
        api = rows("Apis")[0]
        details = headers["Location"]
        status, html, _ = post(admin, "/Apis/Create", {"Name": "Duplicate", "BaseUrl": fixture_base + "/"}, "/Apis/Create", "duplicate-api")
        check("Duplicate API URL rejected", status == 200 and "already registered" in html and len(rows("Apis")) == 1, status, "200 validation view, unchanged DB")
        fields = {"Name": "Synthetic health", "Path": "/health", "Method": "GET", "ExpectedStatusCode": "200", "MaxResponseTimeMs": "60000", "ExpectedContentType": "application/json", "RequiresAuthentication": "true"}
        status, html, _ = post(admin, f"/Apis/AddEndpoint/{api['Id']}", {**fields, "Path": "/%2e%2e/admin"}, details, "path-rejected")
        check("Encoded traversal rejected via real MVC action", status == 200 and "within the registered API" in html and not rows("Endpoints"), status, "200 validation view, 0 endpoints")
        status, html, _ = post(admin, f"/Apis/AddEndpoint/{api['Id']}", {**fields, "ExpectedStatusCode": "99"}, details, "range-rejected")
        check("Numeric out-of-range rejected", status == 200 and not rows("Endpoints"), status, "200 validation view, 0 endpoints")
        # Record malformed numeric handling separately: it is a pre-existing
        # validation probe and must not prevent the remaining regression checks.
        status, html, _ = post(admin, f"/Apis/AddEndpoint/{api['Id']}", {**fields, "Name": "Malformed numeric probe", "Path": "/numeric-probe", "ExpectedStatusCode": "not-a-number"}, details, "malformed-number")
        malformed = [row for row in rows("Endpoints") if row["Path"] == "/numeric-probe"]
        rejected = status == 200 and not malformed
        findings.append({"id": "ENDPOINT-NUMERIC-BINDING", "status": "PASS" if rejected else "CONFIRMED", "http": status,
                         "expected": "Malformed numeric input rejected without persisting endpoint", "persistedRows": malformed,
                         "sourceLocation": "SecureApiQualityPlatform.Web/Controllers/ApisController.cs:129",
                         "scope": "Investigate existing ModelState.Clear; do not silently fold into accessibility fix."})
        check("Malformed numeric endpoint input rejected", rejected, {"http": status, "persistedRows": malformed}, "validation error, no endpoint persisted", fatal=False)
        status, _, _ = post(admin, f"/Apis/AddEndpoint/{api['Id']}", fields, details, "endpoint-created")
        check("Admin valid endpoint registered", status == 302 and any(row["Path"] == "/health" for row in rows("Endpoints")), status, "302, health endpoint persisted")
        status, html, _ = post(admin, f"/Apis/AddEndpoint/{api['Id']}", fields, details, "duplicate-endpoint")
        check("Duplicate method/path rejected", status == 200 and "already registered" in html, status, "200 duplicate guidance")
        before = len(rows("Endpoints"))
        status, _, _ = post(tester, f"/Apis/AddEndpoint/{api['Id']}", {**fields, "Path": "/tester-denied"}, details, "tester-endpoint-denied")
        check("Tester cannot add endpoint", status == 302 and len(rows("Endpoints")) == before, status, "302, unchanged DB")
        status, _, _ = post(anonymous, f"/Apis/RunChecks/{api['Id']}", {}, details, "viewer-run-denied")
        check("Viewer cannot run checks", status == 302 and not fixture_requests and not rows("CheckResults"), status, "302, no outbound request or result")
        status, _, _ = post(tester, f"/Apis/RunChecks/{api['Id']}", {}, details, "tester-run-failure")
        check("Tester executes owned fixture and persists failures", status == 302 and len(fixture_requests) == len(rows("Endpoints")) and len(rows("CheckResults")) > 0 and len(rows("Defects")) > 0, {"http": status, "fixtureRequests": len(fixture_requests), "results": len(rows("CheckResults")), "defects": len(rows("Defects"))}, "302, one fixture GET per endpoint, persisted results/defects")
        health = next(row for row in rows("Endpoints") if row["Path"] == "/health")
        defect = next(row for row in rows("Defects") if row["EndpointId"] == health["Id"] and row["CheckType"] == "Expected status")
        mutation = {"id": defect["Id"], "status": "Resolved", "priority": "Urgent", "assignedTo": 'Synthetic, "Tester"'}
        status, _, _ = post(anonymous, "/Defects/Update", mutation, "/Defects", "viewer-update-denied")
        unchanged = next(row for row in rows("Defects") if row["Id"] == defect["Id"])
        check("Viewer cannot mutate defect", status == 302 and unchanged["Status"] == defect["Status"], status, "302, unchanged defect status")
        status, _, _ = post(tester, "/Defects/Update", mutation, "/Defects", "tester-update")
        changed = next(row for row in rows("Defects") if row["Id"] == defect["Id"])
        check("Tester assignment/priority/status update persists", status == 302 and changed["Status"] == 4 and changed["Priority"] == 3 and changed["AssignedTo"] == mutation["assignedTo"], changed, "Resolved, Urgent, synthetic assignee")
        fixture_state["status"] = 200
        status, _, _ = post(tester, f"/Apis/RunChecks/{api['Id']}", {}, details, "tester-passing-retest")
        closed = next(row for row in rows("Defects") if row["Id"] == defect["Id"])
        check("Passing retest closes resolved expected-status defect", status == 302 and closed["Status"] == 6 and closed["RetestedAtUtc"] is not None, closed, "Closed with RetestedAtUtc")
        status, html, _ = request(tester, "/", label="populated-dashboard")
        check("Populated SQLite dashboard renders", status == 200 and "Open defects" in html and "Recent checks" in html and "PASS" in html and "FAIL" in html, status, "200, metrics and mixed result rows")
        for path, columns, predicate, label in (
            (f"/Reports/ExportResultsCsv?apiId={api['Id']}&passed=false", 15, lambda row: row["ApiId"].lower() == api["Id"].lower() and row["Passed"] == "False", "failed-results"),
            (f"/Reports/ExportDefectsCsv?apiId={api['Id']}&status=Closed&severity=High", 21, lambda row: row["ApiId"].lower() == api["Id"].lower() and row["Status"] == "Closed" and row["Severity"] == "High" and row["IsOpen"] == "False", "closed-high-defects")):
            status, body, headers = request(tester, path, label=label)
            reader = csv.DictReader(io.StringIO(body))
            exported = list(reader)
            (output / f"{label}.csv").write_text(body, encoding="utf-8", newline="")
            check(f"{label} CSV schema/filter/escaping", status == 200 and "text/csv" in headers["Content-Type"] and len(reader.fieldnames) == columns and bool(exported) and all(predicate(row) and row["ApiName"] == api_name and len(row) == columns for row in exported), {"http": status, "columns": len(reader.fieldnames), "rows": len(exported)}, f"200 CSV, {columns} columns, filtered rows, quoted API name round-trips")
        for query in ("/Reports/ExportResultsCsv?passed=wrong", "/Reports/ExportDefectsCsv?severity=999"):
            status, html, _ = request(tester, query, label="bad-filter")
            check(f"Malformed filter rejected: {query}", status == 400 and "Invalid report filter" in html, status, "400 with guidance")
        before_restart = snapshot()
        save("database-before-restart.json", before_restart)
        stop_app()
        start_app("restart")
        after_restart = snapshot()
        save("database-after-restart.json", after_restart)
        check("All API/endpoint/result/defect data survives process restart", before_restart == after_restart, {table: len(values) for table, values in after_restart.items()}, "Identical rows across fresh process")
        status, html, _ = request(tester, details, label="details-after-restart")
        check("Persisted API details render after restart", status == 200 and "Synthetic health" in html and "Returned expected HTTP 200" in html, status, "200 and stored endpoints/results visible")
    except Exception as exception:
        error = f"{type(exception).__name__}: {exception}"
        print(error, file=sys.stderr)
    finally:
        stop_app()
        if fixture is not None:
            fixture.shutdown()
            fixture.server_close()
        for handle in log_handles:
            handle.close()
        save("http-requests.json", requests)
        save("owned-fixture-requests.json", fixture_requests)
        save("processes.json", processes)
        save("findings.json", findings)
        save("checks.json", checks)
        try:
            sha = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=repo, text=True).strip()
            git_status = subprocess.check_output(["git", "status", "--short"], cwd=repo, text=True)
        except Exception:
            sha, git_status = None, "unavailable"
        source_hashes = {}
        for file in (repo / "SecureApiQualityPlatform.Web").rglob("*"):
            if file.is_file() and file.suffix in (".cs", ".cshtml", ".css", ".json", ".csproj") and not {"bin", "obj"}.intersection(file.relative_to(repo).parts):
                source_hashes[file.relative_to(repo).as_posix()] = hashlib.sha256(file.read_bytes()).hexdigest()
        save("source-identity.json", {"head": sha, "workingTreeStatus": git_status, "appDllSha256": hashlib.sha256(app_dll.read_bytes()).hexdigest(), "applicationSourceSha256": source_hashes})
        save("summary.json", {"startedUtc": started, "finishedUtc": utc(), "boundary": "Real HTTP MVC and normal SQLite store; no browser or external service; private-target override scoped to synthetic loopback application process", "python": sys.version,
                              "appUrl": base, "fixtureUrl": fixture_base, "database": str(database), "passed": sum(c["status"] == "PASS" for c in checks), "failed": sum(c["status"] == "FAIL" for c in checks),
                              "confirmedFindings": sum(f["status"] == "CONFIRMED" for f in findings), "error": error,
                              "status": "FAIL" if error or any(f["status"] == "CONFIRMED" for f in findings) else "PASS"})
    return 1 if error or any(f["status"] == "CONFIRMED" for f in findings) else 0


if __name__ == "__main__":
    raise SystemExit(main())
