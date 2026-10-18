# ENSE707 Assessment 2 — Codex Project Handoff

## Repository and working rules

- Repository: https://github.com/3ilmar/apiguard
- Work only on the user's branch: `feature/dashboard-reporting`.
- This is the Secure API Quality and Vulnerability Management Platform, built with C#, ASP.NET Core MVC (.NET 8), MSTest and GitHub Actions.
- The user has copied an Assessment 2 patch into their local repository. Inspect `git status`, `git diff`, and the existing project before making changes. Never assume the patch has been committed, compiled, or verified.
- Preserve teammates' changes and keep work in the dashboard, reporting, related test files, and QA evidence unless an integration fix requires a narrowly scoped edit.
- Do not merge into `main`, force-push, or discard the user's uncommitted changes.
- Request approval before committing or pushing to GitHub, and leave a reviewable diff.

## Assigned development tasks

1. Dashboard: open-defect severity distribution and correct no-data handling.
2. Reporting: CSV lifecycle and filter columns, correct escaping and robustness.
3. Dashboard aggregation: test performance with 2,000 results; target under 2 seconds, measured on a documented environment rather than assumed.
4. Tests: MSTest performance case PT-01, CSV structure/escaping tests, dashboard edge cases, usability/error-message tests.
5. Accessibility and usability: validate wording and labels, correct actionable issues found.
6. CI: code-coverage artifact/report if supported by the repository workflow without breaking existing jobs.
7. Evidence: master test result table, automation suite evidence, metrics and release-readiness inputs; clearly distinguish actual runs from planned tests.
8. Report/presentation coverage owned by Shirui: T3 (test execution), T4 (test automation), T7 performance/usability, T8 quality metrics/release decision, individual evidence for T9 and T11, Appendix A final test cases, slides 11–15.

## Source materials and expectations

- Follow the Assessment 2 brief and team's final report allocation (provided separately in the ChatGPT conversation or by the user as files).
- Existing `docs/TestCases.md`, `docs/TestPlan.md`, `docs/Requirements.md`, `docs/RTM.md`, `docs/TestSummaryReport.md`, and `README.md` provide initial requirements and test strategy.
- Project requirements include at least 15 final cases with preconditions, steps, expected results, actual results, statuses and evidence.
- Record planned, executed, passed, failed, blocked, and not-run totals; do not assign 'pass' to an unexecuted case.
- Record software commit SHA, .NET SDK, OS, test data, tool versions, CI run links, execution dates/times, and measured performance.
- Known early limitations include in-memory state and incomplete production-grade security assurances. Verify current state; do not assume earlier limitations have already been fixed.
- Coordinate defect ownership, security testing, persistence, RBAC, CI gates and RTM work with Eilmar/Lana instead of overwriting their work.

## Execution checklist

1. Inspect branch and uncommitted local changes; ensure they are on `feature/dashboard-reporting`.
2. Review pasted changes for compilation errors, unintended regressions, and conflicts.
3. Run `dotnet --info`, `dotnet restore SecureApiQualityPlatform.sln`, `dotnet build SecureApiQualityPlatform.sln`, and `dotnet test SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj --collect "XPlat Code Coverage"` when SDK/tooling permits.
4. Fix actual compile/test failures and re-run; identify unresolved issues explicitly.
5. Exercise a safe, authorised API test target for integration/system tests; never perform invasive scans against unapproved public services.
6. Measure PT-01 using 2,000 seeded results under documented conditions. Distinguish unit-level aggregation timing from browser page-render time.
7. Update test evidence files using observed results, not invented numbers, screenshots, prompts, decisions, or defects.
8. Report code changes, tests executed, actual pass/fail outcomes, risks, and items requiring manual verification.

## Academic integrity

The Canvas instruction for ENSE707 Assessment 2 prohibits AI tools from writing submitted report prose. You may implement code, generate tests and factual evidence, identify issues, explain results, and provide an outline/checklist. Do not write the final student-authored report sections on the student's behalf. Make Copilot/AI evidence authentic; never fabricate prompts, output, test results, dates, commits, or human decisions.

## First Codex task

Review the entire working-tree diff before editing. Identify the user's changed files and evaluate compilation/test readiness, then fix issues in scope. Run the build and automated tests, report actual evidence, and produce a concise checklist of outstanding manual tests and report inputs. Stop before commit/push until the user approves.
