# PT-01 execution ledger — 18 October 2026

Technical execution records and report inputs; student-authored assessment prose and the team's release decision remain pending.

**PT-01 PASS for aggregation and end-to-end HTTP document response. Browser rendering NOT RUN.**

| Check | Planned | Executed | Passed | Failed | Blocked | Not run |
|---|---:|---:|---:|---:|---:|---:|
| Release regression suite, without coverage | 71 | 71 | 71 | 0 | 0 | 0 |
| Dedicated PT-01 MSTest checks, without coverage | 2 | 2 | 2 | 0 | 0 | 0 |
| Browser layout/paint acceptance (SQA-18) | 1 | 0 | 0 | 0 | 0 | 1 |

The two PT-01 checks are also in the regression suite; these rows overlap. Timing samples are observations, not additional test cases. Restore and Release build succeeded, with 0 warnings and 0 errors. Runner exit code: 0. [Execution status and exact command arguments](execution-status.json), [TRX summary](trx-summary.json), [full-suite TRX](full-suite/full-suite.trx), [dedicated TRX](performance/performance.trx).

| Uninstrumented PT-01 measurement | First call/request (ms) | Warm median (ms) | Warm mean (ms) | Warm P95 (ms) | Warm maximum (ms) | Result |
|---|---:|---:|---:|---:|---:|---|
| Dashboard aggregation | 21.4494 | 0.7386 | 1.1824 | 0.9888 | 8.7894 | PASS |
| End-to-end HTTP document | 223.0098 | 1.8280 | 1.8832 | 2.4933 | 2.6382 | PASS |

Each check retains a first measurement, performs 3 further warm-ups, then retains 20 sequential samples. All first and retained sample durations must be strictly below **2,000 ms**, with correctness assertions passing. Warm-ups are excluded from the statistics. Median averages the two middle values; P95 uses nearest rank. Raw samples and full precision: [aggregation JSON](measurements/pt01-aggregation.json), [HTTP JSON](measurements/pt01-http.json), [console output](test-performance.txt).

## Timing boundaries and isolation

- Aggregation: `Stopwatch` surrounds the real `DashboardService.Build()`, including store snapshot copying/sorting and recent-item selection. Seeding, assertions and evidence generation are excluded. The first measurement is the first call on that service instance; cold-process/JIT state is not guaranteed.
- HTTP: a fresh application created by the real `Program.CreateApp` listens on an ephemeral `127.0.0.1` port. `HttpClient GET /` is timed from request initiation until the complete HTML body is buffered, including TCP, middleware, MVC, aggregation and Razor. Every retained response was HTTP 200 and 13,047 bytes. Redirects are disabled; first request opens a connection and later requests reuse it. Concurrency: 1. Host startup, seeding, decoding/assertions and file writes are excluded.
- Browser CSS fetches, layout and paint, deployment TLS, remote-network latency, process startup and concurrent-user load were not measured. The existing NFR-PERF-01 says browser rendering on an agreed machine and remains provisional; these results do not mark that broader requirement passed.
- Each check creates a private store and asserts it is empty before seeding; the HTTP check also creates a fresh application service provider. No existing application process is restarted, cleared or seeded. No manual results or persistence files are accessed. No outbound API checks run; `https://pt01.invalid` is only recorded fixture context. The host is stopped/disposed after the test.

Fixture: 2,000 fixed-ID results at a fixed UTC origin plus index seconds; 1,000 pass, 1,000 fail, 1,333 measured response values (`100 + index % 100` ms), 667 missing timings; average measured response 149.5 ms. One API and endpoint, plus 80 defects cycling all four severities and five lifecycle states. Closed/Deferred are excluded: 48 open, 12 per severity, 24 High/Critical. Assertions also verify 12 recent checks and 8 recent defects in both model and real HTML.

Both independently seeded fixtures have SHA-256 `CCBE6723FB603221C3CECB95BB176BD6426E8F7611B96D3AC0565A1D7927AC18`. The HTTP test checks the complete fixture hash before/after requests. [Saved actual dashboard HTML](measurements/pt01-dashboard.html) is response evidence, not a browser performance trace.

## Actual environment and source

| Item | Recorded value |
|---|---|
| Runner time | 18 October 2026, 18:38:34–18:38:49 NZDT (UTC+13) |
| Full suite / dedicated run | 18:38:43–18:38:45 / 18:38:47–18:38:49 NZDT, per TRX |
| OS / architecture | Microsoft Windows 10.0.26200 / x64 |
| CPU / total physical RAM | 12th Gen Intel Core i7-12700H; 20 logical processors / 16,834,023,424 bytes |
| SDK / MSBuild | .NET SDK 10.0.400 / MSBuild 18.9.6+14fbf8d52 |
| Measured test runtime / build | .NET 8.0.30 x64 / Release, net8.0 |
| Timer | High-resolution Stopwatch, frequency 10,000,000 ticks/second |
| Tools | MSTest 3.6.4; Microsoft.NET.Test.Sdk 17.11.1; coverlet.collector 6.0.2 installed; PowerShell 7.6.5; Git 2.45.1.windows.1 |
| Branch / HEAD | `feature/dashboard-reporting` / `1505cc9f1d60687111765aec1355ec1e9f87b2a9` |
| Tested source | HEAD plus uncommitted changes; [SHA-256 source manifest](tested-source-sha256.csv), [initial Git status](git-status.txt), [tracked diff](git-diff.txt) |
| Execution actor / CI | Codex automated local execution; no CI run triggered; human/team review pending |

[Environment JSON](environment.json), [SDK information](dotnet-info.txt), [package versions](test-package-versions.txt), [restore](restore.txt), [build](build-release.txt), [full-suite console](test-full-suite.txt). Only SDK 10 is installed locally; SDK 8 CI parity has not been verified. Workload/background machine load and power mode were not controlled or recorded; timing is specific to this local run.

## Execution history and coverage limitation

Previous attempts are retained unchanged, not combined into the final totals:

| Earlier run directory | Observed outcome |
|---|---|
| [18:32 run](../pt01-20261018T053209677Z-51a300/) | 71/71 full-suite checks passed with coverage; dedicated 2/2 passed. Dedicated HTTP JSON/HTML attachments were absent although timing stdout/TRX was retained. |
| [18:34 run](../pt01-20261018T053438680Z-f12700/) | 71/71 full-suite checks passed with coverage; dedicated 2/2 passed. New artifact guard correctly failed the runner because dedicated attachments were missing. |
| [18:37 run](../pt01-20261018T053714804Z-49fe7e/) | Explicit output directories retained both dedicated JSON records and HTML; dedicated 2/2 passed. Coverage run reported 36 discovered/executed failures and incomplete discovery because Windows Application Control blocked the instrumented Web assembly (`0x800711C7`); runner exit 1. [Failure log](../pt01-20261018T053714804Z-49fe7e/test-full-suite.txt). |

Final run uses `-SkipCoverage`: all 71 regression checks and both dedicated checks passed without instrumentation; final-run coverage is **NOT COLLECTED**. No security control was changed. Earlier coverage artifacts must retain their own run/source identity and must not be described as final-run coverage. Artifact handling was fixed by an explicit MSTest `PT01EvidenceDirectory` run parameter; the runner requires both PASS JSON records, matching fixture hashes and the HTML before reporting success. The expected missing-HTTPS-port warning comes from the HTTP-only loopback fixture; retained responses are verified HTTP 200.

## Reproduce and evidence to save

From the repository root:

```powershell
pwsh -NoProfile -File docs/qa-evidence/Invoke-PT01.ps1 -SkipCoverage
```

This creates a new timestamped directory and retains the original records. Omitting `-SkipCoverage` attempts the full-suite coverage collector, which may encounter the documented local Application Control failure. Dedicated performance checks always run without coverage instrumentation.

Save these original logs/files with the report evidence:

- `test-performance.txt` and `performance/performance.trx`: two passing checks, raw sample arrays and timing statistics. A screenshot of these PT-01 output lines plus the two-test success summary is useful.
- `measurements/pt01-aggregation.json`, `measurements/pt01-http.json`: exact values, UTC execution times, boundary definitions, runtime and fixture identity. A screenshot showing first/median/P95/max and the 2,000 ms threshold is useful; keep the JSON as primary evidence.
- `measurements/pt01-dashboard.html`: actual HTTP 200 fixture response. A screenshot of its 2,000 executed checks, 1,000/1,000 split, 48 open defects and severity counts can illustrate the fixture; label a saved-HTML view as captured response evidence and do not use its load time as live dashboard timing.
- `environment.json`, `dotnet-info.txt`, `tested-source-sha256.csv`, `execution-status.json`, `full-suite/full-suite.trx`, `test-full-suite.txt` and `build-release.txt`: machine, source, commands and regression evidence. Retain the failed coverage log separately as an environment issue.

Outstanding manual/report inputs:

- [ ] SQA-18: on agreed hardware/browser, measure actual navigation/CSS/layout/paint with 2,000 results in a separate isolated fixture. Save a DevTools Network timing screenshot plus a Performance trace, browser version, cache setting, repeated durations and a screenshot of populated metrics. The test host here has already been stopped; the saved URL is not a persistent preview.
- [ ] Human inspection of dashboard labels, keyboard navigation and narrow layouts; the remaining SQA cases retain their existing NOT RUN status.
- [ ] Team confirmation of the provisional performance criterion, SDK 8 CI run/commit URL after approved commit/push, and human release decision.
- [ ] Student-authored T3/T4/T7/T8, individual T9/T11, Appendix A and slides 11–15 can cite these factual records; do not turn unexecuted browser checks into passes.

No commit, push, merge or changes to teammate-owned security/persistence/defect-lifecycle functionality were performed.
