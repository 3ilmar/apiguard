"""Validate original SQA-20 browser downloads without modifying their bytes.

Usage: python validate_browser_csv.py <evidence-session-directory>
Writes csv-validation.json. A results export omits its result ID, so an exact
SourceCheckResultId foreign-key join is explicitly outside this validator.
"""
import argparse
import collections
import csv
import datetime
import hashlib
import io
import json
import re
import uuid
from pathlib import Path


RESULTS_HEADER = "ExecutedAtUtc,ApiName,ApiId,EndpointName,EndpointId,Method,Path,CheckType,Passed,ActualStatusCode,ResponseTimeMs,FailureSeverity,Message,ApiBaseUrl,Outcome".split(",")
DEFECTS_HEADER = "DefectId,ApiName,ApiId,EndpointName,EndpointId,Method,Path,CheckType,Severity,Priority,Status,AssignedTo,CreatedAtUtc,UpdatedAtUtc,RetestedAtUtc,SourceCheckResultId,Title,Description,ApiBaseUrl,LifecycleStage,IsOpen".split(",")
ORDERS_API = 'SQA-20 Orders, "review"'
CATALOGUE_API = 'SQA-20 Catalogue, "healthy"'
ORDERS_ENDPOINT = 'Orders, "manual review" endpoint with a deliberately long descriptive label'
CATALOGUE_ENDPOINT = 'Catalogue, "passing" endpoint'
DISCLOSURE = 'Server header disclosed: SQA-20 fixture, "synthetic" server.'
SEVERITY_STATUS = {"Critical": "New", "High": "Assigned", "Medium": "InProgress", "Low": "Retest"}


def check(output, name, condition, actual):
    output["checks"].append({"name": name, "passed": bool(condition), "actual": actual})


def validate_file(session, browser, kind):
    prefix = "api-check-results" if kind == "results" else "defects"
    header = RESULTS_HEADER if kind == "results" else DEFECTS_HEADER
    directory = session / browser / "downloads"
    files = sorted(directory.glob(prefix + "-*.csv"))
    output = {"status": "BLOCKED", "checks": [], "files": [], "limitations": []}
    if not files:
        output["reason"] = "Original browser download was not found."
        return output, []
    check(output, "one_original_download", len(files) == 1, [p.name for p in files])
    rows = []
    for path in files:
        data = path.read_bytes()
        output["files"].append({"path": path.relative_to(session).as_posix(), "bytes": len(data), "sha256": hashlib.sha256(data).hexdigest()})
        check(output, "filename_pattern", re.fullmatch(re.escape(prefix) + r"-\d{8}-\d{6}\.csv", path.name) is not None, path.name)
        try:
            decoded = data.decode("utf-8", errors="strict")
            parsed = list(csv.reader(io.StringIO(decoded, newline=""), strict=True))
        except (UnicodeDecodeError, csv.Error) as error:
            check(output, "strict_utf8_and_csv_parse", False, str(error))
            continue
        check(output, "strict_utf8_and_csv_parse", True, "UTF-8 decoded and csv.reader(strict=True) parsed logical records.")
        check(output, "exact_header", bool(parsed) and parsed[0] == header, parsed[0] if parsed else [])
        widths = [len(row) for row in parsed]
        check(output, "logical_record_column_counts", bool(parsed) and all(width == len(header) for width in widths), widths)
        if not parsed or parsed[0] != header or any(len(row) != len(header) for row in parsed):
            continue
        records = [dict(zip(header, row)) for row in parsed[1:]]
        rows.extend(records)
    check(output, "data_record_count", len(rows) == (8 if kind == "results" else 4), len(rows))
    if kind == "results":
        totals = collections.Counter((row["Passed"], row["Outcome"]) for row in rows)
        check(output, "four_pass_four_fail_with_matching_outcomes", totals == {("True", "Pass"): 4, ("False", "Fail"): 4}, {"|".join(key): value for key, value in totals.items()})
        api_counts = collections.Counter(row["ApiName"] for row in rows)
        endpoint_counts = collections.Counter(row["EndpointName"] for row in rows)
        check(output, "api_comma_and_quotes_preserved", api_counts == {ORDERS_API: 4, CATALOGUE_API: 4}, dict(api_counts))
        check(output, "endpoint_comma_and_quotes_preserved", endpoint_counts == {ORDERS_ENDPOINT: 4, CATALOGUE_ENDPOINT: 4}, dict(endpoint_counts))
        disclosures = [row["Message"] for row in rows if row["CheckType"] == "Server disclosure" and row["Passed"] == "False"]
        check(output, "synthetic_disclosure_text_preserved", disclosures == [DISCLOSURE], disclosures)
    else:
        severities = collections.Counter(row["Severity"] for row in rows)
        check(output, "one_of_each_severity", severities == {value: 1 for value in SEVERITY_STATUS}, dict(severities))
        statuses = [{"severity": row["Severity"], "status": row["Status"], "lifecycle": row["LifecycleStage"], "isOpen": row["IsOpen"]} for row in rows]
        check(output, "fixture_status_and_lifecycle", bool(rows) and all(row["Status"] == SEVERITY_STATUS.get(row["Severity"]) and row["LifecycleStage"] == ("Verification" if row["Severity"] == "Low" else "Active") and row["IsOpen"] == "True" for row in rows), statuses)
        check(output, "api_comma_and_quotes_preserved", bool(rows) and all(row["ApiName"] == ORDERS_API for row in rows), sorted({row["ApiName"] for row in rows}))
        check(output, "endpoint_comma_and_quotes_preserved", bool(rows) and all(row["EndpointName"] == ORDERS_ENDPOINT for row in rows), sorted({row["EndpointName"] for row in rows}))
        disclosures = [row["Description"] for row in rows if row["CheckType"] == "Server disclosure"]
        check(output, "synthetic_disclosure_text_preserved", disclosures == [DISCLOSURE], disclosures)
        source_ids = [row["SourceCheckResultId"] for row in rows]
        try:
            valid_ids = all(uuid.UUID(value).int != 0 for value in source_ids)
        except (ValueError, AttributeError):
            valid_ids = False
        check(output, "source_result_ids_nonempty_valid_unique", bool(rows) and valid_ids and len(set(source_ids)) == len(source_ids), source_ids)
        output["limitations"].append("Results CSV has no result ID column: exact SourceCheckResultId foreign-key equality cannot be established from these CSV exports. Matching failed result contents are checked separately.")
    output["status"] = "PASS" if all(item["passed"] for item in output["checks"]) else "FAIL"
    return output, rows


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("session", type=Path)
    args = parser.parse_args()
    session = args.session.resolve()
    if not session.is_dir():
        parser.error("Session directory does not exist.")
    report = {"generatedAtUtc": datetime.datetime.now(datetime.timezone.utc).isoformat(), "method": "Python stdlib strict UTF-8 and CSV validation of original browser downloads", "browsers": {}}
    for browser in ("chrome", "edge"):
        results, result_rows = validate_file(session, browser, "results")
        defects, defect_rows = validate_file(session, browser, "defects")
        if defects["status"] != "BLOCKED":
            failed = {(row["ApiId"], row["EndpointId"], row["CheckType"], row["Message"]) for row in result_rows if row["Passed"] == "False"}
            actual = [(row["ApiId"], row["EndpointId"], row["CheckType"], row["Description"]) for row in defect_rows]
            if results["status"] == "BLOCKED":
                defects["limitations"].append("Matching failed result content could not be validated: results download missing.")
            else:
                check(defects, "matches_same_browser_failed_result_content", len(actual) == 4 and len(set(actual)) == 4 and set(actual) == failed, {"failedResultKeys": len(failed), "defectKeys": len(set(actual))})
                defects["status"] = "PASS" if all(item["passed"] for item in defects["checks"]) else "FAIL"
        report["browsers"][browser] = {"results": results, "defects": defects}
    counts = collections.Counter(item["status"] for browser in report["browsers"].values() for item in browser.values())
    report["totals"] = {status: counts[status] for status in ("PASS", "FAIL", "BLOCKED")}
    output_path = session / "csv-validation.json"
    output_path.write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps({"report": str(output_path), "totals": report["totals"]}))


if __name__ == "__main__":
    main()
