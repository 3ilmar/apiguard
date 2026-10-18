# Quality metric calculations and release evidence worksheet

> Supporting QA calculations only. The final submitted assessment report must be authored by the students.

## Metric definitions

| Metric | Calculation / source | Current measured value |
|---|---|---|
| Test execution rate | Executed / Planned × 100; master SQA case register | 13/22 = 59.09%; 9 not run; 0 blocked |
| Test pass rate | Passed / Executed × 100; exclude blocked and not run | 13/13 = 100%; remaining cases unverified |
| Automated test pass rate | Passed MSTest / executed MSTest × 100; TRX artifact | Latest final run at 18:38 NZDT: 71/71 = 100%; 0 failed/skipped; Release without coverage; dedicated overlapping PT-01 checks 2/2 passed |
| Requirements verification | Fully verified requirements / requirements in scope × 100; RTM | PENDING |
| Defect severity distribution | Count unresolved defects by severity; include Deferred separately from dashboard open counts | NOT ASSESSED; test fixtures are not the team's product defect register |
| Automation coverage | Test cases automated / cases selected for automation × 100; test mapping | Selection denominator requires team agreement; 13 SQA cases have complete automated evidence in the local register |
| Code coverage | Local Cobertura instrumented lines/branches in SecureApiQualityPlatform.Web | Final PT-01 source run: NOT COLLECTED; an instrumented attempt was blocked by Windows Application Control. Earlier 17:56 run: 402/629 lines = 63.91%; 183/390 branches = 46.92%; historical source only; CI pending |
| Commit activity | Count verifiable commits per member from Git history | PENDING |
| Dashboard PT-01 | Aggregation and complete HTTP document receipt for 2,000 deterministic results; browser separately | PASS for both measured boundaries: aggregation first 21.4494 ms / warm median 0.7386 ms / max 8.7894 ms; HTTP first 223.0098 ms / warm median 1.8280 ms / max 2.6382 ms; browser NOT MEASURED |

Latest evidence: [PT-01 execution ledger](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md), [71-test TRX](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/full-suite/full-suite.trx), [dedicated PT-01 TRX](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/performance/performance.trx), [master register](ShiruiFinalTestCases.csv). Historical evidence: [earlier TRX](qa-evidence/2026-10-18/final.trx), [earlier Cobertura](qa-evidence/2026-10-18/cobertura.xml), [earlier environment/source identity](qa-evidence/2026-10-18/README.md). Local execution on 18 October 2026 used SDK 10.0.400 targeting net8.0 on .NET 8.0.30; the workflow's SDK 8 build still requires a CI run. The failed coverage attempt is retained separately and is not counted as a passing suite.

## Release gate checklist

- [ ] Build passes on the submitted commit.
- [x] Final local uncommitted working tree builds; all 71 automated tests pass without coverage. The separate coverage-loading failure is documented in the execution ledger.
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
| | | | | | |

Do not infer measured line coverage from a suite's existence, or mark `pass` when the test was blocked or never executed.
