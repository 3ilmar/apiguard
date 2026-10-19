# Quality metric calculations and release evidence worksheet

> Supporting QA calculations only. The final submitted assessment report must be authored by the students.

## Metric definitions

| Metric | Calculation / source | Current measured value |
|---|---|---|
| Test execution rate | Executed / Planned × 100; includes executed cases awaiting final approval | 16/22 = 72.73%; 14 PASS (including SQA-18 against the proposed criterion), 1 PASS (PROVISIONAL, SQA-19), 1 FAIL (SQA-20), 6 NOT RUN, 0 BLOCKED |
| Test pass rate | Passed / all executed × 100; exclude blocked and not run; distinguish provisional outcomes | Confirmed register PASS only: 14/16 = 87.50%. Proposed including SQA-19 provisional PASS: 15/16 = 93.75%, subject to Shirui's approval. SQA-20 remains FAIL |
| Automated test pass rate | Passed MSTest / executed MSTest × 100; TRX artifact | 19 October 2026 integration run: 136/136 = 100%; 0 failed/skipped; Debug with XPlat coverage; SDK 10.0.402 / runtime 8.0.32. Earlier 71-test and 76-test runs remain historical |
| Requirements verification | Fully verified requirements / requirements in scope × 100; RTM | PENDING |
| Defect severity distribution | Count unresolved defects by severity; include Deferred separately from dashboard open counts | NOT ASSESSED; test fixtures are not the team's product defect register |
| Automation coverage | Test cases automated / cases selected for automation × 100; test mapping | Selection denominator requires team agreement; 15 SQA cases have executed automated evidence (14 PASS including the SQA-18 browser criterion, SQA-20 FAIL). Human SQA-19 is separately reported executed, with 5/5 provisional scenario passes and final observer approval pending |
| Code coverage | Local Cobertura instrumented lines/branches in SecureApiQualityPlatform.Web | 19 October merged-source Debug run: 876/1141 lines = 76.77%; 325/494 branches = 65.78%; measured application package only. Historical 71-test and 76-test coverage retains its original source identity; CI pending |
| Commit activity | Count verifiable commits per member from Git history | PENDING |
| Dashboard PT-01 | Aggregation and complete HTTP document receipt for 2,000 deterministic results; browser separately | Historical 18 October 18:38 run PASS: aggregation first 21.4494 ms / warm median 0.7386 ms / max 8.7894 ms; HTTP first 223.0098 ms / warm median 1.8280 ms / max 2.6382 ms. SQA-18 later measured browser visual readiness separately below. Merged HTTP regression now uses an explicit temporary SQLite database |
| SQA-20 automated browser checks | Eight scoped technical categories in each installed browser, plus separate accessibility scans | 16/16 technical PASS, 0 FAIL/BLOCKED/NOT RUN; Chrome 155.0.8059.80 and Edge 155.0.4283.62. Additional axe/DOM naming and contrast checks FAIL, therefore overall SQA-20 FAIL. No human acceptance inferred |
| SQA-19 human usability | Five submitted interpretations and evidence pairs compared with the actual messages | Reported 19 October 2026, 15:00–16:00 NZDT: 5/5 provisional PASS recommendations, final Shirui approval pending. Tester-reported interpretations preceded ChatGPT wording assistance; no original unpolished transcript supplied |
| SQA-18 earlier browser Network | Five user-reported Network Finish values; screenshots retained; displayed rounded timings | 190, 55, 55, 55, 41 ms; median 55 ms; maximum 190 ms; DOMContentLoaded 15 ms each. Historical supporting evidence; Network Finish is not visual-render time; duplicate attachments disclosed |
| SQA-18 earlier browser paint | One separate matched-navigation Performance trace at 19:50:43 NZDT | FCP 89.415 ms; one LCP candidate 89.415 ms; historical supporting evidence, not finalized LCP or complete rendering; no trace-to-Network-run pairing established |
| SQA-18 visual readiness | Five independent reloads; navigation start to conservative screenshot-verified readiness upper bound, including stylesheet loading, layout and paint readiness | 257.9, 199.5, 197.1, 189.7, 201.0 ms; median **199.5 ms**; maximum **257.9 ms**; every value below 2,000 ms; dashboard values correct; fixture unchanged before/after |
| SQA-18 proposed operational acceptance | Exactly 2,000 deterministic results; correct metrics/severity; five consistent-condition navigation-to-visual-readiness measurements; all below 2,000 ms | **PASS against the user's proposed criterion**. Headed Chrome 155.0.8059.80, Playwright 1.62.1, viewport 1440 × 1600; screenshots and environment/settings retained. Criterion is not formally approved by the team; formal NFR-PERF-01 approval and release decision remain pending |

SQA-19 evidence: [unchanged submitted responses](qa-evidence/SQA-19/observation-sheet-session-eilmar.md), [proposed observer assessment and limitations](qa-evidence/SQA-19/observer-assessment-session-eilmar.md). Execution counts reflect that the human session was reported completed; approval is a separate outcome decision. The provisional case contributes once to the 22-case register. Its five scenarios are not five additional master cases. Automated tests and the [AI-assisted heuristic review](qa-evidence/SQA-20/playwright-20261019T035608Z/sqa19-heuristic-review.md) remain distinct from the human evidence.

| Master-register count | Before merging main | Current review state |
|---|---:|---:|
| Planned | 22 | 22 |
| Executed, including pending approval | 15 | 16 |
| Confirmed PASS | 13 | 14 |
| PASS (PROVISIONAL), approval pending | 1 | 1 |
| FAIL | 1 | 1 |
| BLOCKED | 0 | 0 |
| NOT RUN | 7 | 6 |

Current status reconciliation: 14 + 1 + 1 + 0 + 6 = 22. Executed: 14 + 1 + 1 = 16. If Shirui accepts the proposed SQA-19 outcome, confirmed register PASS becomes 15 and provisional PASS becomes 0; the executed denominator stays 16. Until then, do not publish 15 confirmed passes. SQA-18 is PASS against the documented proposed criterion only; formal NFR-PERF-01 approval remains pending. SQA-20 remains FAIL. Earlier execution ledgers retain their historical totals.

Latest integration validation: [merge and validation ledger](qa-evidence/integration-20261019T092140Z/README.md), [136-test TRX](qa-evidence/integration-20261019T092140Z/merged-suite.trx), [measured coverage](qa-evidence/integration-20261019T092140Z/cobertura.xml), [test summary](qa-evidence/integration-20261019T092140Z/test-summary.json) and [fixture-isolation smoke](qa-evidence/integration-20261019T092140Z/host-smoke/summary.json). These results identify the merged source via both parent SHAs and the retained source manifest; they do not rerun or extend the historical human or browser acceptance sessions.

Retained SQA-18 evidence: [five visual-readiness runs, screenshots and environment](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/README.md), [user-proposed operational criterion](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/PROPOSED-CRITERION.md). Earlier evidence: [user Network observations, trace extraction and limitations](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/browser-measurements/README.md), [isolated host/setup ledger](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/README.md), [76-test TRX](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/coverage-run/full-suite-coverage.trx), [historical 76-test Cobertura](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/cobertura.xml), [master register](ShiruiFinalTestCases.csv). The earlier uninstrumented [PT-01 execution ledger](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md) retains aggregation/HTTP timing evidence. Historical [earlier TRX](qa-evidence/2026-10-18/final.trx), [earlier coverage](qa-evidence/2026-10-18/cobertura.xml) and [environment/source identity](qa-evidence/2026-10-18/README.md) remain unchanged. Local setup used SDK 10.0.400 targeting net8.0 on .NET 8.0.30; workflow SDK 8 CI verification remains pending. The earlier failed coverage attempt is not counted as a passing suite.

Latest SQA-20 evidence: [execution ledger](qa-evidence/SQA-20/playwright-20261019T035608Z/README.md), [all 16 results](qa-evidence/SQA-20/playwright-20261019T035608Z/browser-execution.json), [accessibility findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md), [71-test TRX](qa-evidence/SQA-20/playwright-20261019T035608Z/dotnet/automated-suite.trx), [coverage](qa-evidence/SQA-20/playwright-20261019T035608Z/dotnet/cobertura.xml), [master register](ShiruiFinalTestCases.csv). This run used SDK 10.0.402 on Windows 10.0.26200 x64; the application host loads .NET 8.0.32. No production source changes were made for browser execution.

Retained PT-01 evidence: [execution ledger](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md), [dedicated PT-01 TRX](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/performance/performance.trx). Historical evidence: [earlier TRX](qa-evidence/2026-10-18/final.trx), [earlier Cobertura](qa-evidence/2026-10-18/cobertura.xml), [earlier environment/source identity](qa-evidence/2026-10-18/README.md). The earlier execution ledger records SDK 10.0.400/.NET 8.0.30 and a separate coverage-loading failure; those are not the latest run. CI on the submitted commit is still unverified.

## Release gate checklist

- [ ] Build passes on the submitted commit.
- [x] Merged-source local restore/build passed with 0 warnings/errors; 136/136 tests passed with actual coverage. Three isolated fixture hosts built and passed HTTP preparation smoke; prior runs remain historical.
- [ ] CI build/tests/coverage pass on the submitted commit; run URL and artifact retained.
- [ ] Manual acceptance and usability testing completed; evidence is linked.
- [ ] No unresolved Critical defect affecting core workflows; High issues have approved mitigations.
- [ ] Requirement coverage and defect traceability reviewed with Eilmar and Lana.
- [x] SQA-18 recorded five visual-readiness measurements below 2,000 ms with correct 2,000-result dashboard values, against the user-proposed operational criterion.
- [ ] Team/stakeholders formally approve NFR-PERF-01 and the operational acceptance rule. The historical local SQA-18 PASS does not supply this approval.
- [x] Automated CSV parsing, lifecycle, filters, malformed input, and apostrophe-prefix formula mitigation pass locally; spreadsheet application inspection remains manual.
- [ ] Known issues and ownership/mitigations are agreed by the team.

## Release decision (fill only after verification)

Decision: **PENDING / NOT ASSESSED BY THE TEAM**

Basis: [actual test run URL, failing tests, severe defects, accepted risk, unverified requirements]

Observed limitations for team assessment: the merged normal application persists API, endpoint, check and defect data in SQLite. Migration/backup operations require separate operational review. Server-side demo RBAC is present, defaults to Viewer and reads a user-selected role cookie; it is not authenticated identity or production authorization. DEF-010 remains open because defect status transitions are not enforced. The selected passive checks do not establish production security. Dashboard open counts exclude both Closed and Deferred; Deferred can still carry release risk. Numeric performance/compatibility targets remain provisional in Requirements.md. No AI-generated final report prose or human release decision is supplied here.

Residual risk and mitigation table:

| Risk / defect ID | Severity | User/system impact | Mitigation / responsible person | Due date | Evidence |
|---|---|---|---|---|---|
| SQA20-A11Y-01 | High (QA triage recommendation) | Endpoint and defect controls lack reliable programmatic label associations; axe fails 22 rendered control instances per browser, DOM inspection identifies 26 association gaps | Associate visible labels and unique control IDs; team owner/acceptance pending | PENDING | [Observed findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md) |
| SQA20-A11Y-02 | Medium (QA triage recommendation) | API BaseUrl text contrast 4.23:1 and Defects export-link contrast 4.4:1 fall below the tested 4.5:1 criterion | Adjust colours and remeasure; team owner/acceptance pending | PENDING | [Observed findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md) |
| SQA20-UI-01 | Low (QA triage recommendation) | Defect severity backgrounds stretch vertically; reproduced at narrow width, with labels still readable | Align badge to start of flex row; team owner/acceptance pending | PENDING | [Observed findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md) |

Do not infer measured line coverage from a suite's existence, or mark `pass` when the test was blocked or never executed.
