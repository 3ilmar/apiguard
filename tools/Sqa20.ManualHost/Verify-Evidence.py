"""Check artifact references, preservation, source scope and credential patterns."""
import argparse
from collections import Counter
import csv
import hashlib
import json
from pathlib import Path
import re
import struct
import subprocess


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write(path, data):
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("session", type=Path)
    args = parser.parse_args()
    session = args.session.resolve()
    root = Path(__file__).resolve().parents[2]
    base = session.as_posix().lower()

    # Portable artifact references; retain baseline bytes and external environment paths.
    def portable(value):
        if isinstance(value, dict):
            return {key: portable(item) for key, item in value.items()}
        if isinstance(value, list):
            return [portable(item) for item in value]
        if isinstance(value, str):
            normalized = value.replace("\\", "/")
            if normalized.lower().startswith(base + "/"):
                return normalized[len(base) + 1:]
        return value
    for file in session.rglob("*.json"):
        if "baseline" in file.relative_to(session).parts:
            continue
        before = load(file)
        after = portable(before)
        if before != after:
            write(file, after)

    checks = []
    def check(name, condition, actual):
        checks.append({"name": name, "passed": bool(condition), "actual": actual})
    branch = subprocess.check_output(["git", "branch", "--show-current"], cwd=root, text=True).strip()
    check("requested_branch", branch == "qa/sqa19-sqa20-manual-validation", branch)
    source_diff = subprocess.run(["git", "diff", "--exit-code", "--", "SecureApiQualityPlatform.Web", "SecureApiQualityPlatform.Tests"], cwd=root, capture_output=True)
    check("application_and_dotnet_tests_unchanged", source_diff.returncode == 0, "git diff on both project directories")
    staged = subprocess.check_output(["git", "diff", "--cached", "--name-only"], cwd=root, text=True).strip()
    check("no_staged_changes", not staged, staged)
    tracked_changes = subprocess.check_output(["git", "diff", "--name-only"], cwd=root, text=True).splitlines()
    check("tracked_scope", set(tracked_changes) <= {"docs/ShiruiFinalTestCases.csv", "docs/ShiruiQualityMetrics-Evidence.md"}, tracked_changes)

    baseline = list(csv.DictReader((session / "baseline/existing-evidence-sha256.csv").read_text(encoding="utf-8-sig").splitlines()))
    changed = []
    for item in baseline:
        file = root / item["Path"]
        if not file.exists() or digest(file).lower() != item["SHA256"].lower():
            changed.append(item["Path"].replace("\\", "/"))
    allowed = {f"docs/qa-evidence/SQA-20/{file}" for file in ("README.md", "observation-sheet.csv", "observations/README.md", "screenshots/README.md", "downloads/README.md")}
    check("existing_evidence_preserved_except_authorized_current_records", set(changed) <= allowed, changed)
    check("existing_sqa19_evidence_unchanged", not any("/SQA-19/" in file for file in changed), "All baseline-hashed SQA-19 artifacts; locked live host logs explicitly excluded before testing.")
    original_register = subprocess.check_output(["git", "show", "HEAD:docs/ShiruiFinalTestCases.csv"], cwd=root)
    current_register = (root / "docs/ShiruiFinalTestCases.csv").read_bytes()
    old_sqa19 = next(line for line in original_register.splitlines(keepends=True) if line.startswith(b"SQA-19,"))
    new_sqa19 = next(line for line in current_register.splitlines(keepends=True) if line.startswith(b"SQA-19,"))
    preservation = load(session / "sqa19-register-preserved.json")
    same_content = old_sqa19.rstrip(b"\r\n") == new_sqa19.rstrip(b"\r\n")
    same_working_bytes = preservation["sha256"] == hashlib.sha256(new_sqa19).hexdigest()
    check("sqa19_master_case_unchanged", same_content and same_working_bytes, {"sha256": hashlib.sha256(new_sqa19).hexdigest(), "workingTreeBytesUnchanged": same_working_bytes, "gitContentComparison": "Line endings normalized because Git stores LF while this working-tree CSV has CRLF."})

    summary = load(session / "browser-execution.json")
    with (session.parent / "observation-sheet.csv").open(encoding="utf-8", newline="") as stream:
        rows = list(csv.DictReader(stream))
    check("all_16_explicitly_automated", len(rows) == 16 and all(row["Execution method"].startswith("AUTOMATED") for row in rows), len(rows))
    check("scoped_case_totals_match", dict(Counter(row["Execution status"] for row in rows)) == {key: value for key, value in summary["counts"].items() if value}, summary["counts"])
    check("overall_accessibility_failure_explicit", summary["overallAutomatedStatus"] == "FAIL" and all(row["Additional accessibility status"].startswith("FAIL") for row in rows), "16 technical PASS; additional accessibility FAIL")
    observations_missing = []
    for row in rows:
        for reference in row["Screenshot or download reference"].split(";"):
            reference = reference.strip()
            if reference and not (session.parent / reference).exists():
                observations_missing.append(reference)
    check("observation_evidence_references", not observations_missing, observations_missing)

    artifact_extensions = {".png", ".jpg", ".jpeg", ".json", ".csv", ".trx", ".txt", ".cjs", ".py", ".md", ".xml"}
    reference_keys = {"evidence", "evidencePath", "keyboardEvidence", "screenshot", "screenshots", "path"}
    missing = []
    checked = set()
    def references(value, key=""):
        if isinstance(value, dict):
            for field, item in value.items():
                references(item, field)
        elif isinstance(value, list):
            for item in value:
                references(item, key)
        elif isinstance(value, str) and key in reference_keys and not value.startswith(("http:", "https:", "C:", "c:", "/")):
            if Path(value).suffix.lower() in artifact_extensions and "\n" not in value:
                checked.add(value)
                if not (session / value).exists():
                    missing.append(value)
    for file in session.rglob("*.json"):
        if "baseline" not in file.relative_to(session).parts and file.name not in ("evidence-validation.json", "evidence-index.json"):
            references(load(file))
    check("machine_evidence_references", not missing, {"checked": len(checked), "missing": sorted(set(missing))})
    markdown_missing = []
    documents = list(session.glob("*.md")) + [session.parent / "README.md", root / "docs/ShiruiQualityMetrics-Evidence.md"]
    markdown_links = 0
    for file in documents:
        for link in re.findall(r"\[[^\]]*\]\(([^)]+)\)", file.read_text(encoding="utf-8")):
            if link.startswith(("http:", "https:", "#")):
                continue
            target = link.split("#", 1)[0].strip("<>")
            markdown_links += 1
            if not (file.parent / target).exists():
                markdown_missing.append({"file": file.relative_to(root).as_posix(), "link": link})
    check("markdown_evidence_references", not markdown_missing, {"checked": markdown_links, "missing": markdown_missing})

    pngs = []
    invalid_images = []
    for file in sorted(session.rglob("*.png")):
        data = file.read_bytes()
        if data[:8] != b"\x89PNG\r\n\x1a\n":
            invalid_images.append(file.relative_to(session).as_posix())
            continue
        width, height = struct.unpack(">II", data[16:24])
        pngs.append({"path": file.relative_to(session).as_posix(), "width": width, "height": height})
    check("real_png_files_and_expected_widths", bool(pngs) and not invalid_images and all(image["width"] in (375, 1440) for image in pngs), {"images": len(pngs), "invalid": invalid_images})
    downloads = list(session.glob("*/downloads/*.csv"))
    check("four_original_browser_downloads", len(downloads) == 4, [file.relative_to(session).as_posix() for file in downloads])
    validation = load(session / "csv-validation.json")
    check("all_downloads_semantically_validated", validation["totals"] == {"PASS": 4, "FAIL": 0, "BLOCKED": 0}, validation["totals"])
    suite = load(session / "dotnet/test-summary.json")
    check("dotnet_actual_counters", suite["counters"]["total"] == 71 and suite["counters"]["passed"] == 71 and suite["counters"]["failed"] == 0 and suite["counters"]["notExecuted"] == 0, suite["counters"])

    # Candidate-pattern audit only: no credential values are printed or copied.
    patterns = {
        "private_key_block": re.compile(r"-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----"),
        "github_access_token": re.compile(r"\bgh[pousr]_[A-Za-z0-9]{36,}\b|\bgithub_pat_[A-Za-z0-9_]{50,}\b"),
        "aws_access_key_id": re.compile(r"\bAKIA[A-Z0-9]{16}\b"),
        "openai_style_key": re.compile(r"\bsk-(?:proj-)?[A-Za-z0-9_-]{40,}\b")
    }
    candidates = []
    text_files = [file for file in session.rglob("*") if file.is_file() and file.suffix.lower() not in (".png", ".jpg", ".jpeg")]
    text_files += [file for file in (root / "tools/Sqa20.ManualHost").rglob("*") if file.is_file() and not any(part in ("bin", "obj") for part in file.relative_to(root / "tools/Sqa20.ManualHost").parts)]
    for file in text_files:
        text = file.read_text(encoding="utf-8", errors="ignore")
        for name, pattern in patterns.items():
            if pattern.search(text):
                candidates.append({"file": file.relative_to(root).as_posix(), "pattern": name})
    check("credential_candidate_scan", not candidates, {"textFilesScanned": len(text_files), "candidates": candidates, "limitation": "Pattern audit of this execution and its tools; not a claim of exhaustive secret detection. Existing locked live-host logs were not read."})

    report = {"method": "Filesystem/reference/hash/source-scope validation; browser outcomes were not rerun or changed", "checks": checks, "passed": all(item["passed"] for item in checks), "imageIndex": pngs}
    write(session / "evidence-validation.json", report)
    index = [{"path": file.relative_to(session).as_posix(), "bytes": file.stat().st_size, "sha256": digest(file)} for file in sorted(session.rglob("*")) if file.is_file() and file.name != "evidence-index.json"]
    write(session / "evidence-index.json", {"files": index, "fileCount": len(index), "bytes": sum(item["bytes"] for item in index)})
    print(json.dumps({"passed": report["passed"], "checks": len(checks), "images": len(pngs), "files": len(index), "failed": [item for item in checks if not item["passed"]]}))
    if not report["passed"]:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
