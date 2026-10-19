# SQA-20 change inventory

No application source, MSTest source, production data, SQA-19 evidence or SQA-19 master-case content was changed. No commit, push, merge or rebase was performed.

## Existing records updated

- `docs/ShiruiFinalTestCases.csv`: only the SQA-20 physical CSV record, with overall automated FAIL and execution-method distinction.
- `docs/ShiruiQualityMetrics-Evidence.md`: actual execution/pass denominators, latest test/coverage evidence, SQA-20 outcome and proposed issue severities.
- `docs/qa-evidence/SQA-20/observation-sheet.csv`: all 16 automated technical executions; original blank sheet preserved in this session's baseline.
- `docs/qa-evidence/SQA-20/README.md`: current automated outcome and links; original manual procedure retained as historical reference.
- `docs/qa-evidence/SQA-20/{screenshots,downloads,observations}/README.md`: links to actual automated evidence, separate from human evidence.

## New executable testing tools

All are under `tools/Sqa20.ManualHost/`:

- `Run-BrowserChecks.cjs`: independent installed Chrome/Edge collection, guarded network policy, actual downloads, scans and category summary; finalize-only mode reuses saved collection.
- `browser-keyboard.cjs`: trusted Tab/Shift+Tab sequences, computed focus evidence, screenshots and native smooth-scroll settling.
- `browser-layout.cjs`: geometry, content, tables, real horizontal wheel input and screenshots.
- `browser-accessibility.cjs`: verified pinned local axe injection and supported WCAG-tag scans.
- `validate_browser_csv.py`: strict UTF-8/CSV/download data validation.
- `Verify-NarrowKeyboard.cjs`: targeted native keyboard investigation of scanner scroller flags.
- `Prepare-EvidenceRecords.py`: derives observation/master records and QA summaries from saved results.
- `Verify-Evidence.py`: artifact/reference/hash/source-scope and credential-pattern checks.
- `axe/{axe.min.js,package.json,LICENSE,dependency.json}`: pinned axe-core 4.10.3, license, source and integrity records.

The existing `Sqa20Host.cs`, host project and start/stop/HTTP-verification scripts were reused. They were not edited in this execution. The temporary local session-pointer file was removed after collection.

## New execution artifacts

All current outputs are in this dated directory. [evidence-index.json](evidence-index.json) lists individual paths, sizes and SHA-256 hashes. It includes real browser screenshots, keyboard events, layout measurements, 24 raw axe scans, four original browser CSV downloads, strict CSV validation, source snapshots/hashes, logs and .NET results. The local `.gitignore` permits the genuine runner log and TRX-referenced original coverage attachment; it does not change application rules.

The initial collection/finalization error and initial narrow timing measurements remain available. Final settled focus evidence is separately identified. Earlier SQA-19/SQA-20 preparation files already existed untracked before this work and were not silently reclassified as new human evidence.

## Review recommendation

A draft PR limited to these QA tools/records is reviewable after the user's commit/push approval. This is not a passing accessibility or release signoff: confirmed label/contrast failures remain open. The current branch also contains earlier untracked preparation files; select the intended QA files deliberately when staging.
