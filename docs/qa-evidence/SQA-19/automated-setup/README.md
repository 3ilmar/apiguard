# Automated SQA-19 preparation evidence

**Human SQA-19 / TC-12 status: NOT RUN.** No representative tester answers or assessments have been collected. The PASS values in HTTP checks describe environment preparation only. These are factual tool outputs and setup notes, not final assessment report prose.

Prepared on **18 October 2026, approximately 21:51–21:54 NZDT (Pacific/Auckland, UTC+13)**. Exact UTC timestamps are retained in JSON/TRX records. Branch: `qa/sqa19-sqa20-manual-validation`; base commit: `8e69c6273080b01db100c29cc76eb33a798e84af`. The initial working tree was clean. Host/scripts/templates are additional uncommitted files and are not part of that base commit. No commit or push was performed.

| Actual preparation activity | Observed result | Evidence |
|---|---|---|
| SDK/environment inspection | SDK 10.0.402; Windows 10.0.26200 x64; host loaded .NET 8.0.32 | [environment.json](environment.json), [dotnet-info.txt](dotnet-info.txt) |
| Restore solution | Succeeded; existing projects up to date | [dotnet-restore.txt](dotnet-restore.txt) |
| Build solution, Debug | Succeeded, 0 warnings / 0 errors | [dotnet-build.txt](dotnet-build.txt) |
| Build/start separate host, Release | Succeeded, 0 build warnings / 0 errors; Development on localhost:51919 | [host-stdout.txt](host-stdout.txt), [host-stderr.txt](host-stderr.txt), [host-launch.json](host-launch.json) |
| Existing MSTest suite, Debug, XPlat coverage | 71 passed, 0 failed, 0 skipped; runner reported 6 seconds | [dotnet-test.txt](dotnet-test.txt), [automated-suite.trx](automated-suite.trx), [cobertura.xml](cobertura.xml) |
| Original MVC dashboard and five ordered failure messages | HTTP 200; five exact evaluator messages and five FAIL badges | [http-verification.json](http-verification.json) |
| Host isolation controls | Listeners only 127.0.0.1 and ::1; POSTs rejected with 405, other controller paths rejected with 404; dashboard HTML unchanged afterwards | [listeners.json](listeners.json), [http-verification.json](http-verification.json) |
| Browser preparation check | Codex in-app browser rendered all five messages in Recent checks | [dashboard-preparation.jpg](dashboard-preparation.jpg) |
| Observation CSV structure | Five NOT RUN rows, all PASS/FAIL and answer fields blank | [../observation-sheet.csv](../observation-sheet.csv) |

Commands executed from the repository root:

```powershell
dotnet --info
dotnet restore SecureApiQualityPlatform.sln
dotnet build SecureApiQualityPlatform.sln --no-restore
dotnet build tools/Sqa19.ManualHost/Sqa19.ManualHost.csproj --configuration Release --nologo
dotnet test SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj --no-build --collect "XPlat Code Coverage" --logger "trx;LogFileName=sqa19-preparation.trx" --results-directory "$PWD/TestResults/sqa19-preparation"
powershell -NoProfile -File .\tools\Sqa19.ManualHost\Start-Sqa19.ps1
.\tools\Sqa19.ManualHost\Verify-Sqa19.ps1
```

The launcher was started in a hidden background PowerShell process for the requested ready-to-use instance. It builds the tool, then runs the separate DLL with the original Web content root. Current process IDs are preparation metadata; verify ownership before stopping anything. The stop script performs that check. `host-stdout.txt` / `host-stderr.txt` are live process logs and can grow while this instance remains running.

The normal application sources, routes and settings were not edited. `Program.CreateApp` creates a new service provider and a fresh `InMemoryPlatformStore` for the separate executable. Only this store is seeded. The host permits dashboard/CSS reads and blocks MVC write/run actions; it never invokes `ApiCheckService`. No external API scanning was performed. The original shared layout fetches its Bootstrap stylesheet from jsDelivr.

All messages come from the existing `CheckEvaluator`. The availability reason `Request timed out.` is the same literal used by the existing `ApiCheckService` timeout handler. Five results are seeded directly; defect processing is deliberately outside this message-review fixture. Result timestamps are startup-relative synthetic ordering values, and the supplied 1001 ms is fixture data. These values are not measurements of live endpoint requests.

Initial sandboxed .NET attempts encountered SDK profile/workload access restrictions. Normal Windows-access executions then completed successfully; no unresolved build or test blocker remains. The test report and coverage file were copied byte-for-byte from the generated test results; coverage was retained as `cobertura.xml` because the repository ignores `coverage*.xml`. No coverage threshold or release decision is inferred here. CI reference: N/A, local execution.

The browser screenshot is **setup evidence only**. It does not establish representative-user understanding, Chrome/Edge compatibility, accessibility, or SQA-20 completion. During the human session, record the participant's actual browser/version, viewport/zoom, date/time and raw answers in a copy of the observation sheet. Keep the facilitator key out of the participant's view. Existing SQA-19/SQA-20 test-case statuses remain unchanged.

Source hashes for the application and preparation code are in [source-sha256.csv](source-sha256.csv). They identify the uncommitted tool alongside the base application, independently of the commit label.
