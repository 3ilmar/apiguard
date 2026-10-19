"""Record the explicitly authorised SQA-19 outcome; preserve other CSV rows."""
import collections
import csv
import io
import json
from pathlib import Path

root = Path(__file__).resolve().parent
repo = root.parents[3]
path = repo / "docs/ShiruiFinalTestCases.csv"
original = (root / "baseline-register.csv").read_bytes()
assert path.read_bytes() == original, "Review changed register before updating"
lines = original.decode("utf-8-sig").splitlines(keepends=True)
index = next(i for i, line in enumerate(lines) if line.startswith("SQA-19,"))
row = next(csv.reader([lines[index]], strict=True))
row[7] = ("PASS — observer approved. Shirui explicitly reviewed and approved all five scenario outcomes in this conversation: "
          "5/5 interpretations and supporting explanations accepted for EILMAR-P01, session 19 October 2026 15:00-16:00 NZDT. "
          "ChatGPT assisted wording after tester-reported interpretation; original unpolished answers and a session-specific screenshot were not supplied. "
          "Some aspects of the unaided session cannot be independently corroborated. Original responses preserved unchanged. "
          "No signature or precise approval timestamp supplied or inferred.")
row[8] = "PASS"
buffer = io.StringIO(newline="")
csv.writer(buffer, lineterminator="\r\n" if lines[index].endswith("\r\n") else "\n").writerow(row)
lines[index] = buffer.getvalue()
updated = "".join(lines).encode("utf-8")
path.write_bytes(updated)
before = list(csv.DictReader(io.StringIO(original.decode("utf-8-sig"))))
after = list(csv.DictReader(io.StringIO(updated.decode("utf-8-sig"))))
assert all(a == b for a, b in zip(before, after) if a["ID"] != "SQA-19")
counts = collections.Counter(row["Status"] for row in after)
planned = len(after)
executed = sum(count for status, count in counts.items() if status.startswith("PASS") or status == "FAIL")
totals = {"planned": planned, "executed": executed, "passed": counts["PASS"],
          "provisionalPass": counts["PASS (PROVISIONAL)"], "failed": counts["FAIL"],
          "blocked": counts["BLOCKED"], "notRun": counts["NOT RUN"],
          "executionRatePercent": round(100 * executed / planned, 2),
          "passRatePercent": round(100 * counts["PASS"] / executed, 2),
          "statusCounts": dict(counts), "otherOutcomesUnchanged": True,
          "boundary": "Master acceptance/evidence cases, not MSTest or HTTP check counts"}
(root / "qa-totals.json").write_text(json.dumps(totals, indent=2) + "\n", encoding="utf-8")
print(json.dumps(totals))
