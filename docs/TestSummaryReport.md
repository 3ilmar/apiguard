# Test Summary Report - Template for Real Execution Evidence

> Complete this after running the prototype. Do not invent pass/fail numbers.

## 1. Summary
Build/commit tested: [insert]
Date/time: [insert]
Tester(s): [insert]

## 2. Features tested
[insert]

## 3. Features not tested
[insert]

## 4. Test environment
OS: [insert]
.NET SDK/runtime: [insert]
Browser: [insert]
Authorised API target: [insert]

## 5. Test results
| Test area | Planned | Executed | Passed | Failed | Blocked | Notes |
|---|---:|---:|---:|---:|---:|---|
| Automated MSTest | | | | | | |
| Registration/safety | | | | | | |
| Endpoint scanning | | | | | | |
| Defect/retest workflow | | | | | | |
| Dashboard/export | | | | | | |

## 6. Defects found
Link to DefectLog.md / exported defect evidence.

## 7. Defects fixed and retested
[insert real evidence]

## 8. Known issues / residual risk
- In-memory persistence.
- RBAC not yet implemented.
- Selected passive checks do not prove absence of OWASP API Security Top 10 vulnerabilities.
- [add actual remaining issues]

## 9. Release/demo recommendation
Choose one with evidence: **Recommended for demonstration** / **Not recommended until critical defects are fixed**.

## 10. Lessons learned
[insert]
