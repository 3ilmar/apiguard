"""Validate SQA-19 documentation consistency; no product tests are executed."""
import collections
import csv
import hashlib
import json
import re
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path

root = Path(__file__).resolve().parent
repo = root.parents[3]
base = (root / "base-commit.txt").read_text(encoding="utf-8-sig").strip()
checks = []


def check(name, condition, actual):
    checks.append({"check": name, "passed": bool(condition), "actual": actual})


def git(*args):
    return subprocess.check_output(["git", *args], cwd=repo)


def sha(data):
    return hashlib.sha256(data).hexdigest()


path = repo / "docs/ShiruiFinalTestCases.csv"
with path.open(encoding="utf-8-sig", newline="") as stream:
    records = list(csv.reader(stream, strict=True))
with (root / "baseline-register.csv").open(encoding="utf-8-sig", newline="") as stream:
    old_records = list(csv.reader(stream, strict=True))
check("strict_csv_schema_and_widths", records[0] == old_records[0] and len(records[0]) == 10 and
      all(len(row) == 10 for row in records), {"header": records[0], "recordWidths": sorted(set(map(len, records)))})
rows = [dict(zip(records[0], row)) for row in records[1:]]
ids = [row["ID"] for row in rows]
check("case_ids_unique_complete", len(ids) == len(set(ids)) and set(ids) == {f"SQA-{i:02}" for i in range(1, 23)}, ids)
changed = [new[0] for old, new in zip(old_records[1:], records[1:]) if old != new]
check("only_sqa19_outcome_changed", len(old_records) == len(records) and changed == ["SQA-19"], changed)
by_id = {row["ID"]: row for row in rows}
check("sqa19_final_status", by_id["SQA-19"]["Status"] == "PASS" and
      "PASS — observer approved" in by_id["SQA-19"]["Actual result"], by_id["SQA-19"]["Actual result"])
for case in ("SQA-18", "SQA-20"):
    old = next(row for row in old_records[1:] if row[0] == case)
    current = next(row for row in records[1:] if row[0] == case)
    check(case + "_record_preserved", old == current, by_id[case]["Status"])
counts = collections.Counter(row["Status"] for row in rows)
executed = sum(value for status, value in counts.items() if status.startswith("PASS") or status == "FAIL")
totals = {"planned": len(rows), "executed": executed, "passed": counts["PASS"],
          "provisionalPass": counts["PASS (PROVISIONAL)"], "failed": counts["FAIL"],
          "blocked": counts["BLOCKED"], "notRun": counts["NOT RUN"],
          "executionRatePercent": round(100 * executed / len(rows), 2),
          "passRatePercent": round(100 * counts["PASS"] / executed, 2)}
saved = json.loads((root / "qa-totals.json").read_text())
check("saved_totals_match_register", all(saved[key] == value for key, value in totals.items()), totals)

observation = "docs/qa-evidence/SQA-19/observation-sheet-session-eilmar.md"
baseline_hashes = list(csv.DictReader((root / "baseline-evidence-sha256.csv").open(encoding="utf-8-sig", newline="")))
original_hash = next(row["Sha256"] for row in baseline_hashes if row["Path"] == observation)
current_hash = sha((repo / observation).read_bytes())
check("human_submission_unchanged", current_hash.lower() == original_hash.lower() and
      git("show", f"{base}:{observation}") == git("show", f":{observation}"),
      {"baselineWorkingSha256": original_hash.lower(), "currentWorkingSha256": current_hash,
       "baselineGitBlobSha256": sha(git("show", f"{base}:{observation}"))})
allowed_history_changes = {"docs/qa-evidence/SQA-19/README.md", "docs/qa-evidence/SQA-19/observer-assessment-session-eilmar.md"}
changed_history = [row["Path"] for row in baseline_hashes if
                   sha((repo / row["Path"]).read_bytes()).lower() != row["Sha256"].lower()]
check("historical_evidence_preserved", set(changed_history) == allowed_history_changes,
      {"filesChecked": len(baseline_hashes), "intentionalCurrentRecordChanges": changed_history})

assessment = repo / "docs/qa-evidence/SQA-19/observer-assessment-session-eilmar.md"
text = assessment.read_text(encoding="utf-8-sig")
section = text.split("## Evidence comparison and approved outcomes", 1)[1].split("For each scenario", 1)[0]
scenario_rows = [line for line in section.splitlines() if re.match(r"\| SQA19-0[1-5] \|", line)]
check("five_scenarios_approved", len(scenario_rows) == 5 and all(line.endswith("| PASS |") for line in scenario_rows),
      {"scenarioRows": len(scenario_rows), "finalOutcomes": [line.rsplit("|", 2)[1].strip() for line in scenario_rows]})
old_assessment = (root / "baseline-observer-assessment.txt").read_text(encoding="utf-8-sig")
old_scenarios = [line.split("|")[3].strip() for line in old_assessment.split("## Evidence comparison", 1)[1].split("For each scenario", 1)[0].splitlines() if re.match(r"\| SQA19-0[1-5] \|", line)]
check("quoted_interpretations_preserved", old_scenarios == [line.split("|")[3].strip() for line in scenario_rows], len(old_scenarios))
disclosures = {"wording_assistance": "ChatGPT compared and lightly polished wording",
               "no_raw_answers": "original unpolished answers", "no_session_screenshot": "No screenshot specifically attributable",
               "unaided_provenance_limit": "cannot independently establish", "no_signature_timestamp": "Not supplied; not invented"}
check("limitations_retained", all(phrase in text for phrase in disclosures.values()),
      {name: phrase in text for name, phrase in disclosures.items()})
current_docs = [repo / name for name in ("docs/qa-evidence/SQA-19/README.md",
                "docs/qa-evidence/SQA-19/observer-assessment-session-eilmar.md",
                "docs/ShiruiQualityMetrics-Evidence.md", "docs/AutomationSuite-Shirui.md")]
obsolete = ("final approval by Shirui PENDING", "PROPOSED 5/5 PASS", "final Shirui approval pending",
            "Shirui final observer approval pending", "provisional approval pending", "await observer approval",
            "final observer approval pending", "SQA-19 has 5/5 provisional")
stale = [{"file": str(document.relative_to(repo)), "phrase": phrase} for document in current_docs
         for phrase in obsolete if phrase.lower() in document.read_text(encoding="utf-8-sig").lower()]
check("no_obsolete_current_approval_wording", not stale, stale)

documents = list((repo / "docs/qa-evidence/SQA-19").rglob("*.md")) + current_docs[2:]
missing = []
links_checked = 0
for document in documents:
    for target in re.findall(r"\]\(([^)]+)\)", document.read_text(encoding="utf-8-sig")):
        if "://" in target or target.startswith("#"):
            continue
        links_checked += 1
        if not (document.parent / target.split("#")[0]).exists():
            missing.append({"file": str(document.relative_to(repo)), "target": target})
check("local_evidence_links_resolve", not missing, {"documents": len(documents), "linksChecked": links_checked, "missing": missing})
for case in ("SQA-18", "SQA-19", "SQA-20"):
    references = by_id[case]["Evidence link or screenshot"].split("; ")
    check(case + "_register_evidence_links", all((repo / "docs" / value).exists() for value in references), references)

changed_paths = git("diff", "--name-only", base).decode().splitlines()
untracked_paths = git("ls-files", "--others", "--exclude-standard").decode().splitlines()
check("documentation_only_diff", all(path.startswith("docs/") for path in changed_paths + untracked_paths), changed_paths + untracked_paths)
check("git_whitespace_check", subprocess.run(["git", "diff", "--check", base], cwd=repo).returncode == 0, "git diff --check " + base)
http = json.loads((repo / "docs/qa-evidence/SQA-20/retest-20261019T095150Z/postfix-http-verified/summary.json").read_text())
check("http_failure_preserved", http["passed"] == 28 and http["failed"] == 1 and http["status"] == "FAIL", {"passed": http["passed"], "failed": http["failed"], "status": http["status"]})
result = {"executedAtUtc": datetime.now(timezone.utc).isoformat(), "purpose": "Automated documentation validation; not human approval time or a new product test run",
          "python": sys.version,
          "baseCommit": base, "branch": git("branch", "--show-current").decode().strip(),
          "status": "PASS" if all(item["passed"] for item in checks) else "FAIL", "checks": checks,
          "totals": totals, "passedChecks": sum(item["passed"] for item in checks), "failedChecks": sum(not item["passed"] for item in checks)}
(root / "validation.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"status": result["status"], "passedChecks": result["passedChecks"], "failedChecks": result["failedChecks"], "totals": totals}))
raise SystemExit(0 if result["status"] == "PASS" else 1)
