# Automation execution worksheet — dashboard and reports

> Technical test evidence worksheet, not final report prose. Record only observed outcomes from the actual run.

## Automated tests added

| Suite | Checks | Purpose |
|---|---|---|
| `DashboardEnhancementTests` | Severity distribution, empty distribution, unordered/tied recent items, missing timings | Deterministic regression checks for dashboard calculations and stable ordering |
| `DashboardPerformanceTests.PT01_BuildWithTwoThousandResults_UnderTwoSeconds` | First service instance call, 3 warm-ups, 20 retained samples; 2,000 deterministic results | Aggregation including store snapshots/sorting; first call and every retained sample must be strictly below 2,000 ms |
| `DashboardPerformanceTests.PT01_DashboardHttpWithTwoThousandResults_UnderTwoSeconds` | First request, 3 warm-ups, 20 retained samples; isolated loopback Kestrel host | Real application registrations, middleware, MVC and Razor through complete HTML receipt; first request and every retained sample must be strictly below 2,000 ms |
| `ReportsEnhancementTests` | API/pass filters, severity/status/API filters, all lifecycle states, independent CSV parsing, formula prefixes, malformed filters | Functional and negative-path tests for exports |
| `FailureMessageTests` | Five failed-check messages retain expectation/observed evidence | Automated wording guards; human usability remains unverified |
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

Each check records the first call/request, performs 3 additional warm-ups, then retains 20 measured samples. The first measurement and all 20 retained samples must each be **strictly below 2,000 ms**, and dashboard correctness assertions must pass. The first aggregation measurement is the first call on that service instance; it does not guarantee a cold process or unused JIT code. Each check creates a fresh private in-memory store containing fixed IDs and UTC timestamps: 2,000 recorded results (1,000 passed and 1,000 failed), 1 API, 1 endpoint and 80 defects, of which 48 are open. The checks do not seed an existing running app or execute outbound API checks; the HTTP check also verifies that the fixture remains unchanged.

Run the evidence collector from the repository root:

```powershell
pwsh -NoProfile -File docs/qa-evidence/Invoke-PT01.ps1
```

Newer PT-01 execution evidence is retained in timestamped directories beginning with `pt01-` under [18 October 2026 evidence](qa-evidence/2026-10-18/). Use each run's command logs, TRX outcomes, environment record, source hashes and PT-01 measurement files together. The earlier results below belong to their original fixture and measurement method.

NFR-PERF-01 specifies **dashboard browser rendering within 2 seconds** on the agreed machine, with the target still provisional in `Requirements.md`. Aggregation and complete HTTP document receipt are separate checks; neither measures browser CSS loading, layout or paint. Browser rendering has not been measured and must remain unverified until that boundary is executed.

Suggested browser evidence: open developer tools -> Network, disable cache, load the dashboard with 2,000 prepared results, and record the document load/render timing. Repeat the measurement and state the machine, build, browser, cache setting and data preparation method. Do not test external APIs without authorisation.

## Manual usability verification

Use keyboard only to reach navigation, links, defect controls, and CSV actions. At narrow browser width, check the metric cards and recent checks table for clipping or overlap. Inspect whether failed-check evidence is readable and whether empty states display meaningful messages. Ask a representative tester to explain five failure messages. Record actual observations and screenshots; do not mark usability as passed based only on added markup.

## PT-01 rerun — 18 October 2026, 18:38 NZDT

[Validated execution ledger](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md): Release build passed with 0 warnings/errors; final regression suite 71/71 passed without coverage; dedicated PT-01 checks 2/2 passed. Aggregation first call 21.4494 ms, warm median 0.7386 ms, P95 0.9888 ms, maximum 8.7894 ms. HTTP first request 223.0098 ms, warm median 1.8280 ms, P95 2.4933 ms, maximum 2.6382 ms. Every first and retained sample was below 2,000 ms. Browser rendering remains NOT RUN.

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

The final evidence register uses **SQA-01–SQA-22** to avoid reusing the initial `TC-*` identifiers with different meanings. Its `Initial case mapping` column maps back to `docs/TestCases.md`. The team's RTM remains unchanged pending coordination. Register totals: **22 planned, 13 executed, 13 passed, 0 failed, 0 blocked, 9 not run**. These are acceptance/evidence cases, separate from the 70 MSTest executions. See [master register](ShiruiFinalTestCases.csv) and [run details](qa-evidence/2026-10-18/README.md).

The owned integration fixture listens only on ephemeral loopback ports and returns controlled HTTP 503 then 200 responses. Test-only private-target permission does not change app defaults. HTTP intentionally produces HTTPS-transport failures in the fixture; those expected results are not failing MSTest cases. The timeout case uses a handler simulation and does not measure a real socket timeout. No external API was scanned.

Automation limitations: the HTTP performance check exercises the real MVC/Razor HTML document response, but it does not establish browser layout/paint performance, accessibility, security, cross-browser compatibility, or persisted state across restarts. A performance threshold may vary by hardware and CI load. Rerun failures before treating them as product defects.
