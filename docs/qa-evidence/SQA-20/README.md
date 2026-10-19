# SQA-20 manual accessibility and browser compatibility

Requirements: **NFR-USA-01, NFR-COMP-01**. **Overall automated SQA-20: FAIL. Human acceptance: NOT PERFORMED.** These are QA procedures and factual evidence, not submitted assessment report prose.

The [19 October automated execution ledger](playwright-20261019T035608Z/README.md) records **16/16 scoped technical browser checks PASS**, while additional programmatic-label and colour-contrast checks **FAIL**. The technical passes cover navigation, physical focus traversal, geometry, severity data, tables and CSV downloads; they do not establish complete accessibility acceptance. The [observation sheet](observation-sheet.csv) now explicitly records automated execution. Its original blank version is preserved in [the baseline](playwright-20261019T035608Z/baseline/observation-sheet.csv). Existing preparation evidence below remains historical. No participant answers or human browser results are invented.

## Open the isolated application

- Local URL: **http://localhost:51920/**. Use the same URL in Google Chrome and Microsoft Edge.
- Start/restart from the repository root: `powershell -NoProfile -File .\tools\Sqa20.ManualHost\Start-Sqa20.ps1`.
- Stop only this host: `powershell -NoProfile -File .\tools\Sqa20.ManualHost\Stop-Sqa20.ps1`. For a foreground launch, Ctrl+C also stops it. Stop before restarting; existing listeners are not terminated by the launcher.
- This development-only host uses the actual application MVC controllers, Razor views, CSS and CSV actions, with a separate process-local in-memory store. Restarting restores the synthetic fixture; it does not read or modify another application's data.
- The fixture makes no API requests. All eight result records are synthetic evaluator inputs; any displayed `PASS`/`FAIL` badges describe those fixture results, not SQA-20 outcomes. Displayed response times are supplied fixture values, not measured API durations.
- The host permits GET/HEAD requests and rejects POST requests with HTTP 405. Focus and inspect form controls, but do not submit **Run all checks**, **Save API**, **Add endpoint** or **Update**. A deliberate POST rejection is a harness restriction, not an application defect. Do not register real API targets or attempt scans.
- The normal layout loads Bootstrap CSS from jsDelivr. Record stylesheet availability and any load error. This browser stylesheet request is separate from API scanning. Do not change application styling or silently substitute a local stylesheet during the session.

## Prepared data and routes

These are expected fixture values for preparation, not observed browser results.

| Item | Expected fixture |
|---|---|
| Registered APIs / configured endpoints | 2 / 2 |
| Checks | 8 total: 4 passing, 4 failing; 50% pass rate |
| Open defects | 4: Critical 1, High 1, Medium 1, Low 1; each severity is 25% of open defects |
| Open High/Critical | 2 |
| Defect statuses | New, Assigned, InProgress and Retest; no Closed/Deferred records |
| Recent records | All 8 checks and 4 defects fit the dashboard's recent lists; seeded near host startup |
| Text stress | Long endpoint name; API text containing commas and quotation marks |

| Page/download | URL |
|---|---|
| Dashboard | http://localhost:51920/ |
| Registered APIs | http://localhost:51920/Apis |
| Failing fixture details | http://localhost:51920/Apis/Details/20200000-0000-0000-0000-000000000001 |
| Healthy fixture details | http://localhost:51920/Apis/Details/20200000-0000-0000-0000-000000000002 |
| Registration form, inspect only | http://localhost:51920/Apis/Create |
| Defect register | http://localhost:51920/Defects |
| Results CSV | http://localhost:51920/Reports/ExportResultsCsv |
| Defects CSV | http://localhost:51920/Reports/ExportDefectsCsv |

If the fixture counts or routes differ, record a setup issue and correct preparation before continuing. Automated HTTP/setup evidence belongs in `automated-setup/`; it cannot establish browser compatibility, focus visibility, narrow layout or download usability.

## Record the session

The original manual procedure below is retained as a reference. The current [observation-sheet.csv](observation-sheet.csv) contains automated results; use the preserved [blank baseline](playwright-20261019T035608Z/baseline/observation-sheet.csv) if a separate human session is later recorded. Keep any future human evidence separate from the automated session, with actual session metadata.

| Field | Actual entry |
|---|---|
| Session ID and tester ID | |
| Start and end date/time in UTC | |
| Start and end date/time in Pacific/Auckland (NZDT for this prepared session date) | |
| OS/version and screen resolution | |
| Software commit SHA; branch; uncommitted source/diff reference | |
| .NET SDK/runtime; automated setup evidence reference | |
| Chrome exact version | |
| Edge exact version | |
| Browser zoom; desktop viewport width/height in CSS pixels | |
| Narrow viewport width/height in CSS pixels | |
| Bootstrap stylesheet loaded / errors | |
| CI reference, or N/A for a local session | |
| Interruptions, deviations and linked issue references | |

Read exact versions from `chrome://version` and `edge://version`. Record actual times with the timezone/offset; do not copy preparation timestamps as execution times. Record source changes separately from the base commit SHA.

## Execute in each browser

Complete all eight rows in Chrome, then all eight in Edge. Use 100% zoom initially and record any changes. Capture actual screenshots and observations as you work; a screenshot alone does not demonstrate keyboard navigation or a completed download.

| Check | Manual steps and expected behaviour |
|---|---|
| 01 Compatibility | Open Dashboard, APIs, both API details, Defects and the registration form. Check that content and styling load, navigation works and controls are usable. Record errors, missing content or browser differences. Do not submit forms. |
| 02 Forward keyboard | Starting at the page, use Tab through navigation and available links/buttons/inputs/selects on the pages above. Confirm visible focus, understandable order, access to controls and no keyboard trap. Use Enter on navigation/download links when relevant, without submitting forms. Record the actual sequence and any skipped/unreachable control. |
| 03 Reverse keyboard | Use Shift+Tab back through the same controls. Confirm visible focus, reverse order and ability to leave every control. Inspect label clarity and the visible relationship between labels and controls; record any ambiguity. |
| 04 Narrow viewport | Open DevTools device emulation with a responsive viewport at approximately 375 CSS pixels wide; record exact width/height and zoom. Revisit Dashboard, APIs, both details, Defects and registration. Check metrics, headings, navigation, labels and controls for overlap/clipping. Save narrow screenshots; restore desktop afterwards. |
| 05 Severity | On Dashboard inspect Critical, High, Medium and Low labels, each count 1 and quarter-width distribution; total open 4 and High/Critical 2. Verify labels/counts convey meaning alongside colours and remain readable at desktop and narrow widths. |
| 06 Tables | At desktop and 375 CSS pixels inspect Recent checks and both API detail tables, including the long endpoint name. Read check messages, PASS/FAIL labels and values; test any horizontal scrolling. Check that overflow remains discoverable and values do not overlap or disappear. Inspect recent defects and Defect Register long text as well. |
| 07 Results CSV | Activate **Export Results** with the browser, save the actual attachment under `downloads/<session-id>/<browser>/`, and record its filename. Check `api-check-results-*.csv`, a 15-column header and 8 data records, including 4 True/Pass and 4 False/Fail outcomes. Open with a CSV-aware viewer or import tool; confirm names containing commas/quotes remain one field and match the displayed fixture text. Retain the original download unchanged. |
| 08 Defects CSV | On Defects activate **Export CSV**, save and record the actual attachment. Check `defects-*.csv`, a 21-column header and 4 data records with one each Critical/High/Medium/Low, the seeded statuses, and `IsOpen=True`. Confirm lifecycle/source-check columns are present and comma/quote-containing names remain one field. Retain the original download unchanged. |

For keyboard evidence, write observations such as the focused control and navigation sequence, with screenshot references for visible focus. A missing visible focus, inaccessible control, unreadable overflow or malformed download needs a specific reproduction and evidence reference. Record any issue as observed; do not preassign failure from source inspection.

## Evidence and completion

- [screenshots/](screenshots/README.md): actual Chrome/Edge desktop, narrow, severity/table and focus captures.
- [observations/](observations/README.md): session metadata, actual notes, statuses and linked findings.
- [downloads/](downloads/README.md): original browser CSV downloads; any annotated/imported copies kept separately.
- `automated-setup/`: automated preparation evidence only, maintained separately from manual outcomes.

Original preparation baseline: **planned 16; executed 0; passed 0; failed 0; blocked 0; not run 16**. The latest automated session executed all 16 technical cases: **passed 16; failed 0; blocked 0; not run 0**. Additional accessibility verification failed, so the master SQA-20 case is **FAIL**. Human acceptance was not performed. Case totals and overall acceptance are deliberately separate.

The user's latest instruction authorizes browser automation to update SQA-20. The master register now records the verified automated failure and the unperformed human-acceptance limitation. SQA-19's representative-user test stays unchanged. The earlier assertion of five human passes is retained as provenance; corresponding local screenshots/downloads/observations were not found, so no human results were imported. Automated scanning does not establish full WCAG compliance.
