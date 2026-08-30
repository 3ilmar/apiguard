# Initial Test Cases

| ID | Requirement | Level / Type | Preconditions | Steps / Data | Expected Result | Priority |
|---|---|---|---|---|---|---|
| TC-01 | FR-01 | System / Functional | App running | Register a valid authorised public HTTPS API | API is saved and shown in the API list | High |
| TC-02 | FR-01, NFR-SEC-01 | System / Security | Private targets disabled | Attempt to register `http://localhost:5000` | Registration rejected with SSRF-safety explanation | High |
| TC-03 | FR-02 | System / Functional | API exists | Add GET `/health`, expected 200, threshold 1000 ms | Endpoint appears with saved expectations | High |
| TC-04 | FR-03, FR-04 | Integration/System / Functional | Endpoint returns a status different from configured expectation | Run checks | Expected-status result fails and defect is created with evidence | High |
| TC-05 | FR-03, FR-04 | Integration/System / Security | Endpoint marked RequiresAuthentication but unauthenticated call returns 200 | Run checks | Authentication-protection fails; Critical/Urgent defect created or reopened | High |
| TC-06 | FR-03 | Unit / Performance | Threshold = 1000 ms | Evaluate 1000 ms then 1001 ms | 1000 passes; 1001 fails | High |
| TC-07 | FR-03, NFR-REL-01 | Integration / Reliability | Target times out or is unreachable | Run checks | Availability fail is recorded; application remains usable | High |
| TC-08 | FR-05 | System / Functional | Open defect exists | Assign user, set priority and move status | Updated values persist in current process and appear in defect list | Medium |
| TC-09 | FR-06 | Unit/System / Regression | Existing defect status = Resolved or Retest | Rerun same check and make it pass | Defect becomes Closed and retest timestamp is recorded | High |
| TC-10 | FR-07 | Unit/System / Functional | Two results: one pass, one fail | Open dashboard | Executed=2 and pass rate=50%; defect metrics reflect store state | Medium |
| TC-11 | FR-08 | System / Functional | Results/defects exist | Export results and defects CSV | Browser downloads CSV with headings and recorded evidence | Medium |
| TC-12 | NFR-USA-01 | Acceptance / Usability | Representative tester | Review at least five failed-check messages | Tester can state what failed and what evidence was observed without inspecting source code | Medium |

## Automated test mapping
`SecureApiQualityPlatform.Tests` currently provides automated MSTest coverage for expected-status evaluation, response-time boundary handling, authentication-protection logic, CORS evaluation, content-type matching, defect creation/priority mapping, retest closure and dashboard pass-rate calculation.
