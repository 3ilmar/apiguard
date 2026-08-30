# Quality Governance

## Process assurance vs product assurance

| Area | Process assurance | Product assurance |
|---|---|---|
| Focus | How the team performs and controls the work | Quality of the prototype/product |
| Project examples | requirement review, meaningful commits, Copilot review, CI, test planning | validation rules, passing tests, defect evidence, working dashboard |
| Evidence | review notes, Git history, CI results, Test Strategy/Plan | test results, RTM, defect log, demonstration |
| Goal | prevent quality problems and make work repeatable | detect/confirm whether the product meets defined expectations |

Both are required. A working prototype without a controlled process can regress quickly; a perfect process with no product evidence cannot show that requirements are actually satisfied.

## Governance rules

| Area | Rule | Evidence |
|---|---|---|
| Requirements | Each implemented feature must have a requirement ID and acceptance basis. | Requirements.md, RTM.md |
| Testing | Each Must requirement must map to at least one test before the mid-project submission. | TestCases.md, RTM.md |
| Code quality | Automated tests should pass before a change is merged. | `dotnet test`, CI |
| GitHub | Commits must be meaningful and describe actual work completed; history must not be falsified or backdated. | Git history |
| AI use | Copilot output must be reviewed, modified where needed, tested and explainable. | CopilotEvidence.md |
| Defects | Defects must contain severity, priority, status and reproducible evidence. | Defect list / export |
| Release/demo | Critical risks must be fixed or explicitly documented before recommending the prototype for demonstration. | Test summary / residual risk |

## Defect severity guide

- Critical: authentication bypass, exposure of protected data, or issue that makes the platform unsafe to demonstrate.
- High: major required workflow or security control fails; no acceptable workaround.
- Medium: important quality issue with limited scope or workaround.
- Low: minor presentation/information issue with low user/business impact.

## Workflow
New -> Triaged -> Assigned/InProgress -> Resolved -> Retest -> Closed. Use Reopened when confirmation testing fails, and Deferred only with a recorded reason/risk owner.
