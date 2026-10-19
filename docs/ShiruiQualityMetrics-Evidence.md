# Quality metric calculations and release evidence worksheet

> Supporting QA calculations only. The final submitted assessment report must be authored by the students.

## Metric definitions

| Metric | Calculation / source | Current measured value |
|---|---|---|
| Test execution rate | Executed / Planned × 100; totals recalculated from the current master register | 16/22 = 72.73%; 16 PASS (SQA-19 observer approved; SQA-20 automated retest; SQA-18 proposed criterion caveat retained), 0 provisional PASS, 0 FAIL, 6 NOT RUN, 0 BLOCKED |
| Test pass rate | Passed / all executed × 100; exclude blocked and not run | Confirmed register PASS: 16/16 = 100%. Six cases remain NOT RUN. Separate HTTP regression remains 28 PASS / 1 FAIL; do not infer overall release readiness from master-register outcomes |
| Automated test pass rate | Passed MSTest / executed MSTest × 100; TRX artifact | 19 October 2026 post-merge baseline and post-fix Release runs: 136/136 = 100% each; 0 failed/skipped; XPlat coverage; SDK 10.0.402 / runtime 8.0.32. Historical Debug and 71/76-test runs retain their own identities |
| Requirements verification | Fully verified requirements / requirements in scope × 100; RTM | PENDING |
| Defect severity distribution | Count unresolved defects by severity; include Deferred separately from dashboard open counts | NOT ASSESSED; test fixtures are not the team's product defect register |
| Automation coverage | Test cases automated / cases selected for automation × 100; test mapping | Selection denominator requires team agreement; 15 SQA cases have executed automated evidence, PASS including SQA-20 retest and SQA-18 proposed criterion. Human SQA-19 is separate: 5/5 PASS, observer approved by Shirui; it counts once in the master register |
| Code coverage | Local Cobertura instrumented lines/branches in SecureApiQualityPlatform.Web | Baseline and post-fix Release: 715/918 lines = 77.89%; 325/492 branches = 66.06%. Historical merged Debug: 876/1141 lines = 76.77%, 325/494 branches = 65.78%; configuration/source denominators differ. CI pending |
| Commit activity | Count verifiable commits per member from Git history | PENDING |
| Dashboard PT-01 | Aggregation and complete HTTP document receipt for 2,000 deterministic results; browser separately | Historical 18 October 18:38 run PASS: aggregation first 21.4494 ms / warm median 0.7386 ms / max 8.7894 ms; HTTP first 223.0098 ms / warm median 1.8280 ms / max 2.6382 ms. SQA-18 later measured browser visual readiness separately below. Merged HTTP regression now uses an explicit temporary SQLite database |
| SQA-20 automated browser checks | Eight technical categories per installed browser, plus original-location accessibility retest | Retest: 16/16 PASS, 0 FAIL/BLOCKED/NOT RUN; Chrome 155.0.8059.80, Edge 155.0.4283.62, both widths. 26/26 original controls per browser/viewport labelled; URL contrast 6.59, export 7.69 resting / 8.18 interactive; compact badges. Raw scroll-wrapper flags preserved with native keyboard proof. Automated SQA-20 PASS on retest; historical FAIL retained; no human acceptance inferred |
| Normal SQLite HTTP regression | Isolated real MVC HTTP, owned loopback target and SQLite restart | Baseline and post-fix: 29 executed, 28 PASS, 1 pre-existing numeric-binding FAIL. Final post-fix CSV validation checks exact headers and raw logical-record widths; older assertions were less strict. No new HTTP regression observed; overall HTTP status remains FAIL |
| SQA-19 human usability | Five submitted interpretations and supporting explanations accepted by observer Shirui | PASS — observer approved; 5/5 PASS. EILMAR-P01 session 19 October 2026, reported 15:00–16:00 NZDT. Shirui explicitly approved all five outcomes in this conversation. ChatGPT assisted wording after tester-reported interpretation; original unpolished answers and session-specific screenshot absent; some unaided-session aspects independently uncorroborated. No signature or precise approval timestamp inferred |
| SQA-18 earlier browser Network | Five user-reported Network Finish values; screenshots retained; displayed rounded timings | 190, 55, 55, 55, 41 ms; median 55 ms; maximum 190 ms; DOMContentLoaded 15 ms each. Historical supporting evidence; Network Finish is not visual-render time; duplicate attachments disclosed |
| SQA-18 earlier browser paint | One separate matched-navigation Performance trace at 19:50:43 NZDT | FCP 89.415 ms; one LCP candidate 89.415 ms; historical supporting evidence, not finalized LCP or complete rendering; no trace-to-Network-run pairing established |
| SQA-18 visual readiness | Five independent reloads; navigation start to conservative screenshot-verified readiness upper bound, including stylesheet loading, layout and paint readiness | 257.9, 199.5, 197.1, 189.7, 201.0 ms; median **199.5 ms**; maximum **257.9 ms**; every value below 2,000 ms; dashboard values correct; fixture unchanged before/after |
| SQA-18 proposed operational acceptance | Exactly 2,000 deterministic results; correct metrics/severity; five consistent-condition navigation-to-visual-readiness measurements; all below 2,000 ms | **PASS against the user's proposed criterion**. Headed Chrome 155.0.8059.80, Playwright 1.62.1, viewport 1440 × 1600; screenshots and environment/settings retained. Criterion is not formally approved by the team; formal NFR-PERF-01 approval and release decision remain pending |

SQA-19 evidence: [unchanged submitted responses](qa-evidence/SQA-19/observation-sheet-session-eilmar.md), [final observer approval and continuing limitations](qa-evidence/SQA-19/observer-assessment-session-eilmar.md), [register-derived totals and verification](qa-evidence/SQA-19/final-approval-20261019/README.md). Shirui's explicit conversational approval finalises the five scenario outcomes with their limitations retained. The session contributes one PASS to the 22-case register, not five master cases. The original sheet's blank observer fields and historical pending labels remain unchanged; the separate final assessment supplies the current outcome. Automated tests and the [historical AI-assisted heuristic review](qa-evidence/SQA-20/playwright-20261019T035608Z/sqa19-heuristic-review.md) remain distinct from human evidence.

| Master-register count | Before merging main | Merged baseline | After accessibility retest | After SQA-19 observer approval |
|---|---:|---:|---:|---:|
| Planned | 22 | 22 | 22 | 22 |
| Executed | 15 | 16 | 16 | 16 |
| Confirmed PASS | 13 | 14 | 15 | 16 |
| Provisional PASS (historical) | 1 | 1 | 1 | 0 |
| FAIL | 1 | 1 | 0 | 0 |
| BLOCKED | 0 | 0 | 0 | 0 |
| NOT RUN | 7 | 6 | 6 | 6 |

Current status reconciliation: 16 + 0 + 0 + 0 + 6 = 22. Executed: 16 PASS + 0 FAIL = 16. These counts and rates are calculated from the actual [register](ShiruiFinalTestCases.csv), with [machine-readable totals](qa-evidence/SQA-19/final-approval-20261019/qa-totals.json). SQA-19 is PASS — observer approved. SQA-18 remains PASS against its proposed criterion only; formal NFR-PERF-01 approval is pending. SQA-20 is PASS ON AUTOMATED RETEST only, with historical FAIL retained. The separate numeric-validation HTTP failure remains open. Earlier ledgers, the [pre-observer-approval register](qa-evidence/SQA-19/final-approval-20261019/baseline-register.csv) and [pre-accessibility-retest register](qa-evidence/SQA-20/retest-20261019T095150Z/initial-register.csv) retain historical totals.

Latest integration validation: [merge and validation ledger](qa-evidence/integration-20261019T092140Z/README.md), [136-test TRX](qa-evidence/integration-20261019T092140Z/merged-suite.trx), [measured coverage](qa-evidence/integration-20261019T092140Z/cobertura.xml), [test summary](qa-evidence/integration-20261019T092140Z/test-summary.json) and [fixture-isolation smoke](qa-evidence/integration-20261019T092140Z/host-smoke/summary.json). These results identify the merged source via both parent SHAs and the retained source manifest; they do not rerun or extend the historical human or browser acceptance sessions.

Retained SQA-18 evidence: [five visual-readiness runs, screenshots and environment](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/README.md), [user-proposed operational criterion](qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/PROPOSED-CRITERION.md). Earlier evidence: [user Network observations, trace extraction and limitations](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/browser-measurements/README.md), [isolated host/setup ledger](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/README.md), [76-test TRX](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/coverage-run/full-suite-coverage.trx), [historical 76-test Cobertura](qa-evidence/2026-10-18/sqa18-browser-20261018T062538973Z/cobertura.xml), [master register](ShiruiFinalTestCases.csv). The earlier uninstrumented [PT-01 execution ledger](qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md) retains aggregation/HTTP timing evidence. Historical [earlier TRX](qa-evidence/2026-10-18/final.trx), [earlier coverage](qa-evidence/2026-10-18/cobertura.xml) and [environment/source identity](qa-evidence/2026-10-18/README.md) remain unchanged. Local setup used SDK 10.0.400 targeting net8.0 on .NET 8.0.30; workflow SDK 8 CI verification remains pending. The earlier failed coverage attempt is not counted as a passing suite.

Latest SQA-20 retest: [factual execution ledger](qa-evidence/SQA-20/retest-20261019T095150Z/README.md), [comparison and source identity](qa-evidence/SQA-20/retest-20261019T095150Z/comparison.json), [browser categories](qa-evidence/SQA-20/retest-20261019T095150Z/after/browser-execution.json), [targeted labels/contrast](qa-evidence/SQA-20/retest-20261019T095150Z/after-targets/accessibility-targets.json), [136-test post-fix TRX](qa-evidence/SQA-20/retest-20261019T095150Z/postfix-suite/postfix.trx), [HTTP regression](qa-evidence/SQA-20/retest-20261019T095150Z/postfix-http-verified/summary.json). The run captured the accessibility diff over PR #12 merge `7f28ee9`, subsequently committed on main as `e43fe52373d3d74b617610e2dbee418ac9b3f9b4`. No human accessibility acceptance or full WCAG conformance was established. Historical FAIL evidence remains unchanged: [original ledger](qa-evidence/SQA-20/playwright-20261019T035608Z/README.md), [findings](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md), [71-test TRX](qa-evidence/SQA-20/playwright-20261019T035608Z/dotnet/automated-suite.trx). At that retest, 361 historical SQA-19/SQA-20 evidence files were verified unchanged; this later observer finalisation deliberately updates only the current SQA-19 README/assessment among those files.

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

Post-merge HTTP additionally confirms malformed numeric endpoint status input can persist default 200 because binding errors are cleared. This predates PR #12 and remains open for separate ownership; see [actual failing request and persisted row](qa-evidence/SQA-20/retest-20261019T095150Z/postfix-http-verified/findings.json).

Residual risk and mitigation table:

| Risk / defect ID | Severity | User/system impact | Mitigation / responsible person | Due date | Evidence |
|---|---|---|---|---|---|
| SQA20-A11Y-01 | High (historical QA triage recommendation) | Original 22 axe naming failures / 26 DOM gaps per browser reproduced | Fixed locally; associated labels/unique IDs; 26/26 targeted controls pass per browser/viewport; review pending | Review pending | [Original FAIL](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md), [retest](qa-evidence/SQA-20/retest-20261019T095150Z/after-targets/accessibility-targets.json) |
| SQA20-A11Y-02 | Medium (historical QA triage recommendation) | Original URL 4.23:1 / export 4.4:1 reproduced | Fixed locally; measured 6.59 / 7.69 resting, 8.18 interactive; review pending | Review pending | [Original FAIL](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md), [retest](qa-evidence/SQA-20/retest-20261019T095150Z/after-targets/accessibility-targets.json) |
| SQA20-UI-01 | Low (historical QA triage recommendation) | Original vertically stretched severity badges reproduced at both widths | Fixed locally with flex self-alignment; 0 stretched badges in both browsers/widths; review pending | Review pending | [Original FAIL](qa-evidence/SQA-20/playwright-20261019T035608Z/findings.md), [retest](qa-evidence/SQA-20/retest-20261019T095150Z/after/browser-execution.json) |
| PM-VALIDATION-01 | Triage pending | Malformed numeric expected-status input silently persists default 200; existing before PR #12 | OPEN; coordinate separate fix/ownership with Eilmar/Lana; accessibility branch leaves logic unchanged | PENDING | [HTTP finding](qa-evidence/SQA-20/retest-20261019T095150Z/postfix-http-verified/findings.json), [parent source](qa-evidence/SQA-20/retest-20261019T095150Z/endpoint-validation-parent-source.txt) |

Do not infer measured line coverage from a suite's existence, or mark `pass` when the test was blocked or never executed.
