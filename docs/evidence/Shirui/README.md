# Shirui — Assignment 2 evidence

Factual evidence index for T3 execution, T4 automation, T7 performance/usability/accessibility, T8 quality/release inputs and individual T9/T11 contributions. This is not final student report prose or a release approval.

This compact organisation replaces the initial deeply nested copy on `codex/shirui-assignment2-evidence`. The baseline remains [bd9cc4340f67fb75d2db36a8294d66cfeda6b9a9](https://github.com/3ilmar/apiguard/commit/bd9cc4340f67fb75d2db36a8294d66cfeda6b9a9), containing merged [PR #13](https://github.com/3ilmar/apiguard/pull/13). Original QA records, application/test/workflow source and `main` are unchanged. No application tests were rerun and no commit, push or PR was created for this repair.

## Evidence locations

| Responsibility | Directly accessible records | Bulk evidence |
|---|---|---|
| Dashboard/reporting; FR-07/08, SQA-10–16/21 | [Combined team source snapshots](01-dashboard/), [related tests](03-automation/) | Relevant browser captures are in performance/accessibility collections. |
| T3 test execution | [Initial ledger](02-execution/initial.md), [baseline TRX](02-execution/baseline.trx), [final TRX](02-execution/final.trx), [integration ledger](02-execution/integration.md), [136-test integration TRX](02-execution/merged-suite.trx) | [Execution archive](02-execution/raw.zip): raw logs, ignored local TestResults/coverage, supporting records. |
| T4 automation | [Suite description](03-automation/suite.md), [MSTest source/configuration and CI workflow snapshots](03-automation/), [HTTP regression runner](03-automation/http-regression.py) | [Automation archive](03-automation/raw.zip): remaining supporting documentation. |
| T7 performance; SQA-17/18, PT-01, NFR-PERF-01 | [Performance index](04-performance/index.md), [PT-01 ledger/data](04-performance/pt01/), [browser ledger](04-performance/visual/ledger.md), [five results](04-performance/visual/results.json), [proposed criterion](04-performance/visual/PROPOSED-CRITERION.md), [host/runner source](04-performance/tools/) | [Performance archive](04-performance/raw.zip): failed attempts, TRX attachments, traces/HAR, HTML, logs and earlier user-supplied browser captures. |
| T7 human usability; SQA-19 / TC-12 | [Eilmar's original submission](05-usability/observation-sheet-session-eilmar.md), [Shirui's approved assessment](05-usability/observer-assessment-session-eilmar.md), [approval ledger](05-usability/approval-ledger.md), [AI-assisted heuristic inspection](05-usability/ai-heuristic.md), [procedure](05-usability/README.md) | [Usability archive](05-usability/raw.zip): preparation, observation templates, earlier assessments and supporting source. |
| T7 accessibility/compatibility; SQA-20 | [Initial FAIL ledger](06-accessibility/initial.md), [initial browser result](06-accessibility/initial-browser.json), [initial findings](06-accessibility/initial-findings.md), [retest ledger](06-accessibility/retest.md), [before](06-accessibility/before-browser.json), [after](06-accessibility/after-browser.json), [application diff](06-accessibility/application-fix.diff), [136-test Release TRX](06-accessibility/postfix.trx), [coverage XML](06-accessibility/coverage.xml) | [Accessibility archive](06-accessibility/raw.zip): all unique before/after screenshots, axe/Playwright outputs, logs, CSVs, synthetic test databases and supporting scripts/source. |
| T8 metrics/release inputs | [Final register](07-metrics/ShiruiFinalTestCases.csv), [quality metrics](07-metrics/ShiruiQualityMetrics-Evidence.md), [shared requirements/RTM/test plans/defects](07-metrics/), [HTTP failure summary](06-accessibility/http-summary.json), [numeric-defect assessment](05-usability/numeric-endpoint-validation-assessment.md) | Shared documents retain their original attribution and historical wording. |
| T9/T11 Git contributions | [Commit register](08-git/git/contributions.csv), [Git exports](08-git/git/), [actual GitHub PR/review snapshots](08-git/github-pr-snapshots.json) | [Previous collection metadata archive](08-git/raw.zip); [previous index](08-git/previous-readme.md) is historical and its old copied paths are superseded. |

All five browser contexts remain directly accessible: [run 1](04-performance/visual/r1.json) / [PNG](04-performance/visual/r1.png), [run 2](04-performance/visual/r2.json) / [PNG](04-performance/visual/r2.png), [run 3](04-performance/visual/r3.json) / [PNG](04-performance/visual/r3.png), [run 4](04-performance/visual/r4.json) / [PNG](04-performance/visual/r4.png), [run 5](04-performance/visual/r5.json) / [PNG](04-performance/visual/r5.png). Their deterministic screenshots are byte-identical, but each run's context and measurement record is retained.

## Results and attribution

- **Master register:** 22 planned, 16 executed/PASS, 6 NOT RUN; 72.73% execution and 100% pass among executed master cases. SQA-01/02/03/05/07/08 remain NOT RUN. Automation populations overlap and must not be added together.
- **SQA-18:** 2,000 deterministic results; five conservative screenshot-verified readiness bounds 257.9/199.5/197.1/189.7/201.0 ms, median 199.5 ms, maximum 257.9 ms. PASS is against the documented user-proposed local criterion only; formal NFR-PERF-01 approval remains pending. Unit aggregation, HTTP and browser readiness have separate boundaries. Capture/automation overhead, a warmed local host and limited browser/viewport scope remain disclosed.
- **SQA-19:** Eilmar (`EILMAR-P01`) is the human tester; Shirui approved the five interpretations/supporting explanations. Final outcome is 5/5 PASS, observer approved, counted once in the master register. ChatGPT wording assistance, absent original unpolished answers/recording/session screenshot and independently uncorroborated session details remain disclosed. Automated preparation and heuristic review are not human-session evidence.
- **SQA-20:** original narrow categories 16/16 PASS but overall FAIL; stronger post-merge baseline 14 PASS/2 FAIL; corrected automated retest 16/16 PASS. The original FAIL, successful retest and historical 136/136 .NET evidence all remain available. Full WCAG, screen-reader and representative-user accessibility acceptance were not established.
- **Separate HTTP regression:** 28 PASS / 1 FAIL. PM-VALIDATION-01 (malformed numeric expected status accepted as default 200) remains open; passing .NET tests do not erase it. DEF-010 lifecycle enforcement also remains open. The team still owns formal acceptance, RTM coordination and the release decision.
- **Authorship:** Shirui's implementation/evidence contributions are supported by actual Git history. Codex-assisted code, tests, automation and documentation remain identified as such. Eilmar's persistence/SQLite performance-test adaptation and Lana's security/RBAC/reliability work are team contributions. User-supplied earlier browser captures are labelled accordingly; upstream axe files retain Deque/axe-core attribution. The GitHub snapshot preserves Lana's PR #5 comment and Eilmar's PR #7 approval. No separate reviewer approval is invented for PR #13.

Source execution ledgers retain their real dates, environments, software states and limitations. CI links in historical PR descriptions remain historical claims; workflow logs/artifacts were not independently downloaded during consolidation. Incomplete AI prompt templates are not authentic interaction records.

## Manifest, archives and preservation

The [manifest](evidence-manifest.csv) retains **every one of the 1,737 original evidence records plus 31 previous collection records**, with their original IDs, contributor/tester, result, date and source-commit metadata. It adds storage type, a short collected path, ZIP member, canonical evidence ID, original source link and previous copied path. Current container/index/verification records are labelled separately.

- **Direct copy:** open `New path` normally; SHA-256 covers the file's original bytes. Last-write timestamps are preserved.
- **Archive member:** open the ZIP in `New path`, then the short filename in `Archive member`. SHA-256 on the evidence row covers the decompressed original file, not the ZIP. Separate container rows hash each ZIP. Original filenames/locations and recorded last-write times remain in the manifest.
- **Duplicate reference:** an identical payload is already retained at the canonical record's direct path or ZIP member. This is a reference to locally retained identical bytes; the provenance/context row is preserved.
- **Reference-only original evidence:** **none**. Every unique original payload is retained locally. No original QA record was deleted or edited.

There were 625 excess identical copies (21,389,492 bytes) in the original collection. **621 payload copies / 20,895,976 uncompressed bytes are no longer duplicated.** Four duplicate PNG payloads are deliberately kept so all five performance runs remain directly accessible. Exact duplicates are identified by full SHA-256, never by filename alone. Failed attempts, empty outputs and repeated-run metadata remain individually represented.

Archives contain only short flat member names, retain unique raw evidence, and can be extracted without recreating the old deep hierarchy. Saved original documents remain byte-exact: their internal relative links are not rewritten. The [historical link map](links.csv) maps original Markdown links to their source and new direct/archive location. The [manifest](evidence-manifest.csv) also maps all previous copied file paths. Do not run archived scripts in place: their original output paths may overwrite historical evidence.

The historical link audit records 33 pre-existing unresolved relative links in `SQA-20/retest-20261019T095150Z/initial-metrics.md`, an unchanged metrics snapshot. They are identified in the link map; the current index's links resolve. The referenced evidence remains available through the manifest and category indexes above.

## Git path repair and verification

The previous layout had **1,768 physical files / 136,517,321 bytes**, a maximum absolute path of **273 characters**, and **24 paths at or above 260**. Both Git 2.45.1 and bundled Git 2.53.0 reproduced `fatal: adding files failed` / `Filename too long` with long paths disabled and passed with them enabled; [captured probes](08-git/path-probe-before.json) record the actual output. `core.longpaths` was unset. No individual old file exceeded 7,325,954 bytes, so oversized files were not the cause.

The compact layout uses short category names and six raw archives. [Validation](validation.json) checks all historical file/member hashes, ZIP CRC/inventories, original-source links, direct-copy timestamps, all **1,841 pre-existing file hashes/timestamps**, key stage-specific results, the new README links and file coverage. It tests both Git versions with `core.longpaths=false` using temporary indexes. No Git configuration changes are required for this layout; the actual evidence folder is staged only after these checks. No commit or push is authorised yet.

See the [full current tree](tree.txt) and [reorganisation counts](08-git/reorganisation.json). To rerun collection validation from the repository root, use an available Python 3.11+ runtime:

```powershell
python docs/evidence/Shirui/verify.py
```

This rewrites only generated collection metadata. If rerun after staging, restage those updated metadata files before a future approved commit. The retained previous verification script/results are archival records for the superseded layout.
