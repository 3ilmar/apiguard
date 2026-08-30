# ENSE707 Assessment 1 - Mid-Project Report
## Secure API Quality and Vulnerability Management Platform

**Group members:** [Member 1 - ID], [Member 2 - ID], [Member 3 - ID]  
**Repository:** [insert GitHub repository link]  
**Lecturer approval evidence:** [insert date / screenshot / email / Canvas evidence]  
**Prototype technology:** C#, ASP.NET Core MVC (.NET 8), MSTest, GitHub Actions  

> **Important before submission:** replace all square-bracket placeholders, run the prototype and tests on a .NET 8 machine, update the Test Summary with real evidence, and insert screenshots. Do not claim tests passed unless they were actually executed.

## 1. Problem Definition and Approval

Modern software teams rely heavily on APIs to connect web applications, mobile applications, cloud services and internal systems. As the number of APIs grows, developers and small organisations can struggle to maintain a clear view of whether endpoints are available, return the correct responses, remain within expected performance limits, and follow basic security expectations. Quality problems may be discovered only after an integration fails or a security issue has already reached a later stage of development. This creates rework and makes release decisions depend on assumptions rather than visible evidence.

Our project addresses this problem through a **Secure API Quality and Vulnerability Management Platform**. The platform allows a user to register an API and its endpoints, define expected behaviour, run selected automated checks, record failures as defects, assign and prioritise defects, retest after a fix, and view a dashboard of quality evidence. This directly matches the approved project topic, which identifies authentication, authorisation, invalid input, rate limiting, response time, error handling and availability as relevant concerns, with security checks informed by the OWASP API Security Top 10. The primary users are developers, testers, security analysts and project managers.

The practical motivation is not to build another general-purpose API client. The quality problem is the lack of an integrated, repeatable workflow from **expectation -> test evidence -> defect -> fix -> retest -> status**. A developer needs fast feedback about failed expectations; a tester needs traceable and reproducible test evidence; a security analyst needs security-related failures to be visible and prioritised; and a project manager needs a concise view of pass rate, response times, open vulnerabilities and residual risk. The ENSE707 material emphasises that software quality is broader than bug counting and includes meeting requirements, satisfying users, supporting business value and managing risks. The project therefore focuses on quality management as well as checking endpoint behaviour.

The key quality concerns are **security, reliability, performance efficiency, maintainability and usability**. Security is the highest product risk because the platform itself makes outbound requests and could become unsafe if arbitrary internal targets were allowed. Reliability matters because network failure should be recorded as evidence rather than crash the application. Performance matters both as an API quality measure and for the dashboard. Maintainability matters because new checking rules should be added without rewriting controllers or unrelated code. Usability matters because failed checks and defects must explain what occurred clearly enough for a user to take action.

This is suitable for a Software Quality Assurance project because it supports both **process assurance** and **product assurance**. Process assurance is demonstrated through requirement IDs, acceptance criteria, test planning, traceability, meaningful Git history, CI, defect workflow and documented AI review. Product assurance is demonstrated through the working prototype, executed tests, recorded results, defects and dashboard evidence. This distinction follows the Week 3 laboratory approach: good quality requires controlled work as well as evidence that the software behaves as expected.

The project is achievable within one semester because the scope can be developed incrementally. The mid-project prototype deliberately implements a bounded subset: registration, endpoint expectations, selected non-destructive checks, defect lifecycle, dashboard and CSV export. Persistent storage, production authentication/RBAC, secure credential handling and deeper controlled security testing are deferred to the next phase. This keeps the current prototype demonstrable while leaving clear extension work for the final project.

**Approval:** [Insert evidence that the lecturer approved the topic before detailed implementation. If approval was verbal, state the date/context and include whatever supporting evidence is available.]

## 2. Requirements and Quality Analysis

### 2.1 Stakeholders and initial requirements

The four primary stakeholder groups have different quality needs. Developers need fast feedback and actionable defect descriptions. Testers need observable expected results, repeatable execution and trace links. Security analysts need high-risk failures to be differentiated from normal functional failures. Project managers need metrics that support decisions rather than numbers without context.

The initial functional requirements are:

- **FR-01:** The system shall allow a user to register an API with a name and valid public HTTP(S) base URL.
- **FR-02:** The system shall allow a user to register one or more endpoints with method, path, expected status code and response-time threshold.
- **FR-03:** The system shall execute configured, non-destructive quality/security checks against a registered endpoint and record each result.
- **FR-04:** The system shall create or update a defect when an executed check fails.
- **FR-05:** The system shall allow a defect to be assigned, prioritised and moved through defined workflow states.
- **FR-06:** The system shall support retesting and close a resolved/retest defect when the associated check passes.
- **FR-07:** The system shall display dashboard metrics for test execution, pass rate, response time and open high/critical defects.
- **FR-08:** The system shall export check and defect evidence as CSV files.

Initial non-functional requirements include:

- **NFR-SEC-01:** By default, the platform shall reject localhost, loopback, link-local and private-network API targets before sending a request.
- **NFR-SEC-02:** The prototype shall not store API passwords, bearer tokens or other secrets.
- **NFR-REL-01:** Failure of one endpoint request shall be recorded as evidence and shall not terminate the application or prevent later endpoints from being tested.
- **NFR-MAINT-01:** Check-evaluation rules shall be separated from controllers and be independently unit-testable.
- **NFR-USA-01:** Every failed check shall display a plain-language reason sufficient for a tester to understand the observed mismatch.
- **NFR-PERF-01:** For up to 2,000 recorded check results, the dashboard engineering target is to render within two seconds on the agreed test machine. This target is **provisional/TBC** until stakeholder validation.

### 2.2 Requirements quality review

Week 4 connects quality attributes to requirements, acceptance criteria, tests and evidence. A requirement that is ambiguous can still produce a passing test, but the test may only prove that one interpretation was implemented. We therefore reviewed requirements for **clarity, completeness, consistency, correctness, feasibility and testability** rather than simply adding the word “shall”.

A weak statement such as *“The scanner should check APIs quickly and securely”* fails several checks. “Quickly” and “securely” are undefined, the statement bundles different obligations, the operating conditions are missing, and two reviewers could reach different pass/fail conclusions. It is also unclear whether “should” is mandatory.

The performance part was rewritten as: **For an endpoint with a configured response-time threshold, the system shall record a pass when the measured HTTP response time is less than or equal to the threshold and a fail when it exceeds the threshold.** This is clearer because the input, measurement and boundary are observable. The security part was separated as: **Before sending an API request, the system shall reject a target that resolves to loopback, link-local or private IPv4 address space unless an explicit test-environment configuration enables private targets.** This makes the safety behaviour independently testable.

The set was also checked for consistency. For example, FR-03 allows the platform to send requests while NFR-SEC-01 restricts where requests can be sent. These requirements are not contradictory because the security requirement establishes a precondition for execution. NFR-SEC-02 also affects scope: the current prototype cannot perform a positive authenticated request using stored production credentials, so authentication checking is limited to verifying that an endpoint marked as protected rejects an unauthenticated request. That limitation is documented rather than hidden.

Correctness requires stakeholder validation, not only good wording. A requirement can be precise and still be the wrong requirement. Therefore the exact performance target, browser compatibility target and future role permissions are marked provisional until confirmed. This follows the Week 4 instruction not to invent numeric thresholds when stakeholders have not decided them.

### 2.3 Acceptance criteria

Acceptance criteria make the key behaviours observable. Examples include:

**AC-01 - Register API:** GIVEN a user enters a valid public HTTPS base URL, WHEN the registration form is submitted, THEN the API is stored and shown in the registered list. GIVEN a user enters localhost/private-network target while private targets are disabled, WHEN the form is submitted, THEN registration is rejected with a safety explanation.

**AC-02 - Run checks:** GIVEN an endpoint is configured with expected HTTP 200 and a response-time threshold, WHEN the check run completes, THEN availability, expected-status and response-time evidence is recorded with pass/fail status and an explanatory message.

**AC-03 - Authentication protection:** GIVEN an endpoint is marked as requiring authentication, WHEN an unauthenticated request returns HTTP 200, THEN the authentication-protection check fails and a Critical defect is created or reopened.

**AC-04 - Retest:** GIVEN a failed check has a defect marked Resolved or Retest, WHEN the same check later passes, THEN the defect is marked Closed and a retest time is recorded.

**AC-05 - Dashboard:** GIVEN check results and defects exist, WHEN the dashboard opens, THEN it shows executed checks, pass rate, average recorded response time, open defect count and open high/critical defect count.

These criteria include positive, negative and boundary behaviour rather than only the happy path. They also provide the basis for test cases and traceability.

## 3. Proposed Solution and Initial Prototype

The proposed solution is a C# ASP.NET Core MVC web application. A web GUI is appropriate because the assessment topic includes data entry, workflow processing, dashboards, reporting and end-user decision support; the assignment brief explicitly expects a GUI when these interactions are present.

The prototype is structured around small model, service and controller responsibilities. `RegisteredApi` and `ApiEndpoint` hold the registered test target and expectations. `UrlSafetyService` validates outbound targets and blocks local/private address ranges by default. `ApiCheckService` executes requests using `HttpClient` with a timeout and delegates pass/fail rules to `CheckEvaluator`. This separation supports maintainability and unit testing. `DefectLifecycleService` converts failed checks into defects and handles retest closure/reopening. `DashboardService` calculates dashboard metrics. `ReportsController` exports check and defect evidence as CSV.

The selected checks are intentionally non-destructive: availability, expected HTTP status, configured response-time threshold, expected content type, HTTPS transport, unauthenticated access to endpoints marked protected, permissive wildcard CORS in the protected-endpoint context, and server-header disclosure. The platform does **not** claim to automatically prove compliance with the OWASP API Security Top 10. OWASP’s 2023 API list includes broken object-level authorisation, broken authentication, unrestricted resource consumption, SSRF and security misconfiguration among other risks. Some of those require application-specific identities, data and controlled attack scenarios that would be inappropriate to infer from one passive request. The prototype therefore treats OWASP as a risk basis for selected checks and future test design, not as a certification label.

An important design trade-off is security versus convenience. Allowing users to enter any URL would make local development easier, but it creates SSRF risk because the platform is a server that sends requests on behalf of a user. The prototype therefore blocks local/private targets by default and allows the restriction to be changed only through configuration for an approved test environment. This reduces convenience but is justified by the project’s security objective.

The current storage is in-memory. This was chosen to keep the mid-project implementation small and demonstrable within the available time. The limitation is explicit: data resets when the application restarts and persistence/recovery are not yet proven. The next phase will replace it with a database after the core workflow has been validated.

## 4. AI-Assisted Development Using GitHub Copilot

GitHub Copilot is used as an assistant for code review, test design, requirement review and QA documentation, but the team remains responsible for every accepted output. This is important because the course material warns that AI suggestions may invent requirements, miss edge cases, suggest insecure patterns, create unnecessary complexity or generate tests that pass without evaluating meaningful behaviour.

The report must include actual Copilot evidence. We will capture the exact prompt and response for at least three examples and record: (1) a useful suggestion, (2) a suggestion we modified, (3) a suggestion we rejected, and (4) why human judgement was required. The repository includes `docs/CopilotEvidence.md` with prepared prompts. Examples include asking Copilot to review the requirements for clarity/testability without inventing thresholds; review `ApiCheckService` and `UrlSafetyService` for SSRF, timeout and maintainability issues; and propose MSTest cases covering boundary and defect-state behaviour.

One important human-review rule is that Copilot is not allowed to silently expand the product scope. For example, if it suggests storing API bearer tokens in a normal database column for convenience, that suggestion should be rejected because the current requirement explicitly avoids secret storage until a secure credential-handling design exists. If Copilot proposes high-volume rate-limit testing against arbitrary registered APIs, it should also be rejected because it creates safety and authorisation risk. Conversely, a suggestion to separate evaluation rules into a small testable class is useful because it supports NFR-MAINT-01 and makes automated evidence easier to obtain.

The team will also verify AI-generated tests against the requirement rather than judging them by whether they are green. A test with only `Assert.IsNotNull` may execute code but provide weak evidence; where a specific status, state, severity or percentage is expected, the assertion should verify that observable outcome. This reflects the course emphasis that coverage and pass rate are not proof of quality.

**Insert actual Copilot evidence/screenshots here before submission.**

## 5. Initial Test Strategy and Test Planning

The test strategy is risk-based. Testing provides evidence and reduces uncertainty; it does not prove perfection. Security and the end-to-end defect workflow receive the highest priority because failure in those areas would undermine the purpose of the platform. Lower-risk visual styling is tested after core behaviour.

The test levels are kept distinct from test types. **Unit testing** checks small rules such as status comparison, response-time boundaries, severity/priority mapping and dashboard calculations. **Integration testing** checks connected parts such as HTTP execution, service/store behaviour and later the database boundary. **System testing** checks the complete workflow through the deployed web application. **Acceptance/UAT** asks representative users whether the workflow and messages are understandable and useful. Test types then describe the quality focus: functional, security, reliability, performance and usability.

Black-box techniques include equivalence partitioning for valid versus invalid/private URLs, boundary-value analysis for response-time thresholds, decision-table thinking for the combination of `RequiresAuthentication` and returned HTTP status, and state-transition testing for defect workflow. White-box/code-coverage evidence may be collected with `dotnet test --collect "XPlat Code Coverage"`, but code coverage will not be reported as the percentage of defects found. Exploratory testing will target repeated scan runs, malformed paths, duplicate failures and navigation because scripted tests can miss unexpected interactions.

The initial test environment is a team development machine with .NET 8 SDK and HTTPS localhost. Current Chrome and Edge are provisional browser targets. MSTest and `dotnet test` provide automated execution; GitHub Actions runs build/test on push and pull requests. API tests must target only an API owned by the team or an explicitly authorised test service. Because external service behaviour can change, the environment, timestamp and target must be recorded with evidence.

Entry criteria are: a valid build is available; the application starts; required endpoint configuration exists; the test target is authorised; test data/expected results are defined; and blocking configuration problems are resolved. Exit criteria for the mid-project demonstration are: all high-risk planned tests executed, each Must requirement mapped to at least one test, no unresolved Critical prototype defect, critical regression tests passing after fixes, and residual risks documented. These criteria indicate sufficient evidence for the context; they do not claim the software is defect-free.

Defect management follows the Week 6 distinction between severity and priority. Severity represents impact while priority represents urgency. The workflow is **New -> Triaged -> Assigned/In Progress -> Resolved -> Retest -> Closed**, with Reopened and Deferred branches when justified. A developer marking a defect Resolved is not the same as QA verification; closure requires retest evidence. A useful defect report records environment, precondition, steps, expected result, actual result and links to the related test/requirement.

## 6. Initial Test Cases and Requirements Traceability

At least eight test cases are required; the project defines twelve so the core functional and non-functional risks are covered. High-priority cases include valid API registration (TC-01), private/localhost target rejection (TC-02), endpoint configuration (TC-03), expected-status failure and defect creation (TC-04), unauthenticated access to a protected endpoint (TC-05), response-time boundary behaviour (TC-06), timeout/unreachable endpoint reliability (TC-07), defect assignment/status updates (TC-08), retest closure (TC-09), dashboard calculation (TC-10), CSV export (TC-11) and message usability (TC-12).

The automated MSTest project currently contains tests for expected-status pass/fail, response-time at and above the threshold, protected-endpoint 401/403 behaviour, a protected endpoint incorrectly returning 200, wildcard CORS, content-type matching, defect creation/priority mapping, passing-retest closure and dashboard pass-rate calculation. These tests use the Arrange-Act-Assert pattern and specific observable assertions.

The Requirements Traceability Matrix links each requirement to acceptance criteria, tests and prototype components. For example, FR-03 maps to AC-02/AC-03, TC-04 to TC-07, and the `ApiCheckService`/`CheckEvaluator`. FR-06 maps to AC-04, TC-09 and `DefectLifecycleService`. NFR-SEC-01 maps to AC-01, TC-02 and `UrlSafetyService`. This allows the team to see which requirements have evidence, which are unexecuted or blocked, and what needs regression testing if a requirement changes.

Traceability is useful for change management because it supports both forward and backward reasoning. Forward traceability asks whether an approved requirement was implemented and evaluated. Backward traceability asks why a test or implementation exists and helps detect accidental scope expansion. However, 100% requirement-to-test mapping does not mean 100% quality: the requirement may be wrong, the test may be weak, or the test may not have run. For that reason, RTM status will distinguish implemented, executed, passed, failed, blocked and planned evidence rather than merely recording a link.

The dashboard also follows this evidence principle. Pass rate is calculated as passed checks divided by executed checks, not planned checks. It is displayed together with open high/critical defects and response-time information. A high pass rate cannot override a serious unresolved security failure. Before the final project, additional metrics may include requirement execution coverage and defect status/age, but only if the team can explain what decision each metric supports.

## 7. Project Progress, Risks and Next Steps

The mid-project package now contains the initial requirements, acceptance criteria, prototype source structure, selected security/quality checks, defect lifecycle, dashboard, CSV export, MSTest suite, GitHub Actions workflow, Test Strategy, Test Plan, twelve initial test cases, RTM, quality governance, Copilot evidence template and Test Summary template.

The main technical limitations are in-memory storage, no production authentication/RBAC, no secure credential store, no authenticated positive-path requests, no controlled rate-limit/load test, and no claim of complete OWASP API Security Top 10 coverage. The main project-process risk is compressed development time and weaker historical Git evidence. The assignment requires at least two meaningful commits per student per week and expects continuous development rather than work concentrated immediately before the deadline. We will not fabricate or backdate history; the remaining mitigation is to make every current contribution genuine, clearly described and traceable, and then return to regular weekly development for the final phase.

Immediate next steps are to run the code on a .NET 8 machine, correct any compile/runtime issue, execute TC-01 to TC-12, capture screenshots and test output, update the RTM and Test Summary with real statuses, record real defects, and complete the Copilot evidence log. After the mid-project submission, the highest-value product extensions are persistent SQLite/EF Core storage, authentication and role-based access for Developer/Tester/Security Analyst/Project Manager, secure secret handling, Playwright system tests for the critical workflow, and deeper security testing only in a controlled authorised environment.

The team reflection is that quality evidence should be created alongside implementation rather than assembled at the end. Requirement review improves testability before code exists, automated tests support fast regression feedback, defect records make failures accountable, and traceability makes change impact visible. AI can accelerate each activity, but quality still depends on human review, execution evidence and the ability to explain decisions during the demonstration/viva.

## Contribution Statement

**[Member 1]:** [Describe specific requirements, code, tests, documentation, review and commits.]  
**[Member 2]:** [Describe specific requirements, code, tests, documentation, review and commits.]  
**[Member 3]:** [Describe specific requirements, code, tests, documentation, review and commits.]  

All members reviewed the final report and are responsible for understanding the submitted requirements, prototype, tests, defects, AI-assisted outputs and technical decisions.

## References

Auckland University of Technology. (2026). *ENSE707 Week 3 lab: Process assurance and quality governance* [Course material].

Auckland University of Technology. (2026). *ENSE707 Week 4: Requirements quality and testability* [Course material].

Auckland University of Technology. (2026). *ENSE707 Week 5: Software testing fundamentals* [Course material].

Auckland University of Technology. (2026). *ENSE707 Week 6: Test management* [Course material].

Auckland University of Technology. (2026). *Software quality attributes and models* [ENSE707 course material].

International Organization for Standardization. (2023). *ISO/IEC 25010:2023 Systems and software engineering - Systems and software Quality Requirements and Evaluation (SQuaRE) - Product quality model*. https://www.iso.org/standard/78176.html

OWASP Foundation. (2023). *OWASP API Security Top 10 - 2023*. https://owasp.org/API-Security/editions/2023/en/0x11-t10/

# Appendix A - Initial Test Case Table

| ID | Requirement | Level / Type | Test condition / data | Expected result | Priority |
|---|---|---|---|---|---|
| TC-01 | FR-01 | System / Functional | Register valid authorised public HTTPS API | API saved and visible | High |
| TC-02 | FR-01, NFR-SEC-01 | System / Security | Register localhost/private target | Rejected with safety explanation | High |
| TC-03 | FR-02 | System / Functional | Add GET endpoint, expected 200, threshold 1000 ms | Endpoint expectation saved | High |
| TC-04 | FR-03, FR-04 | Integration/System | Actual status differs from expected | Check fails and defect created/updated | High |
| TC-05 | FR-03, FR-04 | Security | Protected endpoint returns 200 unauthenticated | Critical/Urgent auth defect | High |
| TC-06 | FR-03 | Unit / Performance | Threshold 1000 ms; test 1000 and 1001 | Boundary passes; above boundary fails | High |
| TC-07 | FR-03, NFR-REL-01 | Integration / Reliability | Timeout/unreachable target | Availability failure recorded; app stays usable | High |
| TC-08 | FR-05 | System / Functional | Assign defect/change priority/status | Updated values visible | Medium |
| TC-09 | FR-06 | Unit/System / Regression | Resolved defect; same check later passes | Defect closes and retest time recorded | High |
| TC-10 | FR-07 | Unit/System / Functional | One pass + one fail | Dashboard executed=2, pass rate=50% | Medium |
| TC-11 | FR-08 | System / Functional | Export after results/defects exist | Valid CSV contains evidence | Medium |
| TC-12 | NFR-USA-01 | Acceptance / Usability | Representative tester reviews failure messages | Tester can explain failure without source code | Medium |

# Appendix B - Initial Requirements Traceability Matrix

| Requirement | Acceptance criteria | Tests | Prototype component | Evidence status at draft stage |
|---|---|---|---|---|
| FR-01 | AC-01 | TC-01, TC-02 | ApisController, UrlSafetyService | Implemented; execution pending |
| FR-02 | AC-02 | TC-03 | ApisController, ApiEndpoint | Implemented; execution pending |
| FR-03 | AC-02, AC-03 | TC-04, TC-05, TC-06, TC-07 | ApiCheckService, CheckEvaluator | Implemented; execution pending |
| FR-04 | AC-03 | TC-04, TC-05 | DefectLifecycleService | Implemented; automated tests prepared |
| FR-05 | AC-04 | TC-08 | DefectsController | Implemented; manual evidence pending |
| FR-06 | AC-04 | TC-09 | DefectLifecycleService | Implemented; automated test prepared |
| FR-07 | AC-05 | TC-10 | DashboardService, Home view | Implemented; calculation test prepared |
| FR-08 | - | TC-11 | ReportsController | Implemented; manual evidence pending |
| NFR-SEC-01 | AC-01 | TC-02 | UrlSafetyService | Implemented; evidence pending |
| NFR-REL-01 | AC-02 | TC-07 | ApiCheckService | Implemented; integration evidence pending |
| NFR-MAINT-01 | - | Automated suite | Service separation + MSTest | Implemented |
| NFR-USA-01 | AC-02/03 | TC-12 | CheckEvaluator messages | Implemented; acceptance evidence pending |
| NFR-PERF-01 | TBC | PT-01 planned | Dashboard | Planned / stakeholder validation needed |
