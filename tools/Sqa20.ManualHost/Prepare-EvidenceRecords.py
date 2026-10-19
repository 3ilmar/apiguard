"""Derive factual QA records from the saved SQA-20 executions; no browser testing."""
import argparse
import csv
import datetime as dt
import hashlib
import io
import json
from pathlib import Path
import xml.etree.ElementTree as ET


def load(file):
    return json.loads(file.read_text(encoding="utf-8-sig"))


def save(file, data):
    file.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("session", type=Path)
    args = parser.parse_args()
    session = args.session.resolve()
    root = Path(__file__).resolve().parents[2]
    summary = load(session / "browser-execution.json")
    environment = load(session / "environment.json")
    supplemental = load(session / "narrow-keyboard-settled-verification.json")
    assert len(summary["results"]) == 16
    assert all(result["status"] in ("PASS", "FAIL", "BLOCKED", "NOT RUN") for result in summary["results"])
    browsers = {name: load(session / name / "browser-summary.json") for name in ("chrome", "edge")}

    # Deduplicate repeated viewport observations, retaining their original scan references.
    accessibility = {"method": "Derived from original axe JSON; unique counts use browser + page + selector", "browsers": {}}
    for name in browsers:
        unique = {}
        incomplete = {}
        for scan_file in sorted((session / name).glob("axe-*.json")):
            scan = load(scan_file)
            for group, output in (("violations", unique), ("incomplete", incomplete)):
                for rule in scan[group]:
                    for node in rule["nodes"]:
                        key = (scan["url"], rule["id"], json.dumps(node["target"]))
                        record = output.setdefault(key, {"url": scan["url"], "rule": rule["id"], "impact": rule["impact"], "targets": node["target"], "failureSummary": node.get("failureSummary"), "help": rule["help"], "helpUrl": rule["helpUrl"], "evidence": []})
                        record["evidence"].append(scan_file.relative_to(session).as_posix())
        dom_gaps = sum(len(item["labelIssues"]) for item in browsers[name]["keyboard"])
        accessibility["browsers"][name] = {
            "status": browsers[name]["additionalAccessibilityStatus"],
            "uniqueViolations": list(unique.values()), "incomplete": list(incomplete.values()),
            "uniqueNamingViolations": sum(item["rule"] in ("label", "select-name") for item in unique.values()),
            "domMissingReliableLabelAssociations": dom_gaps,
            "namingCountExplanation": "The DOM count also includes placeholder-backed fields not failed by axe. Counts describe rendered controls across the tested routes; repeated viewport/browser instances are not separate product defects.",
            "scrollableRegionFinding": "Raw axe flag retained. Genuine Tab, Shift+Tab and ArrowRight reached and scrolled each overflowing endpoint table in the tested native browsers. See narrow-keyboard-settled-verification.json; not a confirmed keyboard failure."
        }
    save(session / "accessibility-summary.json", accessibility)
    assert all(item["uniqueNamingViolations"] == 22 and item["domMissingReliableLabelAssociations"] == 26 for item in accessibility["browsers"].values()), "Observed findings changed; review the evidence classification before updating the QA records."

    namespaces = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    trx = ET.parse(session / "dotnet/automated-suite.trx").getroot()
    counters = {key: int(value) for key, value in trx.find("t:ResultSummary/t:Counters", namespaces).attrib.items()}
    coverage = ET.parse(session / "dotnet/cobertura.xml").getroot()
    suite = {"method": "Existing MSTest suite, Debug with XPlat Code Coverage", "counters": counters, "times": trx.find("t:Times", namespaces).attrib, "coverage": {
        key: int(coverage.attrib[key]) for key in ("lines-covered", "lines-valid", "branches-covered", "branches-valid")}}
    suite["coverage"]["linePercent"] = round(100 * suite["coverage"]["lines-covered"] / suite["coverage"]["lines-valid"], 2)
    suite["coverage"]["branchPercent"] = round(100 * suite["coverage"]["branches-covered"] / suite["coverage"]["branches-valid"], 2)
    suite["coverage"]["packages"] = [{"name": package.attrib["name"], "lineRate": package.attrib["line-rate"], "branchRate": package.attrib["branch-rate"]} for package in coverage.findall("packages/package")]
    save(session / "dotnet/test-summary.json", suite)

    # Make the broader accessibility failure explicit without rewriting genuine
    # passes for narrower focus/navigation/geometry/CSV measurements.
    summary["caseScope"] = "The 16 cases verify route/CSS compatibility, physical Tab/Shift+Tab and visible-focus mechanics, viewport geometry, severity data, table scrolling and browser CSV downloads. Programmatic names and contrast are assessed separately and FAIL. These are not 16 full accessibility acceptance passes."
    summary["additionalAccessibilityEvidence"] = "accessibility-summary.json"
    summary["supplementalKeyboardEvidence"] = "narrow-keyboard-settled-verification.json"
    summary["dotnetEvidence"] = "dotnet/test-summary.json"
    summary["humanExecution"] = "NOT PERFORMED; no participant answers or human observations recorded by this automation."
    save(session / "browser-execution.json", summary)

    actions = {
        1: "Visit six MVC pages at both viewports; click navigation/registration/API-detail links; inspect response/resource/console and control visibility records.",
        2: "Send genuine Tab keys across six desktop pages and supplementary narrow detail pages; inspect focus order, rings, viewport and trusted key events.",
        3: "Send genuine Shift+Tab keys; compare reverse traversal with observed forward order; inspect focused screenshots and ability to leave controls.",
        4: "Capture full-page and viewport screenshots at 1440x900 and 375x812; measure document overflow, overlaps and clipped controls.",
        5: "Read severity words, counts, progress accessible names and geometry; inspect defect severity badges at both widths.",
        6: "Inspect table headings, rows and PASS/FAIL words; use horizontal mouse wheel and targeted keyboard ArrowRight on overflowing endpoint tables.",
        7: "Activate Export Results; save the real browser download unchanged; strictly decode UTF-8 and parse logical CSV records.",
        8: "Activate Defects Export CSV; save the real browser download unchanged; strictly parse UTF-8 CSV and validate lifecycle/severity/source-content fields."
    }
    expected = {
        1: "All required pages HTTP 200; expected content, navigation and styles available; no material resource or console errors.",
        2: "Physical Tab traversal reaches enabled controls in DOM order with a visible focus indicator and no keyboard trap; accessible names assessed separately.",
        3: "Shift+Tab reverses physical traversal with visible focus, no missing enabled controls and no trap.",
        4: "Both measured viewports have no unintended document overflow, overlapping headings/buttons or clipped controls; table overflow handled locally.",
        5: "Critical/High/Medium/Low each 1 and 25%; open total 4; high/critical total 2; words/numbers supplement colour.",
        6: "Headings and data retained; PASS/FAIL words readable; overflowing tables scroll to expose their rightmost data.",
        7: "Actual browser attachment filename matches pattern; 15 columns; 8 records; 4 passing/4 failing; valid UTF-8 CSV with preserved commas/quotes.",
        8: "Actual browser attachment filename matches pattern; 21 columns; 4 open defects, one per severity; fixture lifecycle fields and escaping preserved."
    }
    rows = []
    for result in summary["results"]:
        name = result["browser"]
        browser = browsers[name]
        number = int(result["id"][-2:])
        additional = accessibility["browsers"][name]
        facts = {
            1: f'{len(browser["pages"])} required page/viewport visits completed; six navigation clicks verified; original CSS loaded. Browser {browser["version"]}.',
            2: f'Desktop: {sum(len(item["baseline"]["candidates"]) for item in browser["keyboard"])} mandatory control instances reached; {sum(item["trustedForwardEvents"] for item in browser["keyboard"])} trusted Tab events; no missing controls or focus failures. Settled narrow detail checks also passed. Accessible-name checks FAIL separately ({additional["uniqueNamingViolations"]} axe findings; {additional["domMissingReliableLabelAssociations"]} DOM association gaps).',
            3: f'{sum(item["trustedReverseEvents"] for item in browser["keyboard"])} trusted desktop Shift+Tab events; every reverse order/cycle verified. Settled narrow detail traversal also passed; label/contrast defects remain outside this mechanics result.',
            4: 'All six pages measured at both sizes without unintended document overflow, heading/button overlap or clipped controls. Stretched severity badges reproduced cosmetically; labels stayed readable.',
            5: 'All four words/counts/progress names verified at both widths: each 1/25%, open 4, High/Critical 2. Tall Defect Register badge background is a recorded Low cosmetic issue.',
            6: 'Dashboard and detail tables retained headings/data and explicit PASS/FAIL words. Both overflowing endpoint tables scrolled to their right edges by wheel; native Tab/Shift+Tab and ArrowRight also reached/scrolled them despite the raw axe scroller flag.',
            7: 'Real browser results attachment validated: 15 columns, 8 records, 4 True/Pass and 4 False/Fail; strict UTF-8 parsing, quoted text and data integrity checks passed.',
            8: 'Real browser defects attachment validated: 21 columns, 4 records, all severities once; statuses New/Assigned/InProgress/Retest, correct lifecycle and IsOpen=True; strict UTF-8 and quoted text passed.'
        }
        evidence = list(result["evidence"])
        if number in (2, 3, 6):
            evidence.append("narrow-keyboard-settled-verification.json")
        evidence.append("accessibility-summary.json")
        evidence.append("findings.md")
        issues = "SQA20-A11Y-01; SQA20-A11Y-02" if number in (1, 2, 3) else "SQA20-UI-01" if number in (4, 5) else ""
        start = dt.datetime.fromisoformat(browser["startedAtUtc"].replace("Z", "+00:00"))
        end = dt.datetime.fromisoformat(browser["completedAtUtc"].replace("Z", "+00:00"))
        nzdt = dt.timezone(dt.timedelta(hours=13), "NZDT")
        rows.append({
            "Check ID": result["id"], "Browser": browser["product"], "Check": result["test"],
            "Expected result": expected[number], "Execution status": result["status"], "Actual observation": facts[number],
            "Actual UTC date/time": f'{start.isoformat()} to {end.isoformat()}; individual events in JSON',
            "Actual Pacific/Auckland date/time": f'{start.astimezone(nzdt).isoformat()} to {end.astimezone(nzdt).isoformat()}',
            "Session ID": session.name, "Tester ID": "Codex Playwright automation (not a human participant)",
            "Browser version": browser["version"], "Viewport CSS pixels": "1440x900; 375x812", "Zoom": "Fresh context; deviceScaleFactor=1; no zoom override",
            "Screenshot or download reference": "; ".join(f'{session.name}/{reference}' for reference in dict.fromkeys(evidence)),
            "Issue reference": issues, "Notes or deviations": "Scoped technical PASS; overall automated accessibility FAIL. No human acceptance inferred.",
            "Execution method": "AUTOMATED PLAYWRIGHT; installed browser binary, isolated headless context",
            "Preconditions": "Existing localhost:51920 isolated Development host; 2 APIs/2 endpoints/8 synthetic checks/4 open defects; writes and scans disabled",
            "Actions performed": actions[number],
            "Limitations": "Headless browser context; no OS dialogs, screen-reader interaction, spreadsheet-product execution or human comprehension test. CSV result schema omits result IDs, limiting exact source-ID cross-export proof.",
            "Additional accessibility status": "FAIL: programmatic labels and colour contrast",
            "Previous human evidence": "No human result imported: five passes previously asserted, but no corresponding local files or populated observation rows found. Original sheet retained in baseline."
        })
    fields = list(rows[0])
    buffer = io.StringIO(newline="")
    writer = csv.DictWriter(buffer, fieldnames=fields, lineterminator="\n")
    writer.writeheader()
    writer.writerows(rows)
    text = buffer.getvalue()
    (session / "observations.csv").write_text(text, encoding="utf-8", newline="")
    (session.parent / "observation-sheet.csv").write_text(text, encoding="utf-8", newline="")

    # Replace only SQA-20's physical CSV line, preserving every other case byte-for-byte.
    register = root / "docs/ShiruiFinalTestCases.csv"
    original = register.read_bytes()
    lines = original.splitlines(keepends=True)
    sqa19_before = next(line for line in lines if line.startswith(b"SQA-19,"))
    replacements = 0
    for index, line in enumerate(lines):
        if not line.startswith(b"SQA-20,"):
            continue
        row = next(csv.reader([line.decode("utf-8")]))
        row[3] = "Automated browser compatibility/accessibility; human acceptance not performed"
        row[4] = f'Installed Chrome {browsers["chrome"]["version"]} and Edge {browsers["edge"]["version"]}; isolated synthetic fixture; desktop and 375px'
        row[5] = "Execute 16 scoped Playwright categories plus axe WCAG scans; inspect screenshots, trusted keyboard events and original browser CSVs"
        row[7] = "19 October 2026: 16/16 scoped technical browser cases passed (0 failed/blocked/not run); additional axe accessibility verification FAILED: 22 unnamed control instances per browser and contrast 4.23/4.4 below 4.5. DOM inspection found 26 label association gaps. Cosmetic stretched badges reproduced. Overall SQA-20 automated criteria NOT satisfied; no human acceptance claimed."
        row[8] = "FAIL"
        row[9] = f'qa-evidence/SQA-20/{session.name}/README.md; qa-evidence/SQA-20/{session.name}/findings.md; qa-evidence/SQA-20/{session.name}/browser-execution.json'
        output = io.StringIO(newline="")
        csv.writer(output, lineterminator="\r\n" if line.endswith(b"\r\n") else "\n").writerow(row)
        lines[index] = output.getvalue().encode("utf-8")
        replacements += 1
    assert replacements == 1
    updated = b"".join(lines)
    assert next(line for line in updated.splitlines(keepends=True) if line.startswith(b"SQA-19,")) == sqa19_before
    register.write_bytes(updated)
    (session / "sqa19-register-preserved.json").write_text(json.dumps({"unchanged": True, "sha256": hashlib.sha256(sqa19_before).hexdigest()}, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"observationRows": len(rows), "counts": summary["counts"], "overallSqa20": "FAIL", "sqa19Unchanged": True, "dotnet": counters}))


if __name__ == "__main__":
    main()
