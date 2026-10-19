# Quality metric calculations and release evidence worksheet

> Supporting QA calculations only. The final submitted assessment report must be authored by the students.

## Metric definitions

| Metric | Calculation / source | Current measured value |
|---|---|---|
| Test execution rate | Executed / Planned × 100; master SQA case register | 14/22 = 63.64%; 13 PASS, 1 FAIL (SQA-20), 8 NOT RUN, 0 BLOCKED |
| Test pass rate | Passed / Executed × 100; exclude blocked and not run | 13/14 = 92.86%; SQA-20 failed additional accessibility criteria |
| Automated test pass rate | Passed MSTest / executed MSTest × 100; TRX artifact | 19 October 2026: 71/71 = 100%; 0 failed/skipped; Debug with XPlat coverage; SDK 10.0.402 targeting net8.0 on installed runtime 8.0.32 |
| Requirements verification | Fully verified requirements / requirements in scope × 100; RTM | PENDING |
| Defect severity distribution | Count unresolved defects by severity; include Deferred separately from dashboard open counts | NOT ASSESSED; test fixtures are not the team's product defect register |
| Automation coverage | Test cases automated / cases selected for automation × 100; test mapping | Selection denominator requires team agreement; 14 SQA cases now have executed automated evidence (13 PASS, SQA-20 FAIL); human SQA-19 acceptance remains NOT RUN |
| Code coverage | Local Cobertura instrumented lines/branches in SecureApiQualityPlatform.Web | 19 October Debug run: 561/781 lines = 71.83%; 222/392 branches = 56.63%; current local source identity recorded. Earlier coverage-loading failure and older coverage remain historical; CI pending |
| Commit activity | Count verifiable commits per member from Git history | PENDING |
| Dashboard PT-01 | Aggregation and complete HTTP document receipt for 2,000 deterministic results; browser separately | PASS for both measured boundaries: aggregation first 21.4494 ms / warm median 0.7386 ms / max 8.7894 ms; HTTP first 223.0098 ms / warm median 1.8280 ms / max 2.6382 ms; browser NOT MEASURED |
| SQA-20 automated browser checks | Eight scoped technical categories in each installed browser, plus separate accessibility scans | 16/16 technical PASS, 0 FAIL/BLOCKED/NOT RUN; Chrome 155.0.8059.80 and Edge 155.0.4283.62. Additional axe/DOM naming and contrast checks FAIL, therefore overall SQA-20 FAIL. No human acceptance inferred |

Latest SQA-20 evidence: [execution ledger](qa-evidence/SQA-20/playwright-20261019T035608Z/README.md), [all 16 results](qa-evidence/SQA-20/playwright-20261019T035608Z/browser-execution.json), [accessibility findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md), [71-test TRX](qa-evidence/SQA-20/playwright-20261019T035608Z/dotnet/automated-suite.trx), [coverage](qa-evidence/SQA-20/playwright-20261019T035608Z/dotnet/cobertura.xml), [master register](ShiruiFinalTestCases.csv). This run used SDK 10.0.402 on Windows 10.0.26200 x64; the application host loads .NET 8.0.32. No production source changes were made for browser execution.

Retained PT-01 evidence: [execution ledger](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md), [dedicated PT-01 TRX](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/performance/performance.trx). Historical evidence: [earlier TRX](qa-evidence/2026-10-18/final.trx), [earlier Cobertura](qa-evidence/2026-10-18/cobertura.xml), [earlier environment/source identity](qa-evidence/2026-10-18/README.md). The earlier execution ledger records SDK 10.0.400/.NET 8.0.30 and a separate coverage-loading failure; those are not the latest run. CI on the submitted commit is still unverified.

## Release gate checklist

- [ ] Build passes on the submitted commit.
- [x] Latest local working tree builds with 0 warnings/errors; all 71 automated tests pass with coverage. The earlier coverage-loading failure remains documented historically.
- [ ] CI build/tests/coverage pass on the submitted commit; run URL and artifact retained.
- [ ] Manual acceptance and usability testing completed; evidence is linked.
- [ ] No unresolved Critical defect affecting core workflows; High issues have approved mitigations.
- [ ] Requirement coverage and defect traceability reviewed with Eilmar and Lana.
- [ ] Performance browser-render measurement is recorded for NFR-PERF-01 or the target is clearly unverified.
- [x] Automated CSV parsing, lifecycle, filters, malformed input, and apostrophe-prefix formula mitigation pass locally; spreadsheet application inspection remains manual.
- [ ] Known issues and ownership/mitigations are agreed by the team.

## Release decision (fill only after verification)

Decision: **PENDING / NOT ASSESSED BY THE TEAM**

Basis: [actual test run URL, failing tests, severe defects, accepted risk, unverified requirements]

Observed limitations for team assessment: in-memory store loses data at restart; no authentication/RBAC is configured; the selected passive checks do not establish production security. Dashboard open counts exclude both Closed and Deferred; Deferred can still carry release risk. Numeric performance/compatibility targets remain provisional in Requirements.md. No AI-generated final report prose or human release decision is supplied here.

Residual risk and mitigation table:

| Risk / defect ID | Severity | User/system impact | Mitigation / responsible person | Due date | Evidence |
|---|---|---|---|---|---|
| SQA20-A11Y-01 | High (QA triage recommendation) | Endpoint and defect controls lack reliable programmatic label associations; axe fails 22 rendered control instances per browser, DOM inspection identifies 26 association gaps | Associate visible labels and unique control IDs; team owner/acceptance pending | PENDING | [Observed findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md) |
| SQA20-A11Y-02 | Medium (QA triage recommendation) | API BaseUrl text contrast 4.23:1 and Defects export-link contrast 4.4:1 fall below the tested 4.5:1 criterion | Adjust colours and remeasure; team owner/acceptance pending | PENDING | [Observed findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md) |
| SQA20-UI-01 | Low (QA triage recommendation) | Defect severity backgrounds stretch vertically; reproduced at narrow width, with labels still readable | Align badge to start of flex row; team owner/acceptance pending | PENDING | [Observed findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md) |

Do not infer measured line coverage from a suite's existence, or mark `pass` when the test was blocked or never executed.
