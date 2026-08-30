# GitHub Copilot Evidence Log

The assignment expects evidence of responsible AI-assisted development. For each example, preserve the exact prompt, useful suggestion, modification, rejected suggestion and why human judgement was needed.

## Prompt 1 - requirements quality review
**Paste into Copilot Chat:**

> Review the following requirements for a C# ASP.NET Core Secure API Quality and Vulnerability Management Platform. Check each requirement for clarity, completeness, consistency, feasibility and testability. Do not invent stakeholder requirements or numeric thresholds. Identify ambiguous wording and rewrite only where the intent is already clear. Also propose acceptance criteria in Given-When-Then form. [Paste FR/NFR list from docs/Requirements.md]

Record:
- Useful suggestion:
- Suggestion modified:
- Suggestion rejected:
- Why human judgement was required:

## Prompt 2 - code quality/security review
**Paste into Copilot Chat with `ApiCheckService.cs` and `UrlSafetyService.cs` open:**

> Review this C# API-checking prototype for reliability, security, maintainability and testability. Focus especially on SSRF risk, timeouts, exception handling, duplicate defect creation, unsafe logging, and whether a passive scanner could create false confidence. Do not add destructive scanning, credential attacks, fuzzing, or high-volume requests. Explain each issue and suggest the smallest maintainable fix.

Record the same five evidence items.

## Prompt 3 - MSTest design
**Paste into Copilot Chat with `CheckEvaluator.cs` and `DefectLifecycleService.cs` open:**

> Suggest MSTest cases for these C# classes using Arrange-Act-Assert. Cover positive, negative and boundary cases for expected HTTP status, response-time thresholds, authentication-required endpoints, permissive CORS, defect severity/priority mapping, defect reopening, and passing retest closure. Map every test to a requirement or risk and state any assumption. Avoid tests that only assert NotNull when a stronger observable assertion is possible.

Record the same five evidence items.

## Prompt 4 - test strategy/plan critique
**Paste `docs/TestStrategy.md` and `docs/TestPlan.md`:**

> Critique this initial test strategy and test plan for an ENSE707 C# web prototype. Check whether I have confused test levels, test types, techniques or phases. Identify missing scope boundaries, environment/data assumptions, entry/exit criteria, security testing, regression, defect workflow and residual-risk reporting. Do not claim that a high pass rate or code coverage proves quality.

## Prompt 5 - dashboard/metrics critique

> Review the dashboard metrics in this project. Explain what decision each metric supports, what denominator it uses, and what risk it could hide. Suggest at most three additional metrics that are feasible for a student prototype and do not imply that pass rate, requirement mapping or code coverage proves the absence of defects.

## Human-review checklist
Before accepting any Copilot suggestion:
1. Does it match an approved requirement or clearly labelled assumption?
2. Is it secure and non-destructive?
3. Can the team explain the code and test in a viva/code review?
4. Has it been executed or otherwise validated?
5. Is the added complexity justified for a one-semester student project?
