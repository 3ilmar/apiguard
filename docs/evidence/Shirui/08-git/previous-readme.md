# Shirui — Assignment 2 evidence index

This folder collects existing technical evidence for Shirui's ENSE707 Assignment 2 responsibilities: T3 test execution, T4 automation, T7 performance/usability, T8 quality/release inputs, individual T9/T11 evidence, Appendix A cases and inputs for slides 11–15. It is an evidence index, not student-authored report prose, a personal reflection or a release approval.

Collection date: **20 October 2026, Pacific/Auckland**. The separate local branch is `codex/shirui-assignment2-evidence`, created after fetching `origin` from [bd9cc4340f67fb75d2db36a8294d66cfeda6b9a9](https://github.com/3ilmar/apiguard/commit/bd9cc4340f67fb75d2db36a8294d66cfeda6b9a9). [PR #13](https://github.com/3ilmar/apiguard/pull/13) was already merged, and that baseline contains the approved SQA-19 assessment and SQA-20 corrections. The initial working tree on `main` was clean; both staged and unstaged diffs were empty. No application tests were rerun for this collection, and no commit, push or pull request was created.

## Authorship and testing attribution

| Evidence owner/type | What the records support |
|---|---|
| Shirui / Git author `SaltedFishInATank` | Assignment 2 dashboard aggregation/severity/no-data handling, CSV reporting and related tests; PT-01 and browser-evidence work; SQA-19/20 evidence organisation and accessibility corrections; final observer approval and QA register/metrics. Commit records establish authorship of changes, not unaided execution. |
| Codex assistance | Documented code/test development, automated MSTest/HTTP/Playwright/axe verification, heuristic inspection and technical documentation. This consolidation and its validation are also Codex-assisted. Historical ledgers identify the actual executor and tested source state. |
| Eilmar / `EILMAR-P01` | Human tester for the five SQA-19 scenarios. Shirui is the observer/approver. Eilmar also contributed persistence and the SQLite adaptation of the dashboard HTTP performance test; these are teammate contributions. |
| Lana and other shared team work | RBAC, security, reliability tests, defect records and shared CI/requirements documentation remain attributed to their contributors. Included current source files are combined team snapshots. Merging a teammate's PR does not establish authorship of that work. |
| Earlier `ShiruiZang` commits / PR #2 | August 2026 dashboard/reporting baseline context, kept separate from October Assignment 2 activity. |

See [Git/API evidence](08-Git-and-Contributions/) and the [commit register](08-Git-and-Contributions/git/contributions.csv). The GitHub snapshots preserve Lana's PR #5 review comment and Eilmar's PR #7 approval. PR #12/#13 returned no review submissions/comments when collected; PR #13's merge and recorded observer approval must not be relabelled as a separate GitHub reviewer approval.

## Where to find the evidence

Each original file was copied once. The repository-relative source path is retained beneath its category. The [manifest](evidence-manifest.csv) provides one record per collected/generated file, attribution, date when recorded, status, source path, destination and SHA-256 where applicable. Its source-commit field describes the file's Git provenance; use the original execution ledger/source-hash records for the software state actually tested.

| Category | Assignment / scope | Original locations | Copied evidence |
|---|---|---|---|
| 01 Dashboard and reporting | T3/T4; SQA-10–16/21, dashboard and CSV requirements | [Dashboard service](../../../SecureApiQualityPlatform.Web/Services/DashboardService.cs), [report controller](../../../SecureApiQualityPlatform.Web/Controllers/ReportsController.cs), related view/controller/model | [Current combined source](01-Dashboard-and-Reporting/SecureApiQualityPlatform.Web/); test snapshots are in category 03, UI/browser captures in categories 04/06 |
| 02 Test execution | T3 / Appendix A; historical actual runs | [18 October ledger](../../qa-evidence/2026-10-18/README.md), [integration run](../../qa-evidence/integration-20261019T092140Z/README.md), repository `TestResults/` | [Initial execution records](02-Test-Execution/docs/qa-evidence/2026-10-18/), [integration records](02-Test-Execution/docs/qa-evidence/integration-20261019T092140Z/), [local TRX/coverage records](02-Test-Execution/TestResults/) |
| 03 Test automation | T4; MSTest, regression, CSV escaping/structure, message/edge tests | [Automation suite](../../AutomationSuite-Shirui.md), application test project, `.github/workflows/`, `tools/PostMergeRegression/` | [Suite evidence](03-Test-Automation/docs/AutomationSuite-Shirui.md), [test source snapshots](03-Test-Automation/SecureApiQualityPlatform.Tests/), [CI snapshots](03-Test-Automation/.github/workflows/), [HTTP runner](03-Test-Automation/tools/PostMergeRegression/) |
| 04 Performance SQA-18 | T7; PT-01 / proposed NFR-PERF-01 | [Performance index](../../qa-evidence/SQA18-Browser-Performance.md), all `pt01-*`/`sqa18-*` runs, `tools/Pt01BrowserHost/` | [Index](04-Performance-SQA18/docs/qa-evidence/SQA18-Browser-Performance.md), [run folders](04-Performance-SQA18/docs/qa-evidence/2026-10-18/), [host/measurement scripts](04-Performance-SQA18/tools/Pt01BrowserHost/) |
| 05 Human usability SQA-19 | T7; TC-12 / NFR-USA-01 | [Original SQA-19 folder](../../qa-evidence/SQA-19/), `tools/Sqa19.ManualHost/` | [Unchanged human submission](05-Usability-SQA19/docs/qa-evidence/SQA-19/observation-sheet-session-eilmar.md), [final observer assessment](05-Usability-SQA19/docs/qa-evidence/SQA-19/observer-assessment-session-eilmar.md), [procedure and setup](05-Usability-SQA19/docs/qa-evidence/SQA-19/), [host source](05-Usability-SQA19/tools/Sqa19.ManualHost/) |
| 06 Accessibility SQA-20 | T7; compatibility/accessibility / NFR-USA-01 | [Original SQA-20 folder](../../qa-evidence/SQA-20/), `tools/Sqa20.ManualHost/`, relevant views/CSS | [Initial FAIL](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/playwright-20261019T035608Z/), [before/after retest collection](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/retest-20261019T095150Z/), [scripts and pinned axe/license](06-Accessibility-SQA20/tools/Sqa20.ManualHost/), [current source](06-Accessibility-SQA20/SecureApiQualityPlatform.Web/) |
| 07 Quality metrics | T8; case register, coverage/release inputs | [Master register](../../ShiruiFinalTestCases.csv), [metrics](../../ShiruiQualityMetrics-Evidence.md), shared requirements/RTM/test plans/defects | [Register](07-Quality-Metrics/docs/ShiruiFinalTestCases.csv), [metrics](07-Quality-Metrics/docs/ShiruiQualityMetrics-Evidence.md), [shared context documents](07-Quality-Metrics/docs/) |
| 08 Git and contributions | Individual T9/T11 evidence | Actual local Git history and read-only GitHub API responses | [Commits](08-Git-and-Contributions/git/), [PR/review snapshots](08-Git-and-Contributions/github-pr-snapshots.json), [verification records](08-Git-and-Contributions/verification/) |

These category/requirement mappings identify relevance, not proof that every mapped requirement has passed. The unchanged [final register](07-Quality-Metrics/docs/ShiruiFinalTestCases.csv) records case-level preconditions, steps, expected and actual results, status and evidence.

## Recorded outcomes and limits

### Dashboard, reporting and automation

Dashboard snapshots show open-defect severity distribution, aggregate metrics, recent results/defects and explicit no-data handling. CSV snapshots/tests cover **15 result columns and 21 defect columns**, lifecycle/filter fields, independent logical-row parsing, commas/quotes/newlines, formula prefixes, missing context, invalid filters and empty exports. Relevant cases include SQA-10–16/21; message checks support SQA-19 and the simulated transport case is labelled separately in SQA-22.

Historical test populations differ and overlap; **do not add these counts together**:

| Historical record | Actual result / scope |
|---|---|
| [Initial baseline and final records](02-Test-Execution/docs/qa-evidence/2026-10-18/) | 30/30 baseline, then 70/70 Release MSTest, 8/8 HTTP smoke. Initial coverage: 402/629 lines (63.91%), 183/390 branches (46.92%). |
| [Final dedicated PT-01 run](04-Performance-SQA18/docs/qa-evidence/2026-10-18/pt01-20261018T053834379Z-eb0590/README.md) | 71/71 regression and 2/2 dedicated checks; final performance execution collected no coverage. Earlier missing-attachment attempts and a Windows Application Control coverage failure remain archived. |
| [Integration run](02-Test-Execution/docs/qa-evidence/integration-20261019T092140Z/README.md) | 136/136 Debug MSTest; coverage 876/1,141 lines (76.77%), 325/494 branches (65.78%). This is not a new human/browser session. |
| [Post-fix TRX](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/retest-20261019T095150Z/postfix-suite/postfix.trx) and [console](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/retest-20261019T095150Z/postfix-test.txt) | 136/136 Release MSTest; no failed/skipped tests. Release coverage: 715/918 lines (77.89%), 325/492 branches (66.06%). |
| [Separate verified HTTP regression](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/retest-20261019T095150Z/postfix-http-verified/summary.json) | **28 PASS / 1 FAIL**; numeric endpoint validation remains open. Passing MSTest results do not erase this failure. |

The historical ledgers retain OS, SDK/runtime, tool versions, timestamps, source identities and test data. Examples: initial SDK 10.0.400/.NET 8.0.30; later integration SDK 10.0.402/.NET 8.0.32 on Windows 10.0.26200. Use each run's environment record rather than assuming identical configurations or SDK 8 CI parity.

### SQA-18 performance

The final deterministic fixture has **2,000 results**, 1,000 pass/1,000 fail, 80 defects with 48 open (12 per severity), and one API/endpoint. Aggregation includes all results; the page displays 12 recent results and 8 recent defects.

- Dedicated aggregation: first 21.4494 ms; warm median 0.7386 ms, P95 0.9888 ms, maximum 8.7894 ms.
- Complete-document HTTP: first 223.0098 ms; warm median 1.8280 ms, P95 2.4933 ms, maximum 2.6382 ms.
- [Five independent visual measurements](04-Performance-SQA18/docs/qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/results.json): **257.9, 199.5, 197.1, 189.7, 201.0 ms**; median **199.5 ms**, maximum **257.9 ms**. All five screenshots, HTML, HAR, traces, environment and measurement scripts are copied in that [run folder](04-Performance-SQA18/docs/qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/).

These are conservative screenshot-verified readiness upper bounds including capture/automation overhead, using headed Chrome 155.0.8059.80, Playwright 1.62.1, 1440×1600 and a warmed isolated local Release server. **PASS applies to the documented user-proposed under-2,000-ms criterion; formal NFR-PERF-01 approval remains pending.** See the [proposal](04-Performance-SQA18/docs/qa-evidence/2026-10-18/sqa18-visual-20261018T072617937Z/PROPOSED-CRITERION.md). They do not measure startup, remote latency, production TLS, concurrency or every browser/viewport. Earlier user-supplied Network screenshots remain separate evidence with duplicate images and incomplete metadata disclosed in their original ledger.

### SQA-19 human usability

Eilmar (`EILMAR-P01`) supplied five tester-reported interpretations for the **19 October 2026, 15:00–16:00 NZDT** session: HTTP status, duration threshold, authentication expectation, content type and timeout. Shirui accepted all five interpretations and supporting explanations. The final outcome is **5/5 PASS; SQA-19 PASS — observer approved**, counted once in the master register. The [original submission](05-Usability-SQA19/docs/qa-evidence/SQA-19/observation-sheet-session-eilmar.md) retains its blank observer fields; the approval is in the [separate assessment](05-Usability-SQA19/docs/qa-evidence/SQA-19/observer-assessment-session-eilmar.md) and [finalisation ledger](05-Usability-SQA19/docs/qa-evidence/SQA-19/final-approval-20261019/README.md).

ChatGPT lightly polished the submitted wording after the tester reports forming his interpretations. Original unpolished answers, raw recording/transcript, detailed assistance exchange, session-specific screenshots, exact response times and independently verified session build/browser metadata were not supplied. No signature or precise approval timestamp is inferred. The 18 October preparation screenshot is automated setup evidence, not a human-session screenshot. The [AI-assisted heuristic inspection](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/playwright-20261019T035608Z/sqa19-heuristic-review.md) is supplementary and keeps its historical pending-human-test wording.

### SQA-20 accessibility and compatibility

| Stage | Preserved evidence / result |
|---|---|
| [Original execution](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/playwright-20261019T035608Z/README.md) | Narrow technical categories **16/16 PASS**, but **overall FAIL** because additional accessible-name/contrast checks reproduced defects. |
| [Post-merge before state](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/retest-20261019T095150Z/before/browser-execution.json) | **14 PASS / 2 FAIL**, using a stronger compact severity-badge criterion. |
| [After corrections](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/retest-20261019T095150Z/after/browser-execution.json) | **16/16 PASS**, overall automated retest PASS. Before/after screenshots, axe outputs, keyboard traversal, CSV downloads and console logs remain available. |

Recorded browsers are Chrome 155.0.8059.80 and Edge 155.0.4283.62, with Playwright 1.62.1 and axe-core 4.10.3 at 1440×900 and 375×812 on synthetic localhost fixtures. Corrections cover associated labels/unique IDs, URL/export contrast and compact severity badges; the [application diff](06-Accessibility-SQA20/docs/qa-evidence/SQA-20/retest-20261019T095150Z/application-fix.diff) and current view/CSS snapshots preserve implementation evidence. Four raw axe scroll-wrapper flags remain recorded alongside successful keyboard traversal/scrolling; they were not confirmed as product defects for these browser versions. Full WCAG compliance, screen-reader acceptance, representative-user accessibility acceptance and a release decision were not established.

## Metrics, remaining work and release inputs

The final [master register](07-Quality-Metrics/docs/ShiruiFinalTestCases.csv) contains **22 planned; 16 executed; 16 PASS; 0 provisional PASS; 0 FAIL; 0 BLOCKED; 6 NOT RUN**. Execution is **16/22 = 72.73%** and pass rate among executed master cases is **16/16 = 100%**. These counts exclude the separate 29-check HTTP regression, its open failure and overlapping automation populations.

- **SQA-01/02/03/05/07/08 remain NOT RUN** in this register: registration UI, private-target UI rejection, endpoint UI, protected-endpoint negative workflow, actual network timeout and defect UI workflow. Other unit/team evidence does not silently complete those cases.
- **PM-VALIDATION-01 remains open:** malformed numeric `ExpectedStatusCode` binding errors are cleared, leading to HTTP 302 and persisted default 200. See the [technical assessment](05-Usability-SQA19/docs/qa-evidence/SQA-19/numeric-endpoint-validation-assessment.md) and verified HTTP records. No fix or teammate assignment is claimed here.
- **DEF-010 remains open** for lifecycle-transition enforcement. The [team defect log](07-Quality-Metrics/docs/DefectLog.md) supplies context; Lana's work is not attributed to Shirui.
- Formal performance-criterion approval, RTM/requirement coordination, broader accessibility/security acceptance and the team's release decision remain human/team inputs. Current storage uses SQLite; older in-memory/no-RBAC limitations describe earlier states. Demo role selection is not production authentication.
- The exact final evidence commit has not been created or tested in CI. The PR #13 API snapshot contains historical CI links, including [.NET quality checks](https://github.com/3ilmar/apiguard/actions/runs/39238655229) and [ENSE707 Quality Gate](https://github.com/3ilmar/apiguard/actions/runs/39238655356). Their statuses are recorded in the PR description; workflow artifacts/logs were not independently downloaded or reverified during this collection.
- `docs/CopilotEvidence.md` contains incomplete prompt templates, not an authentic completed Shirui interaction history. It was not copied as proof of actual prompts. No missing AI conversation, human decision, screenshot or test execution was reconstructed.

## Git contribution evidence

| Change | Actual commit / PR |
|---|---|
| Dashboard/CSV/tests and initial QA evidence | [ced44546ba9c71f25935e6ba913b27fe08f0acd1](https://github.com/3ilmar/apiguard/commit/ced44546ba9c71f25935e6ba913b27fe08f0acd1), [PR #5](https://github.com/3ilmar/apiguard/pull/5) |
| SQA-18 browser measurements | [d99f6c7b2c4fde9baa911000d169ef9bc36b568a](https://github.com/3ilmar/apiguard/commit/d99f6c7b2c4fde9baa911000d169ef9bc36b568a), [PR #7](https://github.com/3ilmar/apiguard/pull/7) |
| Original SQA-20 FAIL and supporting heuristic evidence | [ff077fce7bdd5dc4891dcc74e8dd01c245789512](https://github.com/3ilmar/apiguard/commit/ff077fce7bdd5dc4891dcc74e8dd01c245789512), [PR #12](https://github.com/3ilmar/apiguard/pull/12) |
| Human submission / integration | [d551a2defd48c72f7b065b865415fd9aee3d4fb1](https://github.com/3ilmar/apiguard/commit/d551a2defd48c72f7b065b865415fd9aee3d4fb1), [18c6401b6618d8f8f16afebc504349705b120c86](https://github.com/3ilmar/apiguard/commit/18c6401b6618d8f8f16afebc504349705b120c86) |
| SQA-20 accessibility corrections/retest | [e43fe52373d3d74b617610e2dbee418ac9b3f9b4](https://github.com/3ilmar/apiguard/commit/e43fe52373d3d74b617610e2dbee418ac9b3f9b4), direct commit already in fetched main |
| Final observer approval and metrics | [ca4a49713e9501b2b6194897012b9473c8a8ca2c](https://github.com/3ilmar/apiguard/commit/ca4a49713e9501b2b6194897012b9473c8a8ca2c), [PR #13](https://github.com/3ilmar/apiguard/pull/13) |
| Eilmar's SQLite adaptation (teammate contribution) | [5ec4efc653ffe7f9321e2e59485b29448cd0d31e](https://github.com/3ilmar/apiguard/commit/5ec4efc653ffe7f9321e2e59485b29448cd0d31e) |

The [API snapshot](08-Git-and-Contributions/github-pr-snapshots.json) preserves actual PR metadata/reviews, while [Git exports](08-Git-and-Contributions/git/) preserve author/date/commit details and scoped implementation patches. Earlier August changes are labelled background in the index and remain identifiable by their dates.

## Preservation and verification

**1,737 original files (133,589,506 bytes / 127.4 MiB)** were selected and copied after the count/size was reported. This includes existing duplicate attachments, failed attempts, ignored local coverage/TRX/logs and six synthetic isolated test databases. All formats and original contents were retained; `copy2` preserves last-write timestamps. The scoped `.gitignore` allows archived artifacts into a future review, and `.gitattributes` prevents Git line-ending conversion of these archived bytes. Build caches, dependencies, `.git`, IDE files, unrelated teammate evidence folders and production databases were excluded.

The [verification result](08-Git-and-Contributions/verification/validation.json) records original/copy hashes, timestamps, all pre-existing tracked-file hashes, manifest coverage, local links in **this new README**, preserved SQA-19 approval/SQA-20 FAIL/retest anchors and unchanged application/test/workflow sources. The [full file tree](08-Git-and-Contributions/verification/directory-tree.txt) and [copy plan](08-Git-and-Contributions/verification/copy-plan.json) give exact paths. Counts distinguish original copies from newly generated collection/index/verification files; the manifest and validation output omit their own hashes to avoid circular self-reference.

Historical README/procedure snapshots keep their original relative links and wording. The audit resolved **310 original README links**; **17 literal relative links in the copies** point outside their new category and do not resolve at their copied location. They were **not rewritten**, to preserve the evidence. Use the original-location column above or the [archived README link map](08-Git-and-Contributions/verification/archived-readme-links.csv), which resolves links against the original path and maps copied targets when available. The manifest contains every actual copy; no requested file was silently replaced with a link. Preserve historical “uncommitted,” pending SQA-19 or FAIL wording as an as-of record; use the final assessment/retest and actual Git provenance for current status.

To repeat collection validation only, from the repository root:

```powershell
python docs/evidence/Shirui/08-Git-and-Contributions/verification/verify-evidence.py
```

This checks evidence integrity and writes only the new collection's manifest/link map/tree/validation records. Archived test/measurement scripts are source snapshots; do not run them in place because their original paths/output assumptions may overwrite historical records.

The [initial collection validation](08-Git-and-Contributions/verification/validation-initial.json) recorded a first-run ordering issue: three links were checked before their generated targets existed. The verifier was corrected to generate those targets before checking links; no original/copy hash or timestamp failed. It uses `git -c core.longpaths=true` per invocation for the deep Windows archive paths, without changing Git configuration. A future Git add/commit may need that option on Windows. The latest validation result supersedes the initial collection check.
