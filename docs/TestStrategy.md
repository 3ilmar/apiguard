# Test Strategy

## 1. Purpose
Provide risk-based evidence that the initial Secure API Quality and Vulnerability Management Platform performs its core workflows correctly and that security, reliability, performance, maintainability and usability risks are made visible.

## 2. Scope of testing
- API registration and endpoint configuration.
- URL safety validation.
- Automated endpoint checks and result recording.
- Defect creation, prioritisation, assignment, status workflow and retest closure.
- Dashboard metric calculations.
- CSV export.
- Selected non-functional checks: security, reliability, response-time evaluation and usability of failure messages.

## 3. Out of scope
- Full OWASP API Security Top 10 penetration testing.
- Destructive fuzzing or exploitation.
- High-volume denial-of-service/rate-limit testing.
- Production credential storage.
- Full Identity/RBAC implementation.
- Persistent database recovery and backup.
- Mobile-browser optimisation.

## 4. Test levels
- Unit: pure check evaluation, defect lifecycle and dashboard calculations.
- Integration: controller/service/store interactions and HTTP client behaviour in a controlled environment.
- System: end-to-end browser workflow from API registration to scan, defect and dashboard.
- Acceptance/UAT: representative developer/tester validates that the workflow and messages are understandable and useful.

## 5. Test types
- Functional testing of all Must requirements.
- Security-focused testing for unsafe target validation, authentication-required endpoint behaviour, CORS and transport checks.
- Reliability testing of network failure and timeout behaviour.
- Performance testing against configured endpoint thresholds plus a provisional dashboard target.
- Usability testing of navigation, forms, failure messages and defect workflow.
- Regression testing after any fix to check evaluators or defect lifecycle logic.

## 6. Test techniques
- Equivalence partitioning: valid public URL vs malformed/private/localhost URL.
- Boundary-value analysis: response time at threshold, just below and just above.
- Decision-table thinking: authentication-required flag x returned status.
- State-transition testing: New -> Assigned/InProgress -> Resolved -> Retest -> Closed; failure after resolution -> Reopened.
- Exploratory testing: malformed paths, repeated scan runs, duplicate failures, navigation and export.

## 7. Test environment
- Development: Windows 11 or equivalent development machine, .NET 8 SDK, HTTPS localhost.
- Browser targets: current Chrome and Edge (provisional until stakeholder confirmation).
- Automated tests: MSTest via `dotnet test`.
- CI: GitHub Actions on push and pull request.
- External API: only an API owned by the team or explicitly authorised test service.

## 8. Tools
- Visual Studio / VS Code.
- MSTest and .NET CLI.
- GitHub and GitHub Actions.
- Browser developer tools; optional Postman for independent API checks.
- GitHub Copilot for assisted coding/test design with documented human review.

## 9. Defect management approach
Failed checks may create defects containing check type, evidence, severity, priority and status. Severity represents impact; priority represents urgency. Workflow: New -> Triaged -> Assigned/InProgress -> Resolved -> Retest -> Closed, with Reopened/Deferred where justified. Closure requires verification evidence.

## 10. Entry criteria
- A build is available and starts successfully.
- Required API/endpoint configuration exists.
- Test target is authorised and reachable for relevant tests.
- Test data and expected results are defined.
- Blocking build/configuration errors are resolved.

## 11. Exit criteria
- All high-risk planned tests executed.
- All Must functional requirements have at least one mapped test.
- No unresolved Critical defects for the prototype demonstration.
- Critical regression tests pass after fixes.
- Remaining limitations and unexecuted tests are documented as residual risk.

## 12. Risks and mitigation
- Time pressure: prioritise security, core workflow and traceability over low-value styling.
- External API instability: use a controlled authorised test API and record environment/time.
- False confidence from passive checks: label the scanner as selected checks, not compliance certification.
- SSRF risk: block private/local targets by default.
- AI-generated defects/code quality issues: review, test, refactor and document accepted/rejected Copilot suggestions.
