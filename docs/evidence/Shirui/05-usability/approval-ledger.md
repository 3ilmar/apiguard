# SQA-19 observer finalisation and document validation

Factual QA evidence only; not submitted assessment report prose.

## Authorised outcome

| Field | Current record |
|---|---|
| Session | SQA19-20261019-EILMAR; 19 October 2026; reported 15:00–16:00 NZDT |
| Human tester / observer | EILMAR-P01 / Shirui |
| Scenarios evaluated | Five |
| Accepted interpretations and supporting explanations | Five |
| Final observer decision | 5/5 PASS |
| Overall SQA-19 | PASS — observer approved |
| Approval provenance | Shirui explicitly confirmed in this conversation that she reviewed and approved Eilmar's assessment, including all five outcomes, and authorised recording the decision |
| Signature / precise approval timestamp | Not supplied; not inferred |

The [final observer assessment](../observer-assessment-session-eilmar.md) records the decision separately from the [original human submission](../observation-sheet-session-eilmar.md), which remains unchanged, including historical blank/pending observer fields. ChatGPT assisted wording after tester-reported interpretation; original unpolished answers were not supplied; no session-specific screenshot was available; some unaided-session aspects cannot be independently corroborated. Approval retains these limitations and does not supply missing evidence.

## Source and historical preservation

- Documentation branch `docs/sqa19-final-approval` starts at fetched `origin/main` commit `e43fe52373d3d74b617610e2dbee418ac9b3f9b4`; initial working tree was clean. Local `main` was not changed.
- No application, test, workflow or business-logic changes. No new human session, browser scan, .NET execution or HTTP regression run is claimed for this documentation update.
- [Original proposed assessment snapshot](baseline-observer-assessment.txt), [previous metrics snapshot](baseline-metrics.txt) and [previous register](baseline-register.csv) preserve the earlier state. The `.txt` snapshots retain their original bytes and source-relative link text as historical source records; the current Markdown documents supply navigable links.
- [Baseline evidence hashes](baseline-evidence-sha256.csv) cover the pre-existing SQA-19, SQA-20 and 18 October evidence. Only the current SQA-19 README and observer assessment are intentionally revised among those files. Original human observations, SQA-18 artifacts, all SQA-20 FAIL/retest artifacts and the numeric HTTP failure evidence remain unchanged.
- Git blob preservation is checked independently of checkout line-ending representation. The original import hash remains historical provenance; the validation output records current baseline/working-file hashes without asserting they use identical newline bytes.

## Register-derived totals

The [current register](../../../ShiruiFinalTestCases.csv) and [machine-readable totals](qa-totals.json) contain **22 planned; 16 executed; 16 PASS; 0 provisional PASS; 0 FAIL; 0 BLOCKED; 6 NOT RUN**. Execution rate is **16/22 = 72.73%**; pass rate is **16/16 = 100% of executed master cases**. SQA-19 contributes one master case, not five. No other case outcome changed.

- SQA-18 remains PASS against its proposed performance criterion; formal criterion approval is separate.
- SQA-19 is PASS — observer approved, with limitations above.
- SQA-20 remains PASS ON AUTOMATED RETEST; original FAIL evidence is preserved; full WCAG and human accessibility acceptance are not claimed.
- The separate [HTTP regression](../../SQA-20/retest-20261019T095150Z/postfix-http-verified/summary.json) remains **28 PASS / 1 FAIL**. These master-register totals do not turn that suite into a passing suite or grant release approval.

## Relevant automated validation

Run from the repository root with Python 3:

```powershell
python docs/qa-evidence/SQA-19/final-approval-20261019/validate.py
```

[Actual validation output](validation.json) records strict CSV header/row widths and unique case IDs; register-derived counts; only SQA-19's row changed; five approved scenario rows; retained disclosures; current outcome wording; local SQA-19/current-summary links; original observation and historical evidence hashes; unchanged application source; and Git whitespace validation. Machine execution timestamps in that JSON describe automated validation, not Shirui's approval time.

The [initial document-validation attempt](validation-initial.json) passed 19/20 checks: its two links to `validation.json` were checked before that report had first been created. After writing the report, the repeat completed **20/20 PASS**, including local evidence links across 11 documents. The initial result is retained; it is a report-generation ordering issue, not a product test or missing historical evidence.

Prior [136/136 MSTest and browser retest evidence](../../SQA-20/retest-20261019T095150Z/README.md) is historical product validation, not a fresh run attributed to this documentation branch. CI on this PR is reported separately after submission.

## Remaining application defect and submission inputs

The [short numeric endpoint-validation assessment](../numeric-endpoint-validation-assessment.md) covers root cause, saved reproduction, expected/actual behavior, correction options, regression tests and assignment evidence. It leaves PM-VALIDATION-01 open and application logic untouched. Implement the correction in a separate branch/PR after ownership is agreed.

Before submission, coordinate the six NOT RUN acceptance cases, broader RTM/requirement coverage, formal performance-criterion approval, PM-VALIDATION-01 and DEF-010 ownership, and the team's release decision. SQA-19 observer approval is complete; it does not close these separate items.
