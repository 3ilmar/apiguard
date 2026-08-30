# Initial Requirements and Acceptance Criteria

## Stakeholders

- Developers: register APIs, inspect failures, fix defects, and retest.
- Testers: define endpoint expectations, run checks, review evidence, and perform regression testing.
- Security analysts: review security-related failures and prioritise vulnerabilities.
- Project managers: view quality status, risk, defects, and progress.

## Functional requirements

| ID | Requirement | Priority |
|---|---|---|
| FR-01 | The system shall allow a user to register an API with a name and valid public HTTP(S) base URL. | Must |
| FR-02 | The system shall allow a user to register one or more endpoints with method, path, expected status code and response-time threshold. | Must |
| FR-03 | The system shall execute configured, non-destructive quality/security checks against a registered endpoint and record each result. | Must |
| FR-04 | The system shall create or update a defect when an executed check fails. | Must |
| FR-05 | The system shall allow a defect to be assigned, prioritised and moved through defined workflow states. | Must |
| FR-06 | The system shall support retesting and close a resolved/retest defect when the associated check passes. | Must |
| FR-07 | The system shall display dashboard metrics for test execution, pass rate, response time and open high/critical defects. | Must |
| FR-08 | The system shall export check and defect evidence as CSV files. | Should |

## Non-functional / quality requirements

The numeric targets below are **prototype engineering targets** and must be validated with the lecturer/stakeholders before being treated as final acceptance thresholds.

| ID | Attribute | Requirement |
|---|---|---|
| NFR-SEC-01 | Security | By default, the platform shall reject localhost, loopback, link-local and private-network API targets before sending a request. |
| NFR-SEC-02 | Security | The prototype shall not store API passwords, bearer tokens or other secrets. |
| NFR-REL-01 | Reliability | Failure of one endpoint request shall be recorded as evidence and shall not terminate the application or prevent later endpoints from being tested. |
| NFR-PERF-01 | Performance | For a prototype data set of up to 2,000 recorded check results, the dashboard target is to render within 2 seconds on the agreed test machine. **Provisional/TBC.** |
| NFR-MAINT-01 | Maintainability | Check-evaluation rules shall be separated from controllers and be independently unit-testable. |
| NFR-USA-01 | Usability | Every failed check shall display a plain-language reason sufficient for a tester to understand the observed mismatch. |
| NFR-COMP-01 | Compatibility | The web interface target is current desktop versions of Chrome and Edge. **Provisional/TBC.** |

## Examples of requirement-quality improvement

Weak: "The scanner should check APIs quickly and securely."

Problems: "quickly" and "securely" are vague; multiple obligations are bundled; operating conditions and evidence are missing.

Improved (performance): "For an endpoint with a configured response-time threshold, the system shall record a pass when the measured HTTP response time is less than or equal to that threshold and a fail when it exceeds the threshold."

Improved (security): "Before sending an API request, the system shall reject a target that resolves to loopback, link-local or RFC1918 private IPv4 space unless an explicit test-environment configuration enables private targets."

## Acceptance criteria (Given-When-Then)

### AC-01 Register API
GIVEN a user enters a valid public HTTPS base URL
WHEN the API registration form is submitted
THEN the API is stored and displayed in the registered API list.

GIVEN a user enters localhost or a private-network target while private targets are disabled
WHEN the form is submitted
THEN the system rejects the registration and explains the safety reason.

### AC-02 Run endpoint checks
GIVEN an endpoint is configured with expected HTTP 200 and a 1,000 ms threshold
WHEN the check run completes
THEN the platform records availability, expected-status and response-time results with observable pass/fail status and evidence.

### AC-03 Authentication protection
GIVEN an endpoint is marked as requiring authentication
WHEN an unauthenticated request receives HTTP 200
THEN the authentication-protection check fails and a Critical defect is recorded or reopened.

### AC-04 Defect workflow and retest
GIVEN a failed check has an open defect marked Resolved or Retest
WHEN the same check later passes
THEN the defect is marked Closed and the retest time is recorded.

### AC-05 Dashboard
GIVEN check results and defects exist
WHEN the dashboard is opened
THEN it shows executed checks, pass rate, average recorded response time, open defect count and open high/critical defect count.
