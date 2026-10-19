# SQA-20 automated environment preparation evidence

**Human SQA-20 status: NOT RUN.** This directory contains local setup outputs, not manual browser observations. The fixture's passing/failing API check badges and the automated suite results do not assign a manual SQA-20 outcome. No Chrome/Edge screenshots or browser downloads were collected.

Prepared on **18 October 2026, 22:23–22:26 NZDT (Pacific/Auckland, UTC+13)**. Exact timestamps are in JSON/TRX records. Branch: `qa/sqa19-sqa20-manual-validation`, following the user's current branch instruction. Base commit: `8e69c6273080b01db100c29cc76eb33a798e84af`. SQA-19 tools/evidence were already untracked when this task began. SQA-20 preparation is additional uncommitted work; the base commit alone does not identify these files. No commit or push was performed.

| Actual preparation activity | Observed result | Evidence |
|---|---|---|
| SDK/environment inspection | SDK 10.0.402; Windows 10.0.26200 x64; running host loaded .NET 8.0.32 | [dotnet-info.txt](dotnet-info.txt), [environment.json](environment.json), [loaded-runtime.json](loaded-runtime.json) |
| Solution restore | Succeeded; existing projects up to date | [dotnet-restore.txt](dotnet-restore.txt) |
| Solution build, Debug | Succeeded; 0 warnings, 0 errors | [dotnet-build.txt](dotnet-build.txt) |
| Separate host build, Release | Succeeded; 0 warnings, 0 errors | [host-build.txt](host-build.txt) |
| Existing MSTest suite, Debug, XPlat coverage | 71 passed; 0 failed; 0 skipped | [dotnet-test.txt](dotnet-test.txt), [test-summary.json](test-summary.json), [automated-suite.trx](automated-suite.trx), [cobertura.xml](cobertura.xml) |
| Start real MVC app with isolated fixtures | Development host at http://localhost:51920/ | [host-launch.json](host-launch.json), [host-stdout.txt](host-stdout.txt), [host-stderr.txt](host-stderr.txt) |
| HTTP route checks | Dashboard, APIs, registration form, both API details, Defects and original CSS returned HTTP 200 | [setup-verification.json](setup-verification.json) |
| Synthetic fixture/recency checks | 2 APIs, 2 endpoints, 8 checks (4 passing/4 failing), 4 open defects; one each Critical/High/Medium/Low; timestamps within 24 hours | [setup-verification.json](setup-verification.json), [automated-results.csv](automated-results.csv), [automated-defects.csv](automated-defects.csv) |
| Original CSV endpoints | UTF-8 CSV attachment headers; 15 result columns/8 rows; 21 defect columns/4 rows; quoted commas/quotes preserved; filtered exports verified | [setup-verification.json](setup-verification.json), CSV samples above |
| Isolation controls | Bound only to 127.0.0.1 and ::1; write/scan attempts rejected with 405; export bodies unchanged afterward | [listeners.json](listeners.json), [setup-verification.json](setup-verification.json) |
| Installed browser executable metadata | Chrome 155.0.8059.80; Edge 155.0.4283.62; browser execution NOT RUN | [environment.json](environment.json) |
| Blank manual evidence template | 16 NOT RUN rows, 8 per browser; all actual observation fields blank; master SQA-20 stays NOT RUN | [manual-template-check.json](manual-template-check.json) |

Commands executed from the repository root:

```powershell
dotnet --info
dotnet restore SecureApiQualityPlatform.sln
dotnet build SecureApiQualityPlatform.sln --no-restore
dotnet build tools/Sqa20.ManualHost/Sqa20.ManualHost.csproj --configuration Release --nologo
dotnet test SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj --no-build --collect "XPlat Code Coverage" --logger "trx;LogFileName=sqa20-preparation.trx" --results-directory "$PWD/TestResults/sqa20-preparation"
powershell.exe -NoProfile -File tools/Sqa20.ManualHost/Start-Sqa20.ps1
powershell.exe -NoProfile -File tools/Sqa20.ManualHost/Verify-Sqa20.ps1 -OutputDirectory docs/qa-evidence/SQA-20/automated-setup
```

The launcher was started as a hidden background PowerShell process so the application remains available for the user's session. The start script rebuilds the separate host, then runs its DLL with the original Web content root. Stop it with `tools/Sqa20.ManualHost/Stop-Sqa20.ps1`; that script checks ownership before stopping the listener. Process IDs identify this launch only. The stdout/stderr files are live logs and may grow.

The SQA-20 host follows the existing SQA-19 host pattern and calls the unchanged application `Program.CreateApp`. It checks for an empty `InMemoryPlatformStore` before seeding a separate service provider. The normal application's source, settings, original data and existing SQA-19 files were left intact. The new tool is not added to the solution or normal startup path. Only the isolated host rejects non-GET/HEAD requests; original forms remain visible for keyboard/label inspection but writes and API execution are unavailable in this session.

Results come from the existing `CheckEvaluator`, and the four defects from `DefectLifecycleService`. Fixture response times and status/header values are synthetic inputs, not live measurements or evidence of vulnerabilities in any target. Timestamps record fixture creation near startup. API base URLs use `.invalid`; no real external API scan was performed. The original layout still requests Bootstrap CSS from jsDelivr when the user opens a browser.

The first sandboxed `dotnet --info` attempt could not inspect Windows services. The subsequent execution with normal Windows access succeeded, as did restore/build/test/start/verification. Test reports and coverage were copied from generated results without alteration. Coverage is named `cobertura.xml` because `coverage*.xml` is ignored by the repository. CI: N/A, local preparation. No release or accessibility decision is inferred.

[source-sha256.csv](source-sha256.csv) identifies the application, test and uncommitted SQA-20 tool sources. [working-tree-status.txt](working-tree-status.txt) records the local file state. Installed executable versions are preparation metadata; the user must record actual browser versions and execution times during the manual session. Store actual browser downloads under `../downloads/`, screenshots under `../screenshots/`, and observations under `../observations/`; the automated CSV samples here are not browser download evidence.
