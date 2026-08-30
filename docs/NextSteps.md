# Project Progress, Risks and Next Steps

## Work completed in this prototype package
- Defined scoped stakeholder problem and quality attributes.
- Created functional/non-functional requirements, quality review examples and acceptance criteria.
- Implemented C# ASP.NET Core MVC prototype structure.
- Implemented selected passive API checks and SSRF-oriented target validation.
- Implemented defect lifecycle, dashboard metrics and CSV export.
- Added MSTest project and CI workflow.
- Added Test Strategy, Test Plan, 12 initial test cases, RTM, governance, Copilot evidence template and test summary template.

## Current limitations and risks
- In-memory data store; no persistent database.
- No production login/RBAC.
- No stored API credentials, therefore authenticated positive-path calls are not yet supported.
- Rate-limit behaviour is not actively stress-tested because high-volume requests would be inappropriate for a safe student prototype without a controlled environment.
- No claim of comprehensive OWASP coverage.
- Historical Git activity cannot be reconstructed honestly if regular commits were not made earlier.

## Recommended next phase
1. Run/repair the project on a .NET 8 machine and capture real screenshots/test output.
2. Replace any compile/runtime issue before adding scope.
3. Execute TC-01 to TC-12 and update TestSummaryReport.md, RTM statuses and DefectLog.md.
4. Ask Copilot the evidence prompts and record accepted/modified/rejected suggestions.
5. Add persistent SQLite/EF Core storage.
6. Add authentication and role-based access (Developer, Tester, Security Analyst, Project Manager).
7. Add authorised-token handling through secure configuration rather than plaintext database fields.
8. Expand selected security checks only in a controlled test environment.
9. Add Playwright system tests for the critical registration -> scan -> defect -> retest workflow.
10. Update final metrics and residual-risk statement before the final project.
