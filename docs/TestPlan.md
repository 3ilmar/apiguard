# Initial Test Plan - Mid-Project Prototype

## Feature under test
Secure API registration, endpoint check execution, defect lifecycle, dashboard and CSV evidence export.

## Test objective
Demonstrate that the prototype's critical workflows conform to the approved initial requirements and produce reproducible quality evidence suitable for a mid-project demonstration.

## Requirements to be tested
FR-01 to FR-08, NFR-SEC-01, NFR-REL-01, NFR-MAINT-01, NFR-USA-01; provisional NFR-PERF-01 where environment permits.

## Test items
- RegisteredApi and ApiEndpoint validation.
- UrlSafetyService.
- CheckEvaluator and ApiCheckService.
- DefectLifecycleService.
- DashboardService and dashboard view.
- CSV report endpoints.

## Test approach
1. Run unit tests first for fast feedback.
2. Execute manual/system smoke workflow on localhost.
3. Run checks only against an authorised test API.
4. Verify failed checks create/reopen defects.
5. Mark a defect Resolved/Retest, rerun the associated check and verify closure if it passes.
6. Export evidence and inspect the CSV.
7. Record failures as defects with reproducible steps and links to requirement/test case.

## Test data
- Valid public HTTPS API URL.
- Invalid URL text.
- `http://localhost` and a private IPv4 target for safety rejection.
- Endpoint expected status 200.
- Authentication-required endpoint that returns 401/403 when unauthenticated and a negative example that returns 200.
- Response-time threshold values including exactly on the boundary.

## Responsibilities
- Developer: implement and unit-test services; diagnose defects.
- Tester/QA role: execute system/manual cases; maintain evidence and RTM.
- Security analyst role: review security failures and residual risk.
- Project manager role: review dashboard, priority and release/demo readiness.

## Schedule
Because the project is under significant time constraint, execute in risk order: build/start -> unit tests -> registration/safety -> scanning -> defect workflow/retest -> dashboard/export -> exploratory/usability.

## Pass/fail criteria
Pass when actual observable behaviour matches the expected result under the recorded conditions. Fail when it differs. Blocked when environment/dependency prevents execution. Do not convert a blocked test into a pass.

## Risks
- No .NET runtime on one machine: use another team machine/CI for execution evidence.
- Public test service changes behaviour: record timestamp and use an owned local/public test API where possible.
- In-memory storage resets on restart: treat persistence as a known limitation, not hidden behaviour.
