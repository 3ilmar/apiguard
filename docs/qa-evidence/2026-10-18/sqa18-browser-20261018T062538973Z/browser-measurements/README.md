# SQA-18 / PT-01 browser measurement evidence — 18 October 2026

Historical observation ledger. Later five-reload visual execution: **PASS against the user's proposed operational criterion**, not formal team approval; see [new execution and decision](../../sqa18-visual-20261018T072617937Z/README.md). The pending status, measurements and missing metadata below describe this earlier evidence only and are preserved.

Status: **EXECUTED — ACCEPTANCE PENDING**. Five Network runs were reported by the user; one exported Performance trace was supplied. Complete-dashboard visual-render time remains unmeasured. No PASS/FAIL or team release decision is assigned. This is a factual evidence ledger, not submitted report prose.

## Recorded Network observations

Source: the user's reported run order and attached Chrome DevTools screenshots. Screenshot labels are rounded displayed values, not higher-precision raw timings. Per-run capture timestamps and HAR files were not supplied.

| Reported run | Network Finish (ms) | DOMContentLoaded (ms) | Complete dashboard visually rendered (ms) |
|---|---:|---:|---|
| 1 | 190 | 15 | Not measured |
| 2 | 55 | 15 | Not measured |
| 3 | 55 | 15 | Not measured |
| 4 | 55 | 15 | Not measured |
| 5 | 41 | 15 | Not measured |
| Median Network Finish | **55** | | |
| Maximum Network Finish | **190** | | |

[Machine-readable observations](../browser-observations.csv). Values reproduce the user's measurements; no synthetic timing samples are added. All five reported Network Finish values are below 2,000 ms. This comparison concerns network completion only and does not establish the browser-render acceptance criterion.

## Attachment provenance

The six original PNG attachments and original trace were copied byte-for-byte into this directory; their source paths, byte lengths and SHA-256 values are in [source-manifest.json](source-manifest.json). Original files remain untouched.

| Attachment | Displayed Finish / DCL (ms) | Provenance note |
|---|---|---|
| [01](network-attachment-01.png) | 190 / 15 | Distinct file |
| [02](network-attachment-02.png) | 55 / 15 | Distinct file |
| [03](network-attachment-03.png) | 55 / 15 | Byte-identical to 04 and 06 |
| [04](network-attachment-04.png) | 55 / 15 | Duplicate retained as supplied |
| [05](network-attachment-05.png) | 41 / 15 | Distinct file |
| [06](network-attachment-06.png) | 55 / 15 | Duplicate retained as supplied |

There are four unique PNG hashes. The attachments support the displayed 190/55/41-ms values but do not independently identify five separate captures. Five-run order/count is attributed to the user's execution report. No trace-to-Network-run correspondence is established.

Visible screenshot conditions: URL `127.0.0.1:51818`; **Disable cache checked**, **No throttling** selected in Network, three requests (document, Bootstrap CSS and local CSS), each HTTP 200. Screenshot content confirms 2,000 executed checks, 1,000 passed/1,000 failed, 50% pass rate, 48 open defects, 24 high/critical open and 12 open per severity. The displayed **149.5 ms average response** is seeded API-check data, not browser performance.

## Independently extracted Performance trace

Source: [Trace-20261018T195043.json](Trace-20261018T195043.json), SHA-256 `AAB8AB51895D7DA096A6047D35529797FB16FE52C6C9C5E77175075063A502ED`. DevTools metadata recording start: `2026-10-18T06:50:43.535Z` / **19:50:43.535 NZDT**. This is recording start, not a reconstructed capture timestamp for each Network run.

| Trace observation | Actual value / boundary |
|---|---|
| Target URL | `http://127.0.0.1:51818/` |
| Navigation ID | `42532C746BF7A9AD46C5872D1EC04ACB` |
| Target `navigationStart.ts` | `287376359086` microseconds on trace monotonic clock |
| FCP | `(287376448501 - 287376359086) / 1000` = **89.415 ms**, approximately 89.4 ms |
| Observed LCP candidate | Same delta: **89.415 ms**; sole `largestContentfulPaint::Candidate`, index 1, text paragraph `P class='text-muted small mb-0'`; finalization not verified |
| Trace DCL start/end | **13.218 / 13.232 ms** after target navigation start |
| Trace load start/end | **47.605 / 47.610 ms** after target navigation start |
| Resources | HTML, local CSS and jsDelivr Bootstrap CSS all HTTP 200; `fromCache=false`, `fromServiceWorker=false` |
| Fixture verification | Document response includes `X-PT01-Fixture-SHA256: CCBE6723FB603221C3CECB95BB176BD6426E8F7611B96D3AC0565A1D7927AC18` |
| Full dashboard visual completion | **Not established** |

These DCL/load numbers belong to the separate trace navigation. They do not replace the five screenshots' 15-ms DCL values. The extractor excludes the earlier blank navigation and matches FCP/LCP to the target navigation ID and frame. Raw relevant event fields are retained in [trace-analysis.json](trace-analysis.json).

Reproduce offline with Node.js (extracted using **v24.19.0**):

```powershell
node docs/qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/browser-measurements/analyze-trace.cjs
```

## Timing boundaries

| Metric | What is recorded | What it does not establish |
|---|---|---|
| Network Finish | Network panel's displayed completion summary for the captured request set; HTML and stylesheet activity included | Completion of layout, paint, or all dashboard content; no raw HAR was supplied to independently reconstruct the five request timelines |
| DOMContentLoaded | DOM lifecycle event shown in Network; separate trace event deltas also retained | Stylesheet/image completion or visually complete rendering |
| FCP | First content paint event relative to the matched dashboard navigation start | All cards, tables and defects rendered |
| LCP candidate | Observed largest eligible content candidate visible in that viewport | Finalized LCP or all content, including below the fold, rendered |
| Required browser acceptance | Team must agree which browser metric, viewport/content scope, repetitions and rule define the provisional below-2,000-ms requirement | No acceptance inferred from Network Finish or one paint sample |

Definitions: [Chrome Network event reference](https://developer.chrome.com/docs/devtools/network/reference/), [FCP](https://web.dev/articles/fcp), [LCP and viewport/candidate behaviour](https://web.dev/articles/lcp). The [collection checklist](../../../SQA18-Browser-Performance.md) proposes navigation start to a fully styled/populated dashboard frame; that boundary has not been timed in the supplied observations.

## Isolated environment and source identity

- Development-only standalone `tools/Pt01BrowserHost`, actual ASP.NET Core MVC/Razor application, Release `net8.0`, runtime **.NET 8.0.30 x64**, SDK **10.0.400**. Branch `feature/dashboard-reporting`; HEAD `ced44546ba9c71f25935e6ba913b27fe08f0acd1` plus uncommitted changes identified by [source hashes](../live-instance/source-sha256.csv).
- Host setup hardware: **Windows 10.0.26200**, Intel **i7-12700H**, **20 logical processors**, **16,834,023,424 bytes RAM**. These come from the setup environment, not from invented Chrome metadata. See [handover verification](../handover-verification.json) and [instance metadata](../live-instance/instance.json).
- Fresh private in-memory store: exactly **2,000 deterministic results**, one API/endpoint, **80 defects**, including 48 open. Fixed IDs/timestamps; no live outbound API scans. Dashboard aggregates all records and renders only the latest **12 checks / 8 defects**.
- Loopback `http://127.0.0.1:51818/`; normal `https://localhost:49994/` application and its five manual results remain separate. No production seed route/configuration; helper is non-publishable and rejects writes. This evidence update does not change or restart either application.
- Server MVC/Razor path was warmed by earlier readiness requests. Browser cache disabled does not imply cold server/JIT, empty OS/DNS caches, fresh connections, or uncached CDN infrastructure. Trace document connection was reused; Bootstrap remains an external dependency.

## Remaining limitations / team inputs

- Full Chrome version, tester name, per-Network-run timestamps, CPU throttle setting, power mode, extension state, exact browser zoom and agreed machine identity were not supplied. Do not infer them from the DevTools release-news pane or from current installed software.
- Trace raw viewport: `590 × 1318`, `isActive=false`; metadata `hostDPR=1`. Paint viewport rectangle is `575 × 1318`, `dpr=0.800000011920929`. These fields do not establish exact browser zoom/display scaling or identical geometry across all reported runs.
- Eight filmstrip events are present. The frame at **89.941 ms** shows the metrics and severity section with only the recent-checks heading near the bottom; full recent lists/defects are below the visible region. A timed complete-dashboard visual boundary is not established.
- One trace is not five paint measurements; the repeated Network samples do not supply repeated FCP/LCP values. No complete-render median/max is calculated.
- This is one local warmed-server scenario, with no production TLS, remote users, cold application startup or concurrent-load evidence. It does not establish accessibility or cross-browser acceptance; SQA-20 remains unchanged.
- Team action: define/confirm NFR-PERF-01's metric and acceptance rule, obtain any missing environment/visual-boundary evidence required by that definition, then record the team's decision. **Acceptance and release decision remain pending.**

Current SQA register totals: **22 planned; 14 executed/attempted (including this partially measured case); 13 passed; 0 failed; 1 acceptance pending; 0 blocked; 8 not run**. Every other test-case status is unchanged. Timing observations are not additional test cases.

Evidence validation: [verification.json](verification.json) records seven matching original/copy hashes, four unique PNG hashes, 50 unchanged launched source files, 21 unchanged other case rows, five observations and 45 valid local links. These are evidence-integrity checks, not browser acceptance results. No build or automated suite was rerun for this evidence-only update.
