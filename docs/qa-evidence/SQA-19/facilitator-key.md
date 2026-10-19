# Facilitator-only assessment key

**Do not show or read this file to the participant before their answers are collected.** This file describes the synthetic fixture and assessment criteria. It contains no human results. Human execution status remains **NOT RUN**.

The application supplies the message text through `SecureApiQualityPlatform.Web/Services/CheckEvaluator.cs`. The availability fixture uses the reason `Request timed out.`, matching the timeout reason passed by `ApiCheckService`. No network timeout is induced in this session.

| Scenario / Recent checks row | Check | Evaluator message expected on screen | Meaning/evidence sufficient for observer assessment |
|---|---|---|---|
| SQA19-01 / 1 | Expected status | Expected HTTP 200 but received 400. | Tester identifies a status mismatch and cites expected 200 versus observed 400. No requirement to explain HTTP semantics beyond the mismatch. |
| SQA19-02 / 2 | Response time | Response took 1001 ms, exceeding the 1000 ms threshold. | Tester identifies that response duration exceeded its allowed limit and cites 1001 ms versus 1000 ms. A difference of only 1 ms still fails the configured check. |
| SQA19-03 / 3 | Authentication protection | Endpoint is marked authentication-required but unauthenticated request returned HTTP 200. | Tester identifies that an unauthenticated request received HTTP 200 despite authentication being required and points to that mismatch. Do not require an unsupported claim that data was accessed or authentication was definitively exploitable. |
| SQA19-04 / 4 | Content type | Expected content type containing 'application/json' but received 'text/html'. | Tester identifies a response-format/content-type mismatch and cites expected application/json versus received text/html. |
| SQA19-05 / 5 | Availability | Request failed: Request timed out. | Tester identifies that the request could not complete because it timed out and cites the displayed timeout reason. Do not require a timeout duration: none is displayed, and the fixture establishes no root cause or server status. |

Accept accurate paraphrases. Assess the unaided interpretation and evidence, not memorized wording or prior knowledge. A FAIL badge alone does not establish that the tester understands the reason. Keep the participant's own words unchanged; record assessment separately. If the tester can explain the mismatch and evidence while criticizing the wording, record both the PASS and the feedback. If the facilitator supplied a needed answer, the response cannot count as an unaided pass.

All five scenario assessments and the session outcome must remain unassigned until the representative tester session has actually occurred. Dashboard HTML checks, fixture tests and build/test results establish preparation only.
