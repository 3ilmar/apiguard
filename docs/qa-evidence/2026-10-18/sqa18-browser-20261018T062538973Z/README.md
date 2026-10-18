# SQA-18 isolated browser environment — 18 October 2026

Latest local SQA-18 result: **PASS against the user's proposed operational criterion**, not formally approved by the team; see [five-reload visual execution](../sqa18-visual-20261018T072617937Z/README.md). Earlier status/results below remain historical snapshots; no raw traces, measurements or launch records were replaced.

Browser evidence update: **EXECUTED — ACCEPTANCE PENDING**. The user's five Network runs and separate Performance trace are now recorded in the [browser measurement ledger](browser-measurements/README.md). Network Finish median/max are **55/190 ms**; trace FCP and one LCP candidate are **89.415 ms**. Complete-dashboard visual time and the team's 2-second acceptance decision remain unresolved. These files are factual setup/measurement evidence, not submitted report prose. Original launch/readiness JSON files retain their **NOT RUN** values as snapshots taken before the user collected browser evidence.

## Live instance

- URL: http://127.0.0.1:51818/
- PID: `35680`; started at `2026-10-18T06:30:15.8398576Z` (19:30:15 NZDT). Left running for the user's measurements.
- Verified HTTP 200 and process alive at `2026-10-18T06:31:31.5490793Z` (19:31:31 NZDT); see `handover-verification.json`.
- Branch: `feature/dashboard-reporting`; HEAD: `ced44546ba9c71f25935e6ba913b27fe08f0acd1`, with uncommitted changes. Use `live-instance/source-sha256.csv` with the SHA to identify the source.
- Separate executable: `tools/Pt01BrowserHost`; actual Web `Program.CreateApp` / MVC / Razor / CSS, Development environment, Release build, .NET 8.0.30 x64 targeting net8.0. SDK: 10.0.400.
- Hardware observed: Windows 10.0.26200, Intel Core i7-12700H, 20 logical processors, 16,834,023,424 bytes physical RAM. See `dotnet-info.txt` and `handover-verification.json`.

## Fixture and isolation

- Exactly 2,000 deterministic recorded results, 1,000 passed and 1,000 failed, one API/endpoint, 80 defects, 48 open, 24 open high/critical and 12 open per severity.
- Fixture SHA-256: `CCBE6723FB603221C3CECB95BB176BD6426E8F7611B96D3AC0565A1D7927AC18`, matching the existing PT-01 fixture. Fixed timestamps are test data, not execution timestamps.
- Shared fixture source is compiled into Tests and the separate host only. No production Web source, configuration, seed endpoint or startup switch was added. The helper is not in the production solution and is marked non-publishable.
- The helper creates a fresh process/service provider/in-memory store. Seeding rejects any populated store before writes. Existing manual app processes/stores are not seeded, cleared or restarted. No outbound checks execute.
- Loopback-only URL; GET/HEAD only in the helper, with POST returning 405 to keep the synthetic fixture fixed. These restrictions do not change the normal Web app.
- Dashboard aggregates 2,000 records but displays the latest 12 results and eight defects. Readiness verified the actual CSV record counts, HTML values, HTTP 200 without redirects, local CSS 200 and unchanged exports after requests. See `live-instance/readiness.json` and `dashboard-http.html`.
- Production-environment invocation was rejected before app creation/seeding; see `host-safety.json` and `production-rejection.txt`.
- Existing HTTPS middleware logs a missing HTTPS-port warning for this HTTP-only local fixture. Readiness confirms HTTP 200, not a redirect.
- Bootstrap still loads from the app's normal jsDelivr URL. Its success and browser loading cost must be recorded by the tester; the HTTP smoke does not fetch it.

## Executed checks

| Check | Actual result | Evidence |
|---|---|---|
| .NET info and solution restore | Exit 0 | `dotnet-info.txt`, `restore.txt` |
| Solution Release build outside sandbox | Exit 0; zero warnings/errors | `build-release-unrestricted.txt` |
| Full MSTest suite with XPlat Code Coverage | 76 executed; 76 passed; zero failed/skipped | `test-coverage.txt`, `coverage-run/full-suite-coverage.trx` |
| Local coverage | 463/635 lines (72.91%); 221/390 branches (56.67%) | `cobertura.xml` |
| Host Release build | Exit 0; zero warnings/errors | `live-instance/host-build.txt` |
| Live HTTP/CSS/export/read-only smoke | PASS for setup only | `live-instance/readiness.json` |
| SQA-18 browser observations | Five reported Network runs plus one trace; ACCEPTANCE PENDING | [Browser ledger](browser-measurements/README.md), [observations](browser-observations.csv); complete visual boundary not timed |

The new five fixture checks are included in the 76 tests, together with the existing aggregation/HTTP PT-01 checks. The latter ran with coverage here; their timings do not establish uninstrumented browser performance. This run creates no CI URL and makes no release decision. Original coverage attachments are duplicated by the test adapter with identical hashes; `cobertura.xml` is a reviewable copy.

Sandbox limitations: `dotnet --info` initially hit Service Control Manager access denial; the focused fixture testhost stalled and was interrupted without test results. The sandbox solution build returned exit 1 with zero diagnostic errors (`build-release.txt`). The completed build/test records above come from the unrestricted retry. A sandbox socket check was denied; the unrestricted handover check succeeded. No security control was changed.

## Browser evidence received and remaining inputs

The user's raw trace and all six screenshot attachments are preserved with verified hashes under [browser-measurements](browser-measurements/README.md). `browser-observations.csv` now records the five user-reported Network samples; per-run timestamps and complete visual durations remain empty. Three attachments are byte-identical and the single trace is not paired to a Network run. The screenshots show cache disabled and no network throttling; full Chrome version, tester, exact zoom, power/extension state and CPU throttle setting remain missing. Readiness warmed MVC/Razor before collection; this is a warmed-server scenario.

The [Chrome collection checklist](../../SQA18-Browser-Performance.md) proposes navigation start to the first fully styled/populated dashboard frame. That complete visual boundary remains unmeasured. HTTP, Network Finish, load, FCP or an LCP candidate alone do not prove complete-dashboard rendering. SQA-18 has partial executed evidence and status **ACCEPTANCE PENDING**; the team must confirm the provisional below-2,000-ms requirement's metric/rule before deciding acceptance. Other test-case statuses remain unchanged.

Relaunch if needed from the repository root:

```powershell
pwsh -File tools/Pt01BrowserHost/Start-PT01Browser.ps1
```

This uses a new evidence directory and fails if the requested port is occupied; it never replaces an existing server. Only after evidence collection, the user may stop this synthetic instance with `Stop-Process -Id 35680` after confirming that PID still belongs to this host. No process has been stopped as part of handover.
