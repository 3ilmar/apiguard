# Requirements Traceability Matrix (Initial)

| Requirement | Summary | Acceptance criteria | Test cases | Prototype component | Status |
|---|---|---|---|---|---|
| FR-01 | Register API | AC-01 | TC-01, TC-02 | ApisController, UrlSafetyService | Implemented; execution evidence pending |
| FR-02 | Register endpoints | AC-02 | TC-03 | ApisController, ApiEndpoint | Implemented; execution evidence pending |
| FR-03 | Run automated checks | AC-02, AC-03 | TC-04, TC-05, TC-06, TC-07 | ApiCheckService, CheckEvaluator | Implemented; execution evidence pending |
| FR-04 | Create/update defects from failures | AC-03 | TC-04, TC-05 | DefectLifecycleService | Implemented; automated tests included |
| FR-05 | Assign/prioritise/manage defects | AC-04 | TC-08 | DefectsController | Implemented; manual evidence pending |
| FR-06 | Retest and close | AC-04 | TC-09 | DefectLifecycleService | Implemented; automated test included |
| FR-07 | Quality dashboard | AC-05 | TC-10 | DashboardService, Home/Index | Implemented; automated calculation test included |
| FR-08 | Export reports | - | TC-11 | ReportsController | Implemented; manual evidence pending |
| NFR-SEC-01 | Block unsafe local/private targets | AC-01 | TC-02 | UrlSafetyService | Implemented; manual evidence pending |
| NFR-REL-01 | Graceful endpoint failure | AC-02 | TC-07 | ApiCheckService | Implemented; integration evidence pending |
| NFR-MAINT-01 | Separated testable rules | - | Automated suite | Services + MSTest project | Implemented |
| NFR-USA-01 | Understandable failure evidence | AC-02/03 | TC-12 | CheckEvaluator messages | Implemented; acceptance evidence pending |
| NFR-PERF-01 | Dashboard target (provisional) | - | PT-01 to be added | DashboardService/View | Planned / TBC |

Traceability supports QA and change management by showing why each implemented behaviour exists, which tests provide evidence, what must be retested when a requirement changes, and where uncovered or unexecuted requirements remain.
