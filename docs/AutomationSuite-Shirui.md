# Automation execution worksheet — dashboard and reports

> Technical test evidence worksheet, not final report prose. Record only observed outcomes from the actual run.

Latest merged-source validation, 19 October 2026: restore/build succeeded with 0 warnings/errors; **136/136 MSTest executions passed**, 0 failed/skipped. Actual application coverage was **876/1141 lines (76.77%)** and **325/494 branches (65.78%)**. Four new startup-isolation executions are included. All three fixture hosts built and passed loopback HTTP preparation checks with an unusable configured database path; no normal database was created. See the [integration ledger](qa-evidence/integration-20261019T092140Z/README.md), [TRX](qa-evidence/integration-20261019T092140Z/merged-suite.trx) and [coverage](qa-evidence/integration-20261019T092140Z/cobertura.xml). Historical browser/human sessions are not rerun by this regression check.

## Automated tests added

| Suite | Checks | Purpose |
|---|---|---|
| `DashboardEnhancementTests` | Severity distribution, empty distribution, unordered/tied recent items, missing timings | Deterministic regression checks for dashboard calculations and stable ordering |
| `DashboardPerformanceTests.PT01_BuildWithTwoThousandResults_UnderTwoSeconds` | First service instance call, 3 warm-ups, 20 retained samples; 2,000 deterministic results | Aggregation including store snapshots/sorting; first call and every retained sample must be strictly below 2,000 ms |
| `DashboardPerformanceTests.PT01_DashboardHttpWithTwoThousandResults_UnderTwoSeconds` | First request, 3 warm-ups, 20 retained samples; isolated loopback Kestrel host | Real application registrations, middleware, MVC and Razor through complete HTML receipt; first request and every retained sample must be strictly below 2,000 ms |
| `ReportsEnhancementTests` | API/pass filters, severity/status/API filters, all lifecycle states, independent CSV parsing, formula prefixes, malformed filters | Functional and negative-path tests for exports |
| `FailureMessageTests` | Five failed-check messages retain expectation/observed evidence | Automated wording guards; human SQA-19 observations are separately recorded with provisional approval pending |
| `ApiCheckPipelineIntegrationTests` | Two owned loopback HTTP retests and one labelled timeout simulation | Check execution, store, defects, dashboard and CSV integration |
| Existing `DashboardServiceTests` and `ReportsControllerTests` | Counts, empty results, ordering and CSV escaping | Guard against regressions in existing behaviour |

## Execute

From the repository root:

```powershell
dotnet restore SecureApiQualityPlatform.sln
dotnet build SecureApiQualityPlatform.sln -c Release --no-restore
dotnet test SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj -c Release --no-build --no-restore --logger "trx;LogFileName=test-results.trx" --results-directory TestResults --collect "XPlat Code Coverage"
```

After the user approves a commit/push, inspect **GitHub Actions -> .NET quality checks**. Download the `qa-test-results-<run-id>` artifact for TRX and Cobertura coverage files. Preserve the workflow run link and commit SHA. CI has not been executed by this review.

## Performance interpretation

PT-01 measures two separate boundaries. The aggregation check times `DashboardService.Build()`, including the store's snapshot copying and sorting. The HTTP check starts a fresh Kestrel host through the application's shared `Program.CreateApp` factory and times a loopback `GET /` until the complete HTML response body is received, including middleware, MVC and Razor rendering. Seeding, host startup, response validation and evidence writes are outside these boundaries. The HTTP first request includes opening the connection; later requests reuse the client and connection.

Each check records the first call/request, performs 3 additional warm-ups, then retains 20 measured samples. The first measurement and all 20 retained samples must each be **strictly below 2,000 ms**, and dashboard correctness assertions must pass. The first aggregation measurement is the first call on that service instance; it does not guarantee a cold process or unused JIT code. Aggregation uses a fresh private in-memory store; the merged HTTP test uses the normal SQLite registration with an explicit disposable temporary database. Both contain fixed IDs and UTC timestamps: 2,000 recorded results (1,000 passed and 1,000 failed), 1 API, 1 endpoint and 80 defects, of which 48 are open. The checks do not seed an existing running app or execute outbound API checks; the HTTP check also verifies that the fixture remains unchanged. The separate browser fixture host continues to select an isolated in-memory store before database initialization.

Run the evidence collector from the repository root:

```powershell
pwsh -NoProfile -File docs/qa-evidence/Invoke-PT01.ps1
```

Newer PT-01 execution evidence is retained in timestamped directories beginning with `pt01-` under [18 October 2026 evidence](qa-evidence/2026-10-18/). Use each run's command logs, TRX outcomes, environment record, source hashes and PT-01 measurement files together. The earlier results below belong to their original fixture and measurement method.

NFR-PERF-01 specifies **dashboard browser rendering within 2 seconds** on the agreed machine, with the target still provisional in `Requirements.md`. Aggregation and complete HTTP document receipt are separate checks. SQA-18 now has [five visual-readiness measurements](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/README.md) against the [user's proposed operational criterion](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/PROPOSED-CRITERION.md). Local result: **PASS against that proposal**; formal team/stakeholder approval of the target and rule remains pending. Earlier Network and paint observations remain in their [historical ledger](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/browser-measurements/README.md).

Browser evidence distinguishes Network Finish, DOMContentLoaded, FCP, LCP candidates and navigation-to-visual-readiness timing. Follow the [collection checklist](qa-evidence/SQA18-Browser-Performance.md) and retain actual repetitions/settings. The new readiness values are conservative screenshot-verified upper bounds; Network completion remains a separate measurement.

## Automated browser visual-readiness execution — 18 October 2026

| Item | Recorded result |
|---|---|
| Operational rule provenance | User-proposed; not formally approved by the team/stakeholders; existing NFR-PERF-01 target remains provisional |
| Fixture/correctness | Exactly 2,000 deterministic results; correct dashboard statistics and severity distribution; fixture unchanged before/after |
| Boundary | Navigation start to conservative screenshot-verified visual-readiness upper bound, including stylesheet loading, layout and paint readiness |
| Five independent reloads (ms) | **257.9, 199.5, 197.1, 189.7, 201.0** |
| Median / maximum (ms) | **199.5 / 257.9** |
| Browser / automation | Headed Chrome **155.0.8059.80**; Playwright **1.62.1** |
| Viewport / screenshots | Fixed **1440 × 1600**; all five screenshots inspected for complete/correct dashboard content |
| Local SQA-18 decision | **PASS against the user's proposed criterion**: every measured upper bound is below 2,000 ms and dashboard values are correct |
| Evidence | [Run ledger, environment, settings and screenshots](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/README.md); [proposed criterion](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/PROPOSED-CRITERION.md) |
| Approval/release | Formal requirement/rule approval and team release decision remain pending; no application functionality changed for this browser execution |

## Earlier user browser evidence received — 18 October 2026

| Observation | Recorded result |
|---|---|
| Five user-reported Network Finish samples | 190, 55, 55, 55, 41 ms; median 55 ms; maximum 190 ms |
| Network screenshots' DOMContentLoaded | 15 ms each; cache disabled and no network throttling visible |
| Separate Performance trace | Recording start 19:50:43.535 NZDT; matched-navigation FCP 89.415 ms and one LCP candidate 89.415 ms |
| Provenance | Six PNG attachments retained; four unique hashes; 03/04/06 byte-identical; no pairing of trace to a Network run |
| Environment | Development-only Release MVC host at `http://127.0.0.1:51818/`; 2,000 deterministic results; isolated from five manual results |
| As-of status of this earlier evidence | ACCEPTANCE PENDING; those Network/paint observations did not establish complete-dashboard visual timing; later automated run above supplies the current local result |

[Raw files, hashes, extractor and limitations](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/browser-measurements/README.md). The [19:29 setup run](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/README.md) previously passed 76/76 MSTest checks with coverage; it was not rerun during this evidence update. Historical run descriptions below retain their original timestamps and results.

## Manual usability verification

Use keyboard only to reach navigation, links, defect controls, and CSV actions. At narrow browser width, check the metric cards and recent checks table for clipping or overlap. Inspect whether failed-check evidence is readable and whether empty states display meaningful messages. Ask a representative tester to explain five failure messages. Record actual observations and screenshots; do not mark usability as passed based only on added markup.

## PT-01 rerun — 18 October 2026, 18:38 NZDT

[Validated execution ledger](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md): Release build passed with 0 warnings/errors; that regression suite passed 71/71 without coverage; dedicated PT-01 checks 2/2 passed. Aggregation first call 21.4494 ms, warm median 0.7386 ms, P95 0.9888 ms, maximum 8.7894 ms. HTTP first request 223.0098 ms, warm median 1.8280 ms, P95 2.4933 ms, maximum 2.6382 ms. Every first and retained sample was below 2,000 ms. Browser evidence was NOT RUN at that execution; later observations are recorded above.

All raw JSON/HTML/TRX files were retained and verified. The final invocation used `-SkipCoverage`: an earlier coverage attempt was blocked by local Windows Application Control while loading the instrumented Web assembly; its failure log is retained. Final-run coverage is NOT COLLECTED. The historical evidence below is preserved with its original source/run identity.

## Earlier observed local execution — 18 October 2026 (NZDT)

| Item | Value / link |
|---|---|
| Tested source | HEAD `1505cc9f1d60687111765aec1355ec1e9f87b2a9` plus uncommitted changes on `feature/dashboard-reporting`; [source hashes](qa-evidence/2026-10-18/tested-source-sha256.csv) |
| Build | Release solution build passed; 0 warnings, 0 errors |
| GitHub Actions run URL | NOT RUN; approved commit/push and .NET 8 SDK CI verification pending |
| Automated tests planned / executed / passed / failed / blocked / not run | 70 / 70 / 70 / 0 / 0 / 0; [TRX](qa-evidence/2026-10-18/final.trx) |
| Baseline copied patch | Debug build passed; 30/30 MSTest executions passed; [baseline TRX](qa-evidence/2026-10-18/baseline.trx) |
| Separate HTTP smoke | 8/8 requests passed; actual MVC/Razor no-data output and filter binding; [results](qa-evidence/2026-10-18/http-smoke.json) |
| PT-01 measured duration (ms) | 0.91 without coverage; 1.20 in coverage run; one warmed measured Build() per run; [performance TRX](qa-evidence/2026-10-18/performance.trx) |
| Browser dashboard render time for 2,000 results (ms) | NOT MEASURED |
| Local coverage | Web assembly: 402/629 instrumented lines (63.91%), 183/390 branches (46.92%); [Cobertura](qa-evidence/2026-10-18/cobertura.xml) |
| Chrome / Edge keyboard and responsive inspection | NOT RUN |
| Execution actor and human review | Codex automated execution; student/team review pending |
| Environment | Windows 10.0.26200 x64; Intel i7-12700H; 16,834,023,424 bytes RAM; SDK 10.0.400, runtime .NET 8.0.30; Release net8.0 |
| Tool versions | MSBuild 18.9.6; Microsoft.NET.Test.Sdk 17.11.1; MSTest 3.6.4; coverlet.collector 6.0.2; Git 2.45.1.windows.1 |
| Final suite time | 2026-10-18 17:56:50–17:56:58 NZDT; [environment](qa-evidence/2026-10-18/environment.json), [SDK](qa-evidence/2026-10-18/dotnet-info.txt) |

The final evidence register uses **SQA-01–SQA-22** to avoid reusing the initial `TC-*` identifiers with different meanings. Its `Initial case mapping` column maps back to `docs/TestCases.md`. The team's RTM remains unchanged pending coordination. Reconciled register totals after integrating main: **22 planned, 16 executed, 14 PASS, 1 PASS (PROVISIONAL), 1 FAIL, 0 blocked, 6 not run**. These are acceptance/evidence cases, separate from MSTest executions. SQA-18 PASS is against the user's proposed operational criterion, with formal NFR-PERF-01 approval pending. SQA-19 has 5/5 provisional human-scenario passes with ChatGPT wording assistance disclosed and final Shirui approval pending. SQA-20 remains FAIL despite 16/16 technical browser passes because label and contrast findings remain open. See the [master register](ShiruiFinalTestCases.csv), [current metrics](ShiruiQualityMetrics-Evidence.md), [SQA-18 browser run](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/README.md) and [SQA-19 assessment](qa-evidence/SQA-19/observer-assessment-session-eilmar.md); earlier run ledgers retain their as-of totals.

The owned integration fixture listens only on ephemeral loopback ports and returns controlled HTTP 503 then 200 responses. Test-only private-target permission does not change app defaults. HTTP intentionally produces HTTPS-transport failures in the fixture; those expected results are not failing MSTest cases. The timeout case uses a handler simulation and does not measure a real socket timeout. No external API was scanned.

Automation limitations: the HTTP performance check exercises the real MVC/Razor HTML document response, but it does not establish browser layout/paint performance, accessibility, security, cross-browser compatibility, or persisted state across restarts. A performance threshold may vary by hardware and CI load. Rerun failures before treating them as product defects.
