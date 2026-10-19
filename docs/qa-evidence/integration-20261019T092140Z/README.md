# Main integration and validation evidence — 19 October 2026

Factual QA execution record, not student-authored assessment report prose or a release decision. No remote push or pull request is authorized by this integration.

## Source and safety record

| Item | Observed value |
|---|---|
| Working branch | `qa/sqa19-sqa20-manual-validation` |
| Original QA HEAD | `d551a2defd48c72f7b065b865415fd9aee3d4fb1`; clean and published before integration |
| Fetched main | `07dfd40` (full SHA in [integration metadata](integration.json)); fetch completed locally on 19 October |
| Initial divergence | QA-only 2 commits; main-only 51 commits |
| Safety backup | `codex/backup-sqa19-sqa20-20261019T092140Z-d551a2d`, verified pointing to original QA HEAD |
| Merge method | `git merge --no-ff --no-commit origin/main`; pause used to reconcile/validate before completing the user-authorized merge commit; no rebase/reset/force operation |
| Stopped servers | Verified repository SQA-19 PID 34240 and SQA-20 PID 12428; launcher wrappers 29652 and 27808 stopped first to avoid evidence-log writes |
| Original database snapshot | [Before](original-databases-before.json): no normal database files present at repository/Web roots |
| Environment | Windows 10.0.26200 x64; SDK 10.0.402; net8.0 application/runtime 8.0.32; [SDK output](dotnet-info.txt) |
| Tested source identity | Both parent SHAs plus [SHA-256 source manifest](source-sha256.csv); validation happened while the merge was pending, before its resulting commit SHA existed |

Existing QA commits and historical raw evidence were preserved. The unchanged human submission remains at [SQA-19](../SQA-19/observation-sheet-session-eilmar.md). Screenshot/network/trace archives are not relabelled as fresh executions.

## Conflict reconciliation

Only two paths conflicted:

| Path | Evidence-based resolution |
|---|---|
| [Final case register](../../ShiruiFinalTestCases.csv) | Main's exact SQA-18 record; original QA branch's exact SQA-19 and SQA-20 records. All other case records agreed between parents and were retained |
| [Quality metrics](../../ShiruiQualityMetrics-Evidence.md) | Combined the three outcomes, recalculated master totals, retained both histories and SQA-20 open findings, then updated automated counts/coverage from this actual merged-source run |

Reconciled outcomes:

- **SQA-18 PASS against the documented user-proposed operational criterion only.** Five 18 October visual-readiness upper bounds: 257.9, 199.5, 197.1, 189.7, 201.0 ms; median 199.5 ms, maximum 257.9 ms. Formal NFR-PERF-01 approval remains provisional. [Original execution](../2026-10-18/sqa18-visual-20261018T072617937Z/README.md), [decision](../2026-10-18/sqa18-visual-20261018T072617937Z/decision.json).
- **SQA-19 PASS (PROVISIONAL).** Reported Eilmar session completed; 5/5 interpretation/evidence pairs support proposed PASS. ChatGPT wording assistance and absent original unpolished responses remain disclosed; Shirui's final observer approval is pending. [Assessment](../SQA-19/observer-assessment-session-eilmar.md).
- **SQA-20 FAIL.** Historical 16/16 technical Chrome/Edge checks passed, but accessible-label and text-contrast failures remain open, with badge presentation also recorded. [Execution](../SQA-20/playwright-20261019T035608Z/README.md), [findings](../SQA-20/playwright-20261019T035608Z/findings.md). This integration does not fix or rerun those browser findings.

Totals: **22 planned; 16 executed; 14 register PASS; 1 provisional PASS; 1 FAIL; 0 BLOCKED; 6 NOT RUN**. Execution 16/22 = 72.73%; confirmed register PASS/all executed 14/16 = 87.50%; including the provisional recommendation 15/16 = 93.75%. SQA-18's register PASS still carries formal requirement-approval limitations. Five performance samples, five human scenarios and sixteen browser subchecks are not extra master cases. Other case outcomes remain unchanged; later teammate evidence is retained without silently approving new mappings into Shirui's register.

## Necessary integration corrections

- Main's normal startup now uses SQLite and initializes the database immediately. The older QA hosts assumed an in-memory store. A trusted `Program.CreateApp` service override now runs **before** database initialization, only in Development/Testing. Normal startup still uses SQLite; production overrides are rejected. All three separate fixture hosts select their own `InMemoryPlatformStore` explicitly. No seed route/configuration switch was added to the production interface.
- Four focused startup tests prove separate empty stores even with an unusable database path, unchanged sentinel data, normal SQLite initialization into an explicit temporary database, and rejection of production overrides. The existing main SQLite performance/integration tests remain intact.
- Main's demo RBAC still defaults to Viewer. SQA-20's future form-inspection runner uses an explicit real Admin demo-role cookie only in its isolated browser context; the HTTP verifier uses it only for registration inspection and checks the no-cookie redirect separately. Role enforcement is not bypassed. Read-only fixture hosts reject role-changing and other POST actions.
- SQA-19 HTML comparison ignores only the new role form's per-render antiforgery token. Its fixture messages/values remain compared. The PT-01 launcher now permits this QA branch as well as its original development branch.
- Current documentation acknowledges SQLite and user-selected demo RBAC, preserves the open DEF-010 transition limitation, and replaces obsolete current NOT RUN totals. Historical ledgers retain their original as-of statements and source identities.

## Actual verification

| Command/check | Actual outcome | Evidence |
|---|---|---|
| `dotnet restore SecureApiQualityPlatform.sln` | Succeeded | [Restore](restore.txt) |
| `dotnet build SecureApiQualityPlatform.sln` | Succeeded; 0 warnings, 0 errors | [Build](build.txt) |
| `dotnet test SecureApiQualityPlatform.sln --no-build --collect "XPlat Code Coverage" --logger "trx;LogFileName=merged-suite.trx" --results-directory "$PWD/TestResults/integration-20261019T092140Z"` | 136 passed, 0 failed, 0 skipped; runner reported 21 seconds | [Output](test.txt), [TRX](merged-suite.trx), [summary with exact run times](test-summary.json) |
| Actual application coverage | 876/1141 lines = 76.77%; 325/494 branches = 65.78% | [Cobertura](cobertura.xml) |
| SQA-19 / SQA-20 / PT-01 browser host Release builds | All succeeded with 0 warnings/errors | [SQA-19](Sqa19.ManualHost-build.txt), [SQA-20](Sqa20.ManualHost-build.txt), [PT-01](Pt01BrowserHost-build.txt) |
| Three real loopback fixture HTTP checks | All PASS; unusable database path never opened, sentinel unchanged, no normal database created; all smoke processes stopped | [Summary](host-smoke/summary.json), [reproducible smoke script](smoke-hosts.ps1), [SQA-19](host-smoke/sqa19-http.json), [SQA-20](host-smoke/sqa20/setup-verification.json), [PT-01](host-smoke/pt01/readiness.json) |
| Register schema, evidence references, source preservation and artifact review | Validation record is saved separately after the review | [Integrity checks](validation.json) |

The 136 tests include 132 executions in existing inherited test classes and four new startup-isolation executions. The previous 71- and 76-test results identify older source states. Instrumented aggregation/HTTP regressions do not constitute a new browser benchmark. Coverage is local and measured, not a CI claim; no new human session, screenshot, browser performance result or release approval is invented.

Smoke output is preparation evidence. The original SQA-19 answers and SQA-18/SQA-20 screenshot archives retain their own dates/provenance. All fixture servers used here were stopped after verification.

## Remaining decisions

Shirui's SQA-19 observer approval, formal NFR-PERF-01 approval, SQA-20 label/contrast remediation, DEF-010 workflow enforcement and the team's release decision remain pending. CI must run on the eventual pushed merge commit. The user must approve a normal push or PR separately. No evidence or status is changed merely to obtain a clean merge.
