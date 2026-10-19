"""Update only SQA-20 after successful observed retest, retaining all other rows."""
import csv
import io
import json
from pathlib import Path

root = Path(__file__).resolve().parent
repo = root.parents[3]
summary = json.loads((root / "comparison.json").read_text())
assert summary["postfixBrowsers"]["overallAutomatedStatus"] == "PASS"
assert summary["postfixTargets"]["status"] == "PASS"
assert summary["historicalEvidence"]["changed"] == []
target = repo / "docs/ShiruiFinalTestCases.csv"
original = (root / "initial-register.csv").read_bytes()
assert target.read_bytes() == original, "Register changed since snapshot; review before editing"
lines = original.decode("utf-8-sig").splitlines(keepends=True)
index = next(i for i, line in enumerate(lines) if line.startswith("SQA-20,"))
row = next(csv.reader([lines[index]]))
row[3] = "Automated browser compatibility/accessibility retest; human acceptance not performed"
row[5] = "Repeat 16 technical categories; Tab/Shift+Tab at desktop and 375px; actual names/label clicks; contrast states; axe scans; original browser CSV parsing"
row[7] = ("19 October 2026 22:59-23:03 NZDT: PASS on automated retest of uncommitted accessibility fixes over 7f28ee9. "
          "16/16 browser categories PASS; 26/26 original controls per browser/viewport correctly labelled; URL contrast 6.59; "
          "export contrast 7.69 resting and 8.18 interactive. Compact badges; 24 page/viewport/browser forward/reverse runs PASS. "
          "Four raw axe scroll-region flags retained; genuine Tab/Shift+Tab and ArrowRight 0-to-40px establish access. "
          "Historical FAIL preserved; no full WCAG/human acceptance claim. Separate pre-existing numeric validation failure remains open.")
row[8] = "PASS"
row[9] = ("qa-evidence/SQA-20/retest-20261019T095150Z/README.md; "
          "qa-evidence/SQA-20/retest-20261019T095150Z/after/browser-execution.json; "
          "qa-evidence/SQA-20/retest-20261019T095150Z/after-targets/accessibility-targets.json; "
          "qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md")
buffer = io.StringIO(newline="")
csv.writer(buffer, lineterminator="\r\n" if lines[index].endswith("\r\n") else "\n").writerow(row)
lines[index] = buffer.getvalue()
updated = "".join(lines).encode("utf-8")
target.write_bytes(updated)
before = list(csv.DictReader(io.StringIO(original.decode("utf-8-sig"))))
after = list(csv.DictReader(io.StringIO(updated.decode("utf-8-sig"))))
assert len(before) == len(after) == 22
assert all(a == b for a, b in zip(before, after) if a["ID"] != "SQA-20")
result = {"planned": len(after), "executed": sum(r["Status"] != "NOT RUN" for r in after),
          "confirmedPass": sum(r["Status"] == "PASS" for r in after),
          "provisionalPass": sum(r["Status"] == "PASS (PROVISIONAL)" for r in after),
          "failed": sum(r["Status"] == "FAIL" for r in after),
          "blocked": sum(r["Status"] == "BLOCKED" for r in after),
          "notRun": sum(r["Status"] == "NOT RUN" for r in after),
          "otherRowsPreserved": True,
          "sqa18Preserved": next(r for r in before if r["ID"] == "SQA-18") == next(r for r in after if r["ID"] == "SQA-18"),
          "sqa19Preserved": next(r for r in before if r["ID"] == "SQA-19") == next(r for r in after if r["ID"] == "SQA-19")}
(root / "register-update.json").write_text(json.dumps(result, indent=2) + "\n")
print(json.dumps(result))
