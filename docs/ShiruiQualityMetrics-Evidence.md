# Quality metric calculations and release evidence worksheet

> Supporting QA calculations only. The final submitted assessment report must be authored by the students.

## Metric definitions

| Metric | Calculation / source | Current measured value |
|---|---|---|
| Test execution rate | Executed / Planned × 100; case register | 14/22 = 63.64%; 14 PASS; 8 not run; 0 blocked |
| Test pass rate | Passed / Executed × 100; case register | 14/14 = 100%; 0 failed; 0 case-level acceptance pending; SQA-18 PASS is against the user's proposed operational criterion, not formal team approval |
| Automated test pass rate | Passed MSTest / executed MSTest × 100; TRX artifact | Latest recorded run at 19:29 NZDT: 76/76 = 100%; 0 failed/skipped; Release with coverage; not rerun during browser evidence import |
| Requirements verification | Fully verified requirements / requirements in scope × 100; RTM | PENDING |
| Defect severity distribution | Count unresolved defects by severity; include Deferred separately from dashboard open counts | NOT ASSESSED; test fixtures are not the team's product defect register |
| Automation coverage | Test cases automated / cases selected for automation × 100; test mapping | Selection denominator requires team agreement; 13 SQA cases have complete MSTest evidence; SQA-18 adds a separate automated browser run with screenshot review |
| Code coverage | Local Cobertura instrumented lines/branches in SecureApiQualityPlatform.Web | Latest recorded 19:29 setup run: 463/635 lines = 72.91%; 221/390 branches = 56.67%; source hashes retained; CI pending. Earlier blocked and successful runs retain their own source identities |
| Commit activity | Count verifiable commits per member from Git history | PENDING |
| Dashboard PT-01 aggregation/HTTP | Existing uninstrumented 18:38 run with 2,000 deterministic results; distinct from browser | PASS for these boundaries: aggregation first 21.4494 ms / warm median 0.7386 ms / max 8.7894 ms; HTTP first 223.0098 ms / warm median 1.8280 ms / max 2.6382 ms |
| SQA-18 earlier browser Network | Five user-reported Network Finish values; screenshots retained; displayed rounded timings | 190, 55, 55, 55, 41 ms; median 55 ms; maximum 190 ms; DOMContentLoaded 15 ms each. Historical supporting evidence; Network Finish is not visual-render time; duplicate attachments disclosed |
| SQA-18 earlier browser paint | One separate matched-navigation Performance trace at 19:50:43 NZDT | FCP 89.415 ms; one LCP candidate 89.415 ms; historical supporting evidence, not finalized LCP or complete rendering; no trace-to-Network-run pairing established |
| SQA-18 visual readiness | Five independent reloads; navigation start to conservative screenshot-verified readiness upper bound, including stylesheet loading, layout and paint readiness | 257.9, 199.5, 197.1, 189.7, 201.0 ms; median **199.5 ms**; maximum **257.9 ms**; every value below 2,000 ms; dashboard values correct; fixture unchanged before/after |
| SQA-18 proposed operational acceptance | Exactly 2,000 deterministic results; correct metrics/severity; five consistent-condition navigation-to-visual-readiness measurements; all below 2,000 ms | **PASS against the user's proposed criterion**. Headed Chrome 155.0.8059.80, Playwright 1.62.1, viewport 1440 × 1600; screenshots and environment/settings retained. Criterion is not formally approved by the team; formal NFR-PERF-01 approval and release decision remain pending |

Latest browser evidence: [five visual-readiness runs, screenshots and environment](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/README.md), [user-proposed operational criterion](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/PROPOSED-CRITERION.md). Earlier evidence: [user Network observations, trace extraction and limitations](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/browser-measurements/README.md), [isolated host/setup ledger](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/README.md), [76-test TRX](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/coverage-run/full-suite-coverage.trx), [latest Cobertura](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/cobertura.xml), [master register](ShiruiFinalTestCases.csv). The earlier uninstrumented [PT-01 execution ledger](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md) retains aggregation/HTTP timing evidence. Historical [earlier TRX](qa-evidence/2026-10-18/final.trx), [earlier coverage](qa-evidence/2026-10-18/cobertura.xml) and [environment/source identity](qa-evidence/2026-10-18/README.md) remain unchanged. Local setup used SDK 10.0.400 targeting net8.0 on .NET 8.0.30; workflow SDK 8 CI verification remains pending. The earlier failed coverage attempt is not counted as a passing suite.

Register totals: **22 planned; 14 executed; 14 passed; 0 failed; 0 case-level acceptance pending; 0 blocked; 8 not run**. Only SQA-18's status changed. Repeated measurements remain one test case. SQA-18 PASS records the user's proposed operational criterion; it does not approve the formal requirement or release. Missing metadata in the earlier user-observation ledger remains historical and is not filled from the new automated run.

## Release gate checklist

- [ ] Build passes on the submitted commit.
- [x] Recorded local uncommitted application/test source builds; 76/76 automated tests passed with coverage at 19:29 NZDT. Browser evidence updates do not constitute another test-suite run.
- [ ] CI build/tests/coverage pass on the submitted commit; run URL and artifact retained.
- [ ] Manual acceptance and usability testing completed; evidence is linked.
- [ ] No unresolved Critical defect affecting core workflows; High issues have approved mitigations.
- [ ] Requirement coverage and defect traceability reviewed with Eilmar and Lana.
- [x] SQA-18 meets the user's proposed operational browser criterion: five visual-readiness upper bounds below 2,000 ms with correct 2,000-result dashboard values; new evidence retained.
- [ ] Team/stakeholders formally approve the NFR-PERF-01 target and operational acceptance rule. The local SQA-18 PASS does not supply this approval.
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
