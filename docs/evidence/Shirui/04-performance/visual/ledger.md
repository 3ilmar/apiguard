# SQA-18 visual-readiness execution — 18 October 2026

Local result: **PASS against the user's proposed operational acceptance criterion v1**. **Not formally approved by the team.** NFR-PERF-01 remains provisional/TBC in `docs/Requirements.md`; no team approval, RTM approval or release decision is asserted here. This is factual QA evidence, not submitted report prose.

## Proposed criterion and measured boundary

The user proposed: exactly 2,000 deterministic results; correct dashboard statistics/severities; navigation start to populated visual readiness including CSS/layout/paint; five independent reloads under consistent conditions; PASS only if every visual-readiness duration is below 2,000 ms and the values are correct. The full proposal and its test implementation were recorded before execution in [PROPOSED-CRITERION.md](PROPOSED-CRITERION.md).

**Measured endpoint:** navigation-relative `performance.now()` immediately after Playwright returns a captured viewport screenshot. Capture occurs only after document load, stylesheet availability and computed-style checks, fonts ready, correct values/lists, all content inside the viewport, and stable geometry over three consecutive animation-frame callbacks. The screenshot confirms pixels were rendered. This is a **conservative verified visual-readiness upper bound** including screenshot capture/encoding/automation overhead; it is not the first visual-ready instant, Network Finish, FCP or LCP.

## Actual five independent reloads

All values below are in milliseconds from that reload's navigation time origin. Presentation is rounded to one decimal; original floating-point values remain in [results.json](results.json) and each `measurement.json`. The browser collector leaves raw results at `SCREENSHOT REVIEW PENDING`; the subsequent [decision record](decision.json) incorporates actual screenshot inspection and evidence review.

| Run | Navigation start (UTC) | Layout-ready checkpoint (supporting only) | Screenshot-verified readiness upper bound | Result under proposal | Screenshot / measurement / trace |
|---|---|---:|---:|---|---|
| 1 | 2026-10-18T07:28:17.605Z | 176.7 | **257.9** | PASS | [PNG](run-1/dashboard.png), [JSON](run-1/measurement.json), [trace](run-1/performance-trace.json) |
| 2 | 2026-10-18T07:28:18.159Z | 106.8 | **199.5** | PASS | [PNG](run-2/dashboard.png), [JSON](run-2/measurement.json), [trace](run-2/performance-trace.json) |
| 3 | 2026-10-18T07:28:18.630Z | 101.8 | **197.1** | PASS | [PNG](run-3/dashboard.png), [JSON](run-3/measurement.json), [trace](run-3/performance-trace.json) |
| 4 | 2026-10-18T07:28:19.092Z | 90.8 | **189.7** | PASS | [PNG](run-4/dashboard.png), [JSON](run-4/measurement.json), [trace](run-4/performance-trace.json) |
| 5 | 2026-10-18T07:28:19.554Z | 112.5 | **201.0** | PASS | [PNG](run-5/dashboard.png), [JSON](run-5/measurement.json), [trace](run-5/performance-trace.json) |
| Median | | | **199.5** | | |
| Maximum | | | **257.9** | | |

Execution window: **20:28:16.957–20:28:20.040 NZDT** / `07:28:16.957–07:28:20.040Z`. Five actual `page.reload()` navigations in five distinct browser contexts/pages were retained. Each context first made an unmeasured preparation navigation, then cleared/disabled cache before its retained reload. No measured sample was discarded or replaced. Preparation and browser launch are outside the navigation boundary.

## Correctness and screenshot observations

| Check | Actual observation in every run |
|---|---|
| Underlying results | Exactly **2,000**, confirmed by result export count before/after and deterministic fixture fingerprint |
| Cards | 1 API; 1 endpoint; 2,000 executed; **50%** pass rate; **1,000 / 1,000** passed/failed; **149.5 ms** average recorded API timing; **48** open defects; **24** high/critical open |
| Severity distribution | Critical/High/Medium/Low: **12 each**, `aria-valuenow=12`, `aria-valuemax=48`, measured bar fraction 25% |
| Recent lists | Correct newest fixture records, **12 checks** and **8 defects**; every row fits in the viewport |
| Stylesheets | Bootstrap 5.3.3 from jsDelivr and local site CSS both HTTP 200; sheet objects and expected computed styles present |
| Layout/font checks | `document.fonts.status=loaded`; expected site background/cards/Bootstrap flex layout; stable geometry and no viewport overflow |
| Pixel evidence | All five 1440 × 1600 viewport PNGs inspected by Codex; all cards, all four severity bars and both complete recent lists are visible, styled and legible without clipping |
| State preservation | Fixture before/after result and defect CSV hashes match; **80** defects retained; no API checks or edits submitted |

The **149.5 ms** card is synthetic API-check data, not a browser-readiness result. Identical screenshot pixels are expected for this deterministic static dashboard; independence is established by separate contexts and distinct navigation timestamps/traces, not by requiring different image hashes.

## Recorded environment and settings

| Item | Actual recorded value |
|---|---|
| Browser | **Google Chrome 155.0.8059.80**, headed; `Browser.getVersion` product `Chrome/155.0.8059.80`, protocol 1.3 |
| Automation | **Playwright 1.62.1**, Node **v24.19.0**, Chrome DevTools Protocol tracing enabled during every retained sample |
| Viewport | **1440 × 1600 CSS pixels**, device scale factor **1**; screenshot and actual `innerWidth`/`innerHeight` confirmed |
| Browser state | Fresh incognito context/page per sample in a separate automation Chrome process; extensions disabled; user Chrome profile not reused |
| Cache/network | Browser cache cleared before each retained reload and disabled; service workers blocked/bypassed; no network throttle (0 latency, unlimited throughput) |
| CPU / locale | CPU throttle rate **1**; locale **en-NZ**, timezone **Pacific/Auckland** |
| OS / hardware | Windows **10.0.26200**; Intel **Core i7-12700H**; **20** logical processors; **16,834,023,424 bytes RAM** |
| Power | Active scheme **Balanced**; raw battery status **2**, charge **100%** at setup; see exact fields in environment JSON |
| Other load | Existing desktop applications/background load were not closed or controlled; no claim of an idle machine |
| Application | Real ASP.NET Core MVC/Razor, **Release net8.0**, runtime **.NET 8.0.30 x64**, SDK **10.0.400**; warmed local Development server at `http://127.0.0.1:51818/` |
| Source | `feature/dashboard-reporting`, HEAD `ced44546ba9c71f25935e6ba913b27fe08f0acd1` plus uncommitted source identified by the setup [manifest](../sqa18-browser-20261018T062538973Z/live-instance/source-sha256.csv); runner snapshot retained here |
| Fixture fingerprint | `CCBE6723FB603221C3CECB95BB176BD6426E8F7611B96D3AC0565A1D7927AC18` |

[Environment JSON](environment.json), [runner snapshot](runner-source.cjs), [raw collector output](runner-output.txt), [screenshot review](visual-review.json). Each run directory also contains the actual HTML and a `network.har`; HAR includes the clearly described unmeasured preparation navigation as well as the retained reload. Its aggregate page timings are preparation-relative and must not be used as retained reload timings. The corresponding Performance trace starts after preparation and before the reload; [decision.json](decision.json) matches the navigation ID to the verified endpoint and confirms paint/filmstrip events precede it.

## Reproduction and evidence limits

With the separate PT-01 host already running, from the repository root:

```powershell
node tools/Pt01BrowserHost/Measure-PT01Browser.cjs --evidence-directory <new-directory> --playwright-module <installed-playwright-directory> --chrome "C:/Program Files/Google/Chrome/Application/chrome.exe"
```

The runner refuses to overwrite an existing `results.json`, never changes Web source/data, and closes only its automation browser. It requires screenshot review before a separate acceptance record is made. The server remains independent of the normal application and its five manual results.

Final [read-only isolation check](isolation-after.json): the fixture still returns HTTP 200 with **2,000** executed checks. The original `https://localhost:49994/` endpoint was unavailable (connection refused) at that check, so its current result count could not be reconfirmed. Neither application was started/stopped or written to by this measurement task; no inference about the original process's in-memory contents is made.

- This passes the **user-proposed operational rule on this machine/viewport**. Formal team approval and the overall release decision remain **pending**; Requirements.md and the RTM are unchanged.
- The endpoint includes capture/automation overhead. It bounds verified readiness; it does not identify the earliest complete frame. Trace/har collection adds overhead consistently to all samples.
- Browser navigation is local HTTP against a warmed server. This does not measure startup, remote-user latency, production TLS, concurrent load, alternative viewports or cross-browser performance.
- Cache disabled does not make DNS/OS/CDN caches cold. External jsDelivr latency remains part of the browser workload; existing desktop load was uncontrolled.
- Earlier [user Network observations and one FCP/LCP trace](../sqa18-browser-20261018T062538973Z/browser-measurements/README.md) remain historical supporting evidence. They were not relabelled as these five visual-readiness measurements, and their missing browser metadata was not filled from this newer run.
- Five timing samples are observations within one SQA case. No other test-case status is changed, and no MSTest/coverage run is claimed for this browser execution.

Automation references: [Playwright Page](https://playwright.dev/docs/api/class-page), [Chrome Network controls](https://chromedevtools.github.io/devtools-protocol/tot/Network/), [Chrome CPU throttling](https://chromedevtools.github.io/devtools-protocol/tot/Emulation/), [Chrome tracing](https://chromedevtools.github.io/devtools-protocol/tot/Tracing/).
