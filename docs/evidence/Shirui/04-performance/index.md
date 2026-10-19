# SQA-18 / PT-01 browser performance collection

Latest local SQA-18 status: **PASS against the user's proposed operational criterion v1**; **not formally approved by the team**. Five actual browser reloads measured conservative screenshot-verified visual-readiness bounds of **257.9, 199.5, 197.1, 189.7 and 201.0 ms**; median **199.5 ms**, maximum **257.9 ms**. Correct dashboard pixels/values were verified in all five runs. See the [new execution ledger](2026-10-18/sqa18-visual-20261018T072617937Z/README.md) and [decision](2026-10-18/sqa18-visual-20261018T072617937Z/decision.json). The existing NFR-PERF-01 remains provisional/TBC, and formal approval/release remain pending.

## User-proposed operational acceptance criteria v1

The user proposed the following on 18 October 2026; no team approval is asserted:

1. Exactly 2,000 deterministic check results.
2. Correct dashboard statistics and severity distribution.
3. Measure navigation start until the populated dashboard is visually ready, including stylesheet loading, layout and paint readiness.
4. Run five independent browser reload measurements under consistent conditions.
5. PASS if all five visual-readiness measurements are below 2,000 ms and the dashboard values are correct.

For the executed automated procedure, the recorded endpoint is immediately after viewport screenshot capture returns, following successful stylesheet/font/correctness/stable-layout checks. The retained PNG verifies rendered pixels; this conservative bound includes capture/automation overhead and does not claim the earliest complete frame. [Proposal and implementation detail](2026-10-18/sqa18-visual-20261018T072617937Z/PROPOSED-CRITERION.md).

The earlier [user Network/FCP/LCP evidence](2026-10-18/sqa18-browser-20261018T062538973Z/browser-measurements/README.md) remains historical supporting evidence with its original limitations. It was not substituted for these five visual-readiness bounds. The manual collection steps below remain a reproducible alternative, not a claim that the earlier screenshots performed every step.

## Start the isolated dashboard

From the repository root on `feature/dashboard-reporting`:

```powershell
pwsh -File tools/Pt01BrowserHost/Start-PT01Browser.ps1
```

Optional: supply `-Port <unused-local-port>` and/or `-EvidenceDirectory <new-evidence-directory>`. The default port is `51818`; use the URL and evidence location reported by the launcher. The launcher builds Release and starts a hidden background process. Leave that process running while collecting evidence.

- The helper runs the actual Web application in Development with its own fresh in-memory store. It does not seed or clear another running application's store.
- The deterministic PT-01 fixture contains exactly **2,000** results: **1,000 passed / 1,000 failed**, one API, one endpoint and **80** defects. Expected dashboard values: **50%** pass rate, **149.5 ms** average recorded API response, **48** open defects, **24** high/critical open and **12** open defects in each severity. The average recorded API response is fixture data, not a browser timing.
- The dashboard aggregates all 2,000 results and displays the newest **12** checks and **8** defects. Keep this rendering scope in the evidence.
- The fixture uses synthetic recorded data; no outbound API checks are needed. This separate host accepts GET/HEAD only and rejects edit/run-check POST actions with HTTP 405 so the fixture stays fixed. Use the normal application for manual records.
- The seeding helper is separate from the production Web application. Restarting the helper creates a new synthetic store; existing manual application processes and records must remain untouched.

## Record the environment before measuring

- [ ] Retain the launcher's evidence, build and readiness records. Record Git commit SHA **and uncommitted source identity**; a SHA alone does not identify a dirty working tree.
- [ ] Record execution date/time with timezone, tester, local URL, Release configuration, .NET SDK/runtime and OS version.
- [ ] Record CPU model, logical processors, RAM and the machine identifier agreed by the team.
- [ ] Copy the full Chrome version from `chrome://version`; record extension state/profile. Prefer a dedicated profile with extensions disabled and keep the setting unchanged for every sample.
- [ ] Fix and record viewport width/height, Chrome zoom, display scale and DevTools docking. Ensure the metric cards, severity section and recent lists are visible together at the chosen viewport; keep it unchanged. If needed, use a tall custom desktop viewport in Device Toolbar and record its dimensions/emulation settings; avoid a mobile preset or automatic throttling.
- [ ] Record power source and power mode. Keep them consistent; close unrelated heavy workloads and note remaining relevant background load.
- [ ] Record **no CPU throttling**, **no network throttling**, and **cache disabled** as observed settings, not assumptions.

The readiness smoke request has already warmed the server's MVC/Razor path. Label the first Chrome navigation as **first browser navigation, server already warmed**. These reload samples do not measure application startup or a cold server.

## Collect five browser samples

1. Open the reported dashboard URL in Chrome. Confirm all expected fixture values above and that styling loads correctly. Open DevTools and choose a layout that preserves the fixed viewport.
2. In **Network**, select **Disable cache** while DevTools is open and **No throttling**. Clear old requests before each sample. Check that the dashboard, local `css/site.css` and Bootstrap stylesheet succeed. Bootstrap from `cdn.jsdelivr.net` is a normal application dependency; retain its timing/status and note any network failure.
3. In **Performance**, enable **Screenshots**, select **No throttling** for CPU/network, then use **Record and reload**. After the complete dashboard appears, stop recording. Repeat until there are **five recorded reload samples** with the same settings.
4. For each trace, use the navigation start as zero and inspect the filmstrip and rendering events. Record the elapsed time to the first frame showing **all styled metric cards, severity distribution, recent checks and recent defects fully displayed**. Record how that frame was identified. If screenshot spacing makes the boundary uncertain, record the interval/uncertainty; do not claim a threshold pass when the available evidence cannot resolve it.
5. Save each Performance trace and a screenshot showing the relevant frame/timeline. Record the Network document timing, DOMContentLoaded and load timing as supporting values. Save the Network log as HAR when useful and retain the Bootstrap request evidence. Keep sample numbers consistent across files.
6. Record every sample, including slow samples and failures. If a run is invalid because of an identified setup problem, retain it with the reason and record its replacement separately. Do not remove an unexplained slow result.

**Measurement boundary:** navigation start to the first visually complete, styled dashboard frame. The HTML response duration, DOMContentLoaded, load event, FCP and LCP are supporting signals; none alone proves this complete-dashboard boundary. A failed/missing stylesheet or incorrect fixture prevents a valid correctness pass.

For this proposed five-sample check, correctness must hold and **every retained complete-dashboard duration must be below 2,000 ms** to meet the local proposed rule. Record the maximum and per-run values. A local PASS against the proposal does not assert formal team acceptance of the provisional requirement; do not reinterpret an average as satisfying an every-sample rule.

## Blank observation template

Copy this CSV into the evidence directory and fill it only from actual observations. Empty duration fields mean unmeasured.

```csv
Run,NavigationStartUtc,CompleteDashboardMs,Correctness,Status,TraceFile,ScreenshotFile,NetworkEvidence,Notes
1,,,,NOT RUN,,,,
2,,,,NOT RUN,,,,
3,,,,NOT RUN,,,,
4,,,,NOT RUN,,,,
5,,,,NOT RUN,,,,
```

- [ ] Attach the completed observations, environment/build identity, traces, screenshots and supporting Network evidence.
- [ ] Record observed correctness, failures, uncertainty and the actual decision against the provisional target.
- [x] Record actual browser visual-readiness execution in `SQA-18` in `docs/ShiruiFinalTestCases.csv`; current status is **PASS against the user's proposed operational criterion**, with measurements and limitations linked.
- [ ] Record formal team/stakeholder approval of the operational rule and the overall requirement/release decision. Local proposal-based PASS does not supply that approval.
- [ ] Use the collected facts as student-authored report inputs; no report prose or human decisions are supplied by this checklist.

Official Chrome references: [Performance reference](https://developer.chrome.com/docs/devtools/performance/reference), [save and load traces](https://developer.chrome.com/docs/devtools/performance/save-trace), [Network reference](https://developer.chrome.com/docs/devtools/network/reference/).
