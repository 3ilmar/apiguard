"""Check evidence consistency and links; this does not execute product tests."""
import csv
import hashlib
import json
import re
import subprocess
from datetime import datetime, timezone
from pathlib import Path

root = Path(__file__).resolve().parent
repo = root.parents[3]
load = lambda name: json.loads((root / name).read_text(encoding="utf-8-sig"))
comparison = load("comparison.json")
assert comparison["postfixBrowsers"]["counts"] == {"PASS": 16, "FAIL": 0, "BLOCKED": 0, "NOT RUN": 0}
assert comparison["postfixTargets"]["status"] == "PASS"
assert comparison["historicalEvidence"]["changed"] == []
for name in ("baselineSuite", "postfixSuite"):
    counters = comparison[name]["counters"]
    assert counters["passed"] == counters["executed"] == counters["total"] == "136"
    assert counters["failed"] == counters["notExecuted"] == "0"
for name in ("baselineHttp", "postfixHttp"):
    assert comparison[name]["passed"] == 28 and comparison[name]["failed"] == 1
assert comparison["csvValidation"] == {"PASS": 4, "FAIL": 0, "BLOCKED": 0}

before = list(csv.DictReader((root / "initial-register.csv").open(encoding="utf-8-sig", newline="")))
after = list(csv.DictReader((repo / "docs/ShiruiFinalTestCases.csv").open(encoding="utf-8-sig", newline="")))
assert len(before) == len(after) == 22
assert all(a == b for a, b in zip(before, after) if a["ID"] != "SQA-20")
assert next(r for r in before if r["ID"] == "SQA-20")["Status"] == "FAIL"
assert next(r for r in after if r["ID"] == "SQA-20")["Status"] == "PASS"

source = list(csv.DictReader((root / "baseline-source-sha256.csv").open(encoding="utf-8-sig", newline="")))
changed_app = sorted(row["Path"] for row in source if row["Path"].startswith("SecureApiQualityPlatform.Web/") and
                     hashlib.sha256((repo / row["Path"]).read_bytes()).hexdigest().lower() != row["Sha256"].lower())
assert changed_app == ["SecureApiQualityPlatform.Web/Views/Apis/Details.cshtml",
                       "SecureApiQualityPlatform.Web/Views/Defects/Index.cshtml",
                       "SecureApiQualityPlatform.Web/wwwroot/css/site.css"]
final_source = list(csv.DictReader((root / "final-source-sha256.csv").open(encoding="utf-8-sig", newline="")))
assert all(hashlib.sha256((repo / row["Path"]).read_bytes()).hexdigest().lower() == row["Sha256"].lower() for row in final_source)
missing_links = []
for document in (root / "README.md", repo / "docs/ShiruiQualityMetrics-Evidence.md", repo / "docs/DefectLog.md"):
    for target in re.findall(r"\]\(([^)]+)\)", document.read_text(encoding="utf-8-sig")):
        if "://" not in target and not target.startswith("#") and not (document.parent / target.split("#")[0]).exists():
            missing_links.append({"document": str(document.relative_to(repo)), "target": target})
assert not missing_links, missing_links
assert subprocess.run(["git", "diff", "--check"], cwd=repo).returncode == 0
assert subprocess.run(["git", "diff", "--cached", "--quiet"], cwd=repo).returncode == 0
state = load("final-state.json")
assert state["mainUnchanged"] and not state["testListeners"] and not state["committed"] and not state["pushed"]
result = {"checkedAtUtc": datetime.now(timezone.utc).isoformat(), "purpose": "Evidence consistency only; no product test executed",
          "status": "PASS", "sourceFilesChecked": len(final_source), "changedApplicationFiles": changed_app,
          "historicalEvidence": comparison["historicalEvidence"], "masterRegister": load("register-update.json"),
          "missingLocalLinks": missing_links, "stagedChanges": False}
(root / "evidence-verification.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
files = []
for path in sorted(root.rglob("*")):
    if path.is_file() and path.name != "evidence-index.json" and not path.name.endswith((".db", ".db-shm", ".db-wal", ".log")):
        files.append({"path": path.relative_to(root).as_posix(), "bytes": path.stat().st_size,
                      "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
(root / "evidence-index.json").write_text(json.dumps({"files": files, "excluded": ["self", "ignored binary SQLite databases", "duplicate ignored .log files; console/execution-log.txt retained"]}, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"evidenceConsistency": result["status"], "indexedFiles": len(files), "sourceFilesChecked": len(final_source),
                  "historicalFilesUnchanged": comparison["historicalEvidence"]["checkedFiles"]}))
