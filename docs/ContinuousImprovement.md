# Continuous Improvement and Quality Culture

## What worked well
- Breaking the platform into small service classes made quality rules easier to reason about and test.
- Writing test cases and the RTM alongside the prototype exposed missing evidence earlier than waiting until the report was finished.
- Treating security checks as selected passive evidence, rather than claiming automatic compliance, reduced the risk of misleading users.

## What did not work well
- Development activity was compressed late in the assessment period, which reduces the value of historical Git evidence and leaves less time for iterative review and regression testing.

## Root cause of one issue
- Project scheduling and deadline interpretation were not verified early enough across the team, so work was not distributed consistently over the intended weeks.

## Improvement action
- Use a shared weekly definition of done: one small requirement/feature, tests, documentation/trace links and at least two genuine meaningful commits per contributor where the course requires it.

## How we will check the improvement
- Review the Git activity and RTM at a fixed weekly team checkpoint; unresolved blockers and missing test evidence become explicit actions for the next week.

## Quality culture reflection
Quality is a shared responsibility rather than a final testing activity. Requirements review can prevent ambiguity before it becomes code; automated and manual tests provide evidence; defect records support accountable decisions; and regular commits make progress and change history visible. AI assistance can improve speed, but the team remains responsible for validating and explaining the result.

## Agile and DevOps practices for this project

| Practice | Application |
|---|---|
| Sprint planning | Select a bounded set of platform features and QA evidence each week. |
| Short stand-up/checkpoint | Share progress, blockers, defects and test evidence. |
| Definition of Done | Code reviewed; mapped acceptance criteria met; required tests pass; docs/RTM updated. |
| Continuous Integration | GitHub Actions builds/tests on push and pull request. |
| Regression testing | Rerun evaluator/defect tests after changes to scanning or defect workflow. |
| Retrospective | Record one process improvement after each milestone. |
