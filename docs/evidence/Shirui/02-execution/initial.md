# Local QA execution evidence — 18 October 2026

Later PT-01 implementation/execution is recorded in the [18:38 NZDT validated execution ledger](pt01-20261018T053834379Z-eb0590/README.md): 71/71 regression checks and 2/2 dedicated aggregation/HTTP checks passed, with deterministic data and raw measurement evidence. The records below describe the earlier run and remain unchanged as historical evidence. Browser rendering remains unverified.

Factual execution records and student report inputs only. All times below are Pacific/Auckland (NZDT, UTC+13). This is not final assessment report prose or a human release approval.

## Source and environment

| Item | Observed value |
|---|---|
| Branch | `feature/dashboard-reporting` |
| HEAD | `1505cc9f1d60687111765aec1355ec1e9f87b2a9` |
| Tested state | HEAD plus uncommitted implementation/test changes; not a committed release |
| Source identity | [SHA-256 file manifest](tested-source-sha256.csv); includes solution, app/test source, configuration, workflow and HTTP smoke script |
| OS / CPU | Microsoft Windows 10.0.26200 x64 / 12th Gen Intel Core i7-12700H, 14 cores, 20 logical processors |
| RAM | 16,834,023,424 bytes |
| SDK / MSBuild | 10.0.400 / 18.9.6+14fbf8d52 |
| Runtime actually logged by PT-01 | .NET 8.0.30, X64 |
| Configuration / framework | Release / net8.0; baseline was Debug |
| Tests / coverage | Microsoft.NET.Test.Sdk 17.11.1; MSTest 3.6.4; coverlet.collector 6.0.2 |
| Other tools | Git 2.45.1.windows.1; HTTP smoke uses PowerShell and .NET HttpClient |
| Executor | Codex agent; human evidence review pending |
| CI identity | No CI run was triggered; run URL and submitted commit pending approved commit/push |

Detailed [environment record](environment.json) and [dotnet --info output](dotnet-info.txt) are retained. The sandbox initially blocked SDK service access and restore failed without a compiler diagnostic; the requested checks succeeded when run outside that sandbox. No project change was needed for that environment issue. Only SDK 10 is installed locally; the existing workflow uses SDK 8. Local results therefore do not establish SDK 8 CI parity.

Initial working-tree inventory before edits:

- Six modified tracked files: `.github/workflows/dotnet.yml`; `SecureApiQualityPlatform.Web/Controllers/ReportsController.cs`; `SecureApiQualityPlatform.Web/Services/DashboardService.cs`; `SecureApiQualityPlatform.Web/ViewModels/DashboardViewModel.cs`; `SecureApiQualityPlatform.Web/Views/Home/Index.cshtml`; `SecureApiQualityPlatform.Web/wwwroot/css/site.css`.
- Seven untracked files: `AGENTS.md`; `SecureApiQualityPlatform.Tests/DashboardEnhancementTests.cs`; `SecureApiQualityPlatform.Tests/DashboardPerformanceTests.cs`; `SecureApiQualityPlatform.Tests/ReportsEnhancementTests.cs`; `docs/AutomationSuite-Shirui.md`; `docs/ShiruiFinalTestCases.csv`; `docs/ShiruiQualityMetrics-Evidence.md`.
- No staged changes. The entire tracked diff and these untracked files were reviewed before implementation edits. This review subsequently added `FailureMessageTests.cs`, `ApiCheckPipelineIntegrationTests.cs` and this evidence directory; it did not modify `AGENTS.md`.

## Actual execution

| Check | Planned | Executed | Passed | Failed | Blocked | Not run | Evidence |
|---|---:|---:|---:|---:|---:|---:|---|
| Copied-patch baseline MSTest, Debug | 30 | 30 | 30 | 0 | 0 | 0 | [baseline.trx](baseline.trx) |
| Final MSTest with coverage, Release | 70 | 70 | 70 | 0 | 0 | 0 | [final.trx](final.trx), [console](test-release.txt) |
| Separate PT-01 without coverage | 1 | 1 | 1 | 0 | 0 | 0 | [performance.trx](performance.trx), [console](test-performance.txt) |
| Actual application HTTP smoke requests | 8 | 8 | 8 | 0 | 0 | 0 | [http-smoke.json](http-smoke.json) |
| SQA evidence/acceptance register | 22 | 13 | 13 | 0 | 0 | 9 | [master cases](../../ShiruiFinalTestCases.csv) |

These rows have different counting units and overlap; do not add them to create a project-wide test total. MSTest data rows count as separate executions. The final suite contains 70 executions: CheckEvaluator 8, DashboardService 4, DashboardEnhancement 7, DashboardPerformance 1, DefectLifecycle 2, FailureMessage 5, RegistrationValidation 4, ReportsController 3, ReportsEnhancement 32, UrlSafety 1, ApiCheckPipelineIntegration 3.

| Step | Result / time |
|---|---|
| `dotnet --info` | Recorded successfully outside sandbox |
| `dotnet restore SecureApiQualityPlatform.sln` | Passed; [restore output](restore.txt) |
| `dotnet build SecureApiQualityPlatform.sln --configuration Release --no-restore` | Passed, 0 warnings / 0 errors; [build output](build-release.txt) |
| Final suite | 2026-10-18 17:56:50–17:56:58 NZDT, per TRX start/finish |
| Separate PT-01 | 2026-10-18 17:57:24–17:57:25 NZDT, per TRX start/finish |
| HTTP smoke | Completed 2026-10-18 17:55:25 NZDT |

## Measurements and scope

| Metric | Observation | Practical limit |
|---|---|---|
| PT-01, no coverage | **0.91 ms** for the measured `DashboardService.Build()` | One measured call after one warm-up; seeding, process startup, Razor, network and browser excluded |
| PT-01, coverage enabled | **1.20 ms** in the final suite | Instrumented run; not directly interchangeable with the no-coverage timing |
| Line coverage | **402 / 629 = 63.91%** | Instrumented executable lines in `SecureApiQualityPlatform.Web`, not requirement coverage |
| Branch coverage | **183 / 390 = 46.92%** | Instrumented branches in the same assembly |
| Master register execution rate | **13 / 22 = 59.09%** | Nine cases remain not run |
| Master register pass rate | **13 / 13 = 100%** | Executed cases only; not a release decision |

The original [Cobertura XML](cobertura.xml) is retained. PT-01 seeds 2,000 in-memory check results: 1,000 pass, 1,000 fail, 667 have no response time, 1,333 have timing values `100 + index % 100` ms. Timestamps are a fixed origin plus index seconds. There are no registered APIs/endpoints or defects in that benchmark fixture. The store's snapshot sorting is included in `Build()` timing. The bounded recent-item selection does not remove that store sorting. This measurement passes the service microbenchmark's 2,000 ms threshold; **NFR-PERF-01 browser rendering remains unverified**. The recorded average response time is per timed check result, not an independent endpoint measurement.

The real integration test creates an owned Kestrel fixture on `127.0.0.1` with ephemeral ports, controlled 503 then 200 replies and test-only private-target permission. It checks defect creation, closure from both Resolved and Retest, dashboard counts and filtered CSV lifecycle evidence. HTTP produces intentional HTTPS-transport failures; these are expected fixture evidence, not failed tests or newly discovered production defects. The third integration execution simulates a transport timeout using `HttpMessageHandler` and then succeeds; no real elapsed socket timeout was measured. No external API was scanned and production target protection was not changed.

The HTTP smoke starts a fresh app process, exercises actual MVC query binding and Razor output, and stops that process. It observes `N/A` metrics/empty states and validates five malformed query responses plus two header-only exports. [Rendered empty HTML](empty-dashboard.html), [app stdout](app-stdout.txt) and [stderr](app-stderr.txt) are retained. The HTTP-only app emits an expected missing-HTTPS-redirect-port warning. No browser layout, screen reader, TLS deployment or 2,000-result page rendering was exercised.

## Review findings addressed

| Finding from source review | Fix / evidence |
|---|---|
| The copied priority queue could reorder/evict equal-timestamp items arbitrarily | Stable timestamp plus snapshot-index priority; unsorted and tied cutoff tests for results and defects |
| Invalid nullable query filters could become null and silently export all data | Actionable HTTP 400 response; undefined enums rejected; unit and actual MVC HTTP checks |
| CSV assertions did not prove multiline logical rows/column counts | Independent framework CSV parser checks 15 result columns and 21 defect columns, escaping, missing context and all nine lifecycle states |
| Section labels were generic divs and metrics named a generic role | Real h2 section headings and named group; actual empty Razor output checked; assistive-technology acceptance remains manual |

The copied baseline compiled and passed its 30 tests. These findings came from review and new coverage; no baseline failing-test count is claimed. Changes stay in dashboard/reporting, related tests, the existing CI patch and QA evidence; teammate security, persistence, registration and defect-lifecycle implementation was not edited.

## Reproduce

From the repository root:

```powershell
dotnet --info
dotnet restore SecureApiQualityPlatform.sln
dotnet build SecureApiQualityPlatform.sln --configuration Release --no-restore
dotnet test SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj --configuration Release --no-build --no-restore --logger "trx;LogFileName=final.trx" --results-directory TestResults/recheck --collect "XPlat Code Coverage"
dotnet test SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj --configuration Release --no-build --no-restore --filter "TestCategory=Performance" --logger "trx;LogFileName=performance.trx" --results-directory TestResults/recheck-performance
```

The retained [http-smoke.ps1](http-smoke.ps1) runs on Windows after the Release build. It overwrites its corresponding HTTP evidence files when rerun; archive existing evidence first if preserving this run. The XML/TRX files are unchanged tool outputs copied out of ignored TestResults directories so they can be reviewed with the patch.

## Outstanding manual and report inputs

- Execute SQA-01/02/03/05/07/08: browser registration/safety/endpoint configuration, protected-endpoint negative workflow, actual unreachable/timeout workflow and assignment/priority/status controls. Partial automated guards are recorded separately in the register.
- Execute SQA-18: prepare 2,000 results in the app and measure browser rendering on agreed hardware/browser/build; record repeats, cache setting, screenshots and timing boundary.
- Execute SQA-19/20: representative tester interpretation of five failure messages, Chrome/Edge keyboard focus, headings, severity labels, narrow layouts and downloaded CSV inspection in the chosen spreadsheet application. Automated apostrophe-prefix checks do not establish universal spreadsheet safety.
- After approved commit/push, retain GitHub Actions URL, exact commit SHA, TRX/Cobertura artifact and SDK 8 result. The workflow upload is configured but not verified in GitHub here.
- Coordinate SQA-to-initial-TC/RTM mapping, unresolved defect severities (including Deferred), ownership, mitigations, requirement thresholds and release decision with Eilmar/Lana. Team documents have not been overwritten.
- Student-authored T3/T4/T7/T8, individual T9/T11 evidence, Appendix A cases and slides 11–15 remain student work. Use real execution records, Git contribution evidence and authentic AI interaction records; do not invent human decisions, prompts or screenshots. The separately provided brief/allocation was not present in this workspace review.

Verified residual limitations: singleton in-memory storage resets on restart; Program.cs configures no authentication/authorization/RBAC; selected passive checks do not establish production-grade security. Release readiness remains for the team to assess after the outstanding evidence is collected. No commit, push or merge was performed.
