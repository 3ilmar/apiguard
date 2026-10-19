# SQA-20 automated execution record — 19 October 2026

**Overall automated outcome: FAIL.** The 16 requested browser category executions passed their stated technical criteria: **16 PASS, 0 FAIL, 0 BLOCKED, 0 NOT RUN**. Additional accessibility scanning found reproducible label and contrast failures, so the master SQA-20 outcome must remain FAIL. “16/16 technical checks passed” does not establish accessibility acceptance or full WCAG compliance.

This directory contains factual QA execution records, original downloads and machine evidence. It is not student-authored assessment report prose. All new browser results are **AUTOMATED PLAYWRIGHT**, using genuine installed Chrome and Edge executables in headless, isolated contexts. No participant session or human acceptance result is claimed; this limitation is not a request for additional manual testing.

## Environment and execution

| Item | Recorded value |
|---|---|
| Application | `http://localhost:51920/`, isolated SQA-20 development host |
| Branch | `qa/sqa19-sqa20-manual-validation` |
| Base commit | `8e69c6273080b01db100c29cc76eb33a798e84af`; testing tools/evidence were uncommitted |
| OS / CPU | Windows `10.0.26200`, x64; Intel Core i7-12700H, 20 logical CPUs |
| Chrome | `155.0.8059.80`, `C:\Program Files\Google\Chrome\Application\chrome.exe` |
| Edge | `155.0.4283.62`, `C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe` |
| Tooling | Node `v24.19.0`; Playwright `1.62.1`; axe-core `4.10.3` |
| Browser configuration | Headless genuine installed executables; fresh contexts; device scale factor 1; desktop 1440 × 900 and narrow 375 × 812 CSS pixels |
| Main browser collection | 19 October 2026, 17:01:46.721–17:02:43 NZDT; original UTC timestamps retained in JSON/logs |
| Result finalization | 19 October 2026, 17:05:59 NZDT; saved browser outputs used without repeating downloads |
| Settled narrow keyboard verification | 19 October 2026, approximately 17:06:11–17:06:41 NZDT |
| .NET suite | 19 October 2026, 17:06:53–17:07:00 NZDT; exact TRX timestamps retained |
| Data | 2 APIs, 2 endpoints; 8 checks, 4 PASS and 4 FAIL; 4 open defects, one each Critical/High/Medium/Low |
| Network/data isolation | No form submissions, API scans or production-data writes; browser traffic allowed only to this localhost host and the application's existing Bootstrap stylesheet URL |

[environment.json](environment.json), [execution.log](execution.log) and [baseline/provenance.json](baseline/provenance.json) record actual software state and timestamps. Browser versions are recorded independently in [Chrome summary](chrome/browser-summary.json) and [Edge summary](edge/browser-summary.json). No Chromium substitute was used.

## Sixteen browser category executions

The table reports physical keyboard behavior, layout geometry, navigation, severity presentation, table data access and CSV integrity. Associated names and colour contrast are separately failed below, even where a control physically receives visible keyboard focus.

| Test ID | Browser | Category | Status | Actual automated result / evidence |
|---|---|---|---|---|
| SQA20-CH01 | Chrome | Compatibility | PASS | Six pages at both viewports; HTTP/content/styles/navigation checks passed, no resource failures. [Summary](chrome/browser-summary.json), [events](chrome/browser-events.json). |
| SQA20-CH02 | Chrome | Forward keyboard | PASS | Genuine Tab reached the expected controls with distinguishable measured focus; no missing mandatory control or trap. [Dashboard](chrome/keyboard-dashboard.json), [API detail](chrome/keyboard-orders.json), [Defects](chrome/keyboard-defects.json). |
| SQA20-CH03 | Chrome | Reverse keyboard | PASS | Genuine Shift+Tab order matched the expected reverse sequence. [Registration](chrome/keyboard-registration.json), [API list](chrome/keyboard-apis.json), [catalogue](chrome/keyboard-catalogue.json). |
| SQA20-CH04 | Chrome | Responsive layout | PASS | All six pages at 1440 × 900 and 375 × 812 passed measured document-overflow, control-overlap and clipping criteria. [Narrow registration](chrome/layout-narrow-registration.json), [narrow Defects](chrome/layout-narrow-defects.json). |
| SQA20-CH05 | Chrome | Severity presentation | PASS | Four text-labelled categories, each count 1, with accessible progressbar names/values; stretched Defects badges remain legible in saved screenshots. [Dashboard](chrome/layout-narrow-dashboard.json), [Defects](chrome/layout-narrow-defects.json). |
| SQA20-CH06 | Chrome | Tables and long content | PASS | Headers, rows and PASS/FAIL text retained; overflowing endpoint tables scroll through genuine horizontal wheel input. [Orders](chrome/layout-narrow-orders.json), [catalogue](chrome/layout-narrow-catalogue.json). |
| SQA20-CH07 | Chrome | Results CSV | PASS | Actual download: 15 columns, 8 records, 4 PASS/4 FAIL, valid UTF-8, quoted commas/quotes preserved. [CSV](chrome/downloads/api-check-results-20261019-040212.csv), [validation](csv-validation.json). |
| SQA20-CH08 | Chrome | Defects CSV | PASS | Actual download: 21 columns, 4 records, all four severities, lifecycle and IsOpen fields verified. [CSV](chrome/downloads/defects-20261019-040213.csv), [validation](csv-validation.json). |
| SQA20-ED01 | Edge | Compatibility | PASS | Six pages at both viewports; HTTP/content/styles/navigation checks passed, no resource failures. [Summary](edge/browser-summary.json), [events](edge/browser-events.json). |
| SQA20-ED02 | Edge | Forward keyboard | PASS | Genuine Tab reached the expected controls with distinguishable measured focus; no missing mandatory control or trap. [Dashboard](edge/keyboard-dashboard.json), [API detail](edge/keyboard-orders.json), [Defects](edge/keyboard-defects.json). |
| SQA20-ED03 | Edge | Reverse keyboard | PASS | Genuine Shift+Tab order matched the expected reverse sequence. [Registration](edge/keyboard-registration.json), [API list](edge/keyboard-apis.json), [catalogue](edge/keyboard-catalogue.json). |
| SQA20-ED04 | Edge | Responsive layout | PASS | All six pages at 1440 × 900 and 375 × 812 passed measured document-overflow, control-overlap and clipping criteria. [Narrow registration](edge/layout-narrow-registration.json), [narrow Defects](edge/layout-narrow-defects.json). |
| SQA20-ED05 | Edge | Severity presentation | PASS | Four text-labelled categories, each count 1, with accessible progressbar names/values; stretched Defects badges remain legible in saved screenshots. [Dashboard](edge/layout-narrow-dashboard.json), [Defects](edge/layout-narrow-defects.json). |
| SQA20-ED06 | Edge | Tables and long content | PASS | Headers, rows and PASS/FAIL text retained; overflowing endpoint tables scroll through genuine horizontal wheel input. [Orders](edge/layout-narrow-orders.json), [catalogue](edge/layout-narrow-catalogue.json). |
| SQA20-ED07 | Edge | Results CSV | PASS | Actual independent download: 15 columns, 8 records, 4 PASS/4 FAIL, UTF-8 and escaping checks passed. [CSV](edge/downloads/api-check-results-20261019-040242.csv), [validation](csv-validation.json). |
| SQA20-ED08 | Edge | Defects CSV | PASS | Actual independent download: 21 columns, 4 records, severities/lifecycle/IsOpen and matching failed-result content verified. [CSV](edge/downloads/defects-20261019-040243.csv), [validation](csv-validation.json). |

The complete expected/actual/evidence records are in [browser-execution.json](browser-execution.json), the session observation sheet and the main QA register. Narrow native Tab/Shift+Tab access and ArrowRight table scrolling are additionally evidenced in [narrow-keyboard-settled-verification.json](narrow-keyboard-settled-verification.json): endpoint table scrollLeft moved from 0 to 40 in both browsers on both API pages.

The results CSV has no result ID column. SourceCheckResultId values were validated as nonempty unique UUIDs and defect contents matched failed result contents; exact source-ID equality across the two exports cannot be established from these schemas.

## Additional accessibility outcome

**FAIL in Chrome and Edge.** axe ran on all six pages at both viewports with supported `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa` and `wcag22aa` tags. Deduplicated across viewports, each browser reported 22 control instances without an acceptable name across the two API detail pages and Defects: 12 `label` targets and 10 `select-name` targets. A separate, more conservative DOM audit found 26 instances without associated labels/names, including four placeholder-backed fields that axe accepts as a naming fallback. These are overlapping measurements, not 48 different product defects.

Three unique contrast targets per browser also failed: the base URL on each API detail page (4.23:1) and the Defects CSV link (4.4:1), against 4.5:1 for the measured normal text. [findings.md](findings.md) records reproducible defects, affected selectors and corrections. Axe's raw `scrollable-region-focusable` flags were investigated through actual browser keyboard input; the tested tables were focusable and keyboard-scrollable, so that flag is not confirmed as a product defect in these browser versions.

Automated checks and AI inspection do not establish full WCAG conformance, screen-reader usability or representative-user acceptance. The remaining limitations are recorded without assigning fabricated human outcomes.

## Evidence map and provenance

- [browser-execution.json](browser-execution.json): 16 category records, counts and overall automated FAIL.
- [Chrome summary](chrome/browser-summary.json), [Edge summary](edge/browser-summary.json): per-page content, stylesheet checks, layout, keyboard, severity, navigation and download metadata.
- `chrome/` and `edge/`: `desktop-<page>-full.png`, `desktop-<page>-viewport.png`, corresponding `narrow-...` screenshots, table right-edge screenshots, focus screenshots, `layout-...json`, `keyboard-...json`, raw `axe-...json` and browser events.
- [Settled narrow verification](narrow-keyboard-settled-verification.json) and each browser's `narrow-settled/`: final stable narrow keyboard JSON/screenshots. Initial narrow verification and screenshots are retained in [narrow-keyboard-verification.json](narrow-keyboard-verification.json) and their original browser paths.
- [csv-validation.json](csv-validation.json): strict parse, column/record counts, naming, quote escaping, content and lifecycle checks. Four original downloaded files remain in the browser-specific `downloads/` directories.
- [dotnet/automated-suite.trx](dotnet/automated-suite.trx), [test log](dotnet/test.txt), [build](dotnet/build.txt), [restore](dotnet/restore.txt), [SDK/runtime](dotnet/dotnet-info.txt), [coverage](dotnet/cobertura.xml).
- [Baseline evidence hashes](baseline/existing-evidence-sha256.csv), [original observation-sheet copy](baseline/observation-sheet.csv) and [provenance](baseline/provenance.json): earlier records preserved before this execution.
- `runner-source/`: copies of the actual browser/layout/keyboard/accessibility/CSV scripts used, plus `-settled` and `-finalizer` variants showing the collection iterations. Pinned axe metadata is in [runner-source/axe-dependency.json](runner-source/axe-dependency.json); its license and source integrity metadata accompany the tools distribution.
- [sqa19-heuristic-review.md](sqa19-heuristic-review.md): separate review; SQA-19 human observations and status were not changed.
- [observations.csv](observations.csv), [accessibility-summary.json](accessibility-summary.json): session observation records and deduplicated scanner findings.
- [evidence-validation.json](evidence-validation.json), [evidence-index.json](evidence-index.json): final reference/preservation checks and per-file hashes; [source hashes](source-sha256.csv) identify application, tests and current tools.
- [changes.md](changes.md): modified/new files and draft-PR scope; [host-runtime.json](host-runtime.json) confirms the still-running loopback host's runtime.

The initial main collection exited 1 after both browsers had completed, when it attempted to archive a CSV validator script that was not yet available. The authentic error is retained in [execution.log](execution.log). `--finalize-only` subsequently validated the saved original downloads and wrote the aggregate result; navigation and downloads were not repeated.

The first targeted narrow keyboard pass sampled visibility after two animation frames, before Bootstrap's smooth scroll had settled. Initial transient screenshots/flags are retained. A subsequent run waits at least 180 ms and checks stable geometry; its actual Tab/Shift+Tab results have no missing controls or focus failures. This is an evidence-harness timing correction, not an application defect or an application change.

## .NET validation

Restore and build succeeded; build produced **0 warnings and 0 errors**. The full .NET suite produced **71 PASS, 0 FAIL, 0 SKIPPED**. The environment used SDK `10.0.402` and .NET runtime `8.0.32` for the net8.0 application. Cobertura reported **561/781 lines (71.83%)** and **222/392 branches (56.63%)**. These totals cover the collected .NET suite; they do not measure browser accessibility completeness.

## Reproduce without overwriting this session

From the repository root on the recorded branch, start the isolated host only if port 51920 is not already serving that host:

```powershell
powershell -NoProfile -File .\tools\Sqa20.ManualHost\Start-Sqa20.ps1
```

In a second PowerShell terminal, use a new evidence directory. `SQA20_NODE_MODULES` is needed when Playwright is supplied by the bundled Codex runtime instead of locally installed dependencies; the recorded module root follows from `playwrightPath` in environment.json. Set `SQA20_PYTHON` to a real Python interpreter so the runner also performs semantic CSV validation. The bundled interpreter below exists on this machine; change these runtime paths on another machine. An alternative is to run the archived Python validator separately, but leaving semantic validation unexecuted will produce BLOCKED CSV results.

```powershell
$env:SQA20_NODE_MODULES = 'C:\Users\zsr20\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules'
$env:SQA20_PYTHON = 'C:\Users\zsr20\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
$sqa20Output = Join-Path 'docs\qa-evidence\SQA-20' ('playwright-' + (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ'))
node .\tools\Sqa20.ManualHost\Run-BrowserChecks.cjs --output $sqa20Output
node .\tools\Sqa20.ManualHost\Verify-NarrowKeyboard.cjs $sqa20Output --settled
dotnet restore SecureApiQualityPlatform.sln
dotnet build SecureApiQualityPlatform.sln --no-restore
dotnet test SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj --no-build --collect "XPlat Code Coverage" --logger "trx;LogFileName=automated-suite.trx" --results-directory (Join-Path $sqa20Output 'dotnet')
```

The collector can exit 0 after saving a completed execution even when acceptance fails: inspect `overallAutomatedStatus` and the per-case results in `browser-execution.json`. The initial nonzero exit described above was a collection-finalization error, not an accessibility pass/fail exit policy. If a future run is interrupted after browser summaries/downloads are saved, investigate its log before using `--finalize-only`; do not use that option to claim an unexecuted browser test. Current editable tools: [runner](../../../../tools/Sqa20.ManualHost/Run-BrowserChecks.cjs), [keyboard](../../../../tools/Sqa20.ManualHost/browser-keyboard.cjs), [layout](../../../../tools/Sqa20.ManualHost/browser-layout.cjs), [axe wrapper](../../../../tools/Sqa20.ManualHost/browser-accessibility.cjs), [CSV validator](../../../../tools/Sqa20.ManualHost/validate_browser_csv.py).

## Readiness record

The 16 category technical criteria are satisfied in the recorded Chrome/Edge versions. Broader automated accessibility criteria are **not satisfied** because SQA20-A11Y-01 and SQA20-A11Y-02 remain open; cosmetic SQA20-UI-01 is also recorded. A draft QA-evidence PR is reviewable, subject to user approval to commit/push. This evidence does not provide release signoff. No application fixes, commits, pushes, merges or rebases were performed for this execution.

Suggested commit title: `test(qa): record SQA-20 Chrome and Edge automated evidence`

Suggested description: `Add reproducible browser, keyboard, layout, CSV and axe evidence with 16 technical category passes. Record confirmed label/contrast failures and overall SQA-20 FAIL; preserve prior evidence and SQA-19 pending human acceptance. Include the 71-test .NET validation and coverage. No application behavior changes.`
