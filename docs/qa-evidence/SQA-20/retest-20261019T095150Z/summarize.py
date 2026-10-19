"""Derive comparison data from the retained execution files; no tests are run."""
import csv
import hashlib
import json
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parent


def read_json(relative):
    return json.loads((ROOT / relative).read_text(encoding="utf-8-sig"))


def suite(relative):
    trx = ET.parse(ROOT / relative).getroot()
    ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    coverage_path = next((ROOT / relative).parent.glob("**/coverage.cobertura.xml"))
    coverage = ET.parse(coverage_path).getroot()
    return {
        "trx": relative,
        "counters": trx.find("t:ResultSummary/t:Counters", ns).attrib,
        "times": trx.find("t:Times", ns).attrib,
        "coverage": {key: coverage.attrib[key] for key in
                     ("lines-covered", "lines-valid", "branches-covered", "branches-valid")},
        "coverageFile": coverage_path.relative_to(ROOT).as_posix(),
    }


def browser_summary(relative):
    data = read_json(relative)
    result = {key: data[key] for key in ("counts", "overallAutomatedStatus", "startedAtUtc", "completedAtUtc")}
    result["source"] = relative
    result["browsers"] = []
    for execution in data["executions"]:
        browser = read_json(str(Path(relative).parent / execution["evidence"]))
        result["browsers"].append({
            "browser": browser["browser"], "version": browser["version"],
            "keyboardPageViewportRuns": len(browser["keyboard"]),
            "forwardPassed": sum(item["forwardPassed"] for item in browser["keyboard"]),
            "reversePassed": sum(item["reversePassed"] for item in browser["keyboard"]),
            "labelGapsAcrossViewports": sum(len(item["labelIssues"]) for item in browser["keyboard"]),
            "stretchedBadgesAcrossViewports": sum(len(item.get("stretchedBadges", [])) for item in browser["layouts"]),
            "rawAxeViolations": [{"page": scan["page"], "viewport": scan["viewportName"],
                                   "violations": scan.get("violations"),
                                   "proof": scan.get("scrollableRegionVerification")}
                                  for scan in browser["accessibility"] if scan.get("violations")],
            "unresolvedAxeRules": sum(len(scan.get("unresolvedViolations", [])) for scan in browser["accessibility"]),
        })
    return result


before_targets = read_json("before-targets/accessibility-targets.json")
after_targets = read_json("after-targets/accessibility-targets.json")
# Raw scan filenames in the target records are relative to their own session.
for name, data in (("before-targets", before_targets), ("after-targets", after_targets)):
    for browser in data["browsers"]:
        for page in browser["pages"]:
            page["axeEvidence"] = f"{name}/{page['axeEvidence']}"


def normalized_targets(data):
    output = {"status": data["status"], "browsers": []}
    for browser in data["browsers"]:
        pages = []
        for page in browser["pages"]:
            raw = read_json(page["axeEvidence"])
            pages.append({"page": page["page"], "viewport": page["viewportName"], "status": page["status"],
                          "controls": len(page["labels"]), "passingControls": sum(item["passed"] for item in page["labels"]),
                          "axeNamingNodes": sum(len(rule["nodes"]) for rule in raw["violations"] if rule["id"] in ("label", "select-name")),
                          "axeContrastNodes": sum(len(rule["nodes"]) for rule in raw["violations"] if rule["id"] == "color-contrast"),
                          "contrast": [{key: item.get(key) for key in ("requestedState", "state", "ratio", "color", "status")} for item in page["contrast"]]})
        output["browsers"].append({"browser": browser["browser"], "version": browser["version"], "pages": pages})
    return output


history = list(csv.DictReader((ROOT / "historical-evidence-sha256.csv").open(encoding="utf-8-sig", newline="")))
repo = ROOT.parents[3]
changed_history = [item["Path"] for item in history if
                   hashlib.sha256((repo / item["Path"]).read_bytes()).hexdigest().lower() != item["Sha256"].lower()]
summary = {
    "baseCommit": (ROOT / "base-commit.txt").read_text(encoding="utf-8-sig").strip(),
    "sourceIdentity": "Uncommitted accessibility diff over base commit; source and DLL hashes retained separately.",
    "baselineSuite": suite("baseline-suite/baseline.trx"),
    "postfixSuite": suite("postfix-suite/postfix.trx"),
    "baselineBrowsers": browser_summary("before/browser-execution.json"),
    "postfixBrowsers": browser_summary("after/browser-execution.json"),
    "baselineTargets": normalized_targets(before_targets),
    "postfixTargets": normalized_targets(after_targets),
    "baselineHttp": read_json("baseline-http-4/summary.json"),
    "postfixHttp": read_json("postfix-http-verified/summary.json"),
    "csvValidation": read_json("after/csv-validation.json")["totals"],
    "historicalEvidence": {"checkedFiles": len(history), "changed": changed_history},
    "limitations": ["Initial full baseline browser scan was blocked by pinned-file CRLF integrity mismatch; separate verified before-targets scans reproduce labels and contrast before application edits.",
                    "Baseline severity category intentionally requires compact badges, unlike the historical 16/16 category result.",
                    "Older HTTP CSV schema assertions were strengthened; final postfix-http-verified uses exact headers, strict parsing and raw row widths.",
                    "Existing numeric endpoint validation fails in both HTTP phases; SQA-20 PASS cannot imply overall release readiness."]
}
(ROOT / "comparison.json").write_text(json.dumps(summary, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"postfixBrowserStatus": summary["postfixBrowsers"]["overallAutomatedStatus"],
                  "postfixTargetStatus": summary["postfixTargets"]["status"],
                  "historicalEvidence": summary["historicalEvidence"]}))
