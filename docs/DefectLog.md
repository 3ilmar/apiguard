# Defect Log

Real defects observed during development and testing. Each entry records the failing evidence and the retest result.

| ID | Title | Severity | Priority | Status | Requirement/Test | Expected | Actual | Owner | Fix/Retest evidence |
|---|---|---|---|---|---|---|---|---|---|
| DEF-001 | Duplicate role provider type definitions | Medium | Medium | Closed | NFR-MAINT-01 / build | Solution builds | Build failed: CS0101, CS0111, CS0229 | Lana | Same types existed in two files; removed the duplicate and kept one definition. Rebuild clean. |
| DEF-002 | UserRole not defined as an enum | Medium | Medium | Closed | NFR-MAINT-01 / build | Build succeeds | Build failed: CS0117, CS0453 (`Admin` missing, `Enum.IsDefined` rejected) | Lana | Replaced with enum (Viewer, Tester, Admin). Build clean. |
| DEF-003 | Layout failed to build with RZ1031 | Medium | High | Closed | NFR-USA-01 / build | Layout compiles and role dropdown works | Build failed on the three `<option>` tags: C# inside the tag attributes | Lana | Replaced with a foreach and `selected="@(condition)"`. Build clean, selector keeps role after refresh. |
| DEF-004 | Duplicate role-provider tests | Low | Low | Closed | NFR-MAINT-01 / automated tests | Four role-provider tests run without duplication; test count is accurate | Duplicate tests inflated the total test count to 30 instead of 26 | Lana | Removed the duplicate test file. Retest: 26 tests passing. |
| DEF-005 | Insecure role cookie configuration and duplicated cookie name | Medium | Medium | Closed | NFR-SEC-03 (new, RBAC) | Role cookie uses `HttpOnly` and `SameSite=Lax`; shared cookie name is consistent; role selector retains the selected role after refresh | Cookie configured with `HttpOnly=false` and no `SameSite`; cookie name duplicated across two files | Lana | Set `HttpOnly` and `SameSite=Lax`, and used a shared cookie-name constant. Retest confirmed the selector keeps the role after refresh. |