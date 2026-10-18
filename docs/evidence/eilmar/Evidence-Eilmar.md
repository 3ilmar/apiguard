\# Eilmar - Assessment 2 Evidence



This folder contains evidence for my Assessment 2 contributions to the Secure API Quality and Vulnerability Management Platform.



My main work included SQLite persistence, persistence regression testing, endpoint duplicate-handling improvements, defect investigation, and CI quality gates.



\---



\## 1. Baseline Regression Suite



Before starting the final-project persistence changes, the existing application passed all 22 automated tests.



!\[Baseline - 22 tests passed](./eilmar-01-baseline-22-tests.png)



\*\*Result:\*\* 22/22 tests passed.



This baseline was used to check that later persistence changes did not break existing behaviour.



\---



\## 2. Copilot Review of SQLite Persistence



\### Prompt used



> Review my new SqlitePlatformStore.cs, PlatformDbContext.cs, and the SQLite configuration in Program.cs. I am replacing in-memory storage with SQLite for registered APIs and endpoints. Identify any EF Core lifetime, persistence, duplicate-detection, concurrency, or data-integrity risks. Do not rewrite the whole application. Give specific recommendations and tests I should perform.



\### Copilot response



!\[Copilot SQLite review](./eilmar-02-copilot-sqlite-review.png)



\### Human decision



I reviewed the recommendations rather than accepting them automatically.



I accepted the recommendation to remove the duplicate `IPlatformStore` registration because the later `InMemoryPlatformStore` registration could prevent SQLite from being used.



I also accepted the recommendation to eventually persist check results and defects.



The recommendation to immediately introduce full EF Core migrations and database-level uniqueness constraints was deferred because the application is an academic prototype and those changes would require additional schema and deployment work.



The existing `IDbContextFactory` design was retained because it avoided keeping a scoped `DbContext` inside the singleton store.



\---



\## 3. DEF-01 - SQLite DateTimeOffset Ordering Failure



During manual execution after enabling SQLite, the dashboard failed with a `NotSupportedException`.



!\[SQLite DateTimeOffset defect](./eilmar-03-defect-datetimeoffset.png)



\### Cause



The EF Core SQLite provider could not translate an `ORDER BY` operation using the `DateTimeOffset` `CreatedAtUtc` property.



\### Correction



The records were materialised before applying the DateTimeOffset ordering in application code.



\### Verification



The dashboard loaded successfully after the correction and the existing automated regression tests continued to pass.



\---



\## 4. DEF-02 - Endpoint Persistence Failure



During endpoint creation with the SQLite store, the application produced a `DbUpdateConcurrencyException`.



!\[Endpoint insertion defect](./eilmar-04-defect-endpoint-insert.png)



\### Cause



The new endpoint was originally being added through the loaded API entity graph, which resulted in incorrect EF Core entity-state behaviour for the disconnected endpoint object.



\### Correction



The persistence logic was changed to explicitly add the endpoint through the endpoint `DbSet`.



API existence and `ApiId` consistency checks were also added.



\### Verification



Endpoint creation worked after the change and the endpoint remained available after restarting the application.



\---



\## 5. Copilot Review of Complete Persistence and Duplicate Handling



\### Prompt used



> I have completed SQLite persistence for RegisteredApi and ApiEndpoint. ApiCheckResult and Defect are still stored in in-memory lists. Review my current design and recommend the smallest changes needed to persist results and defects using the existing PlatformDbContext. Also review whether endpoint HTTP methods should be case-insensitive while endpoint paths remain case-sensitive. Suggest focused regression tests. Do not redesign the whole application.



\### Copilot response



!\[Copilot persistence review](./eilmar-05-copilot-persistence-review.png)



\### Human decision



The following recommendations were accepted:



\- replace in-memory result and defect storage with SQLite persistence

\- perform explicit existence checks for referenced APIs and endpoints

\- normalise HTTP methods

\- preserve endpoint path case sensitivity

\- add regression and persistence tests



The final duplicate-handling rule was:



\- `GET /users` and `get /users` are treated as the same method/path combination

\- `/users` and `/users/` are treated as equivalent

\- `/users` and `/Users` remain different paths because API path casing may be significant



\---



\## 6. API and Endpoint Persistence



The application was stopped and restarted after creating a registered API and endpoint.



The API and its configured endpoint were still available after restart.



!\[API and endpoint persistence](./eilmar-06-api-endpoint-persistence.png)



\*\*Verified data:\*\*



\- API: `Persistence Test API`

\- Base URL: `https://jsonplaceholder.typicode.com`

\- Endpoint: `GET /posts/1`

\- Expected status: `200`

\- Response-time threshold: `2000 ms`



This confirmed that API and endpoint data were no longer process-local.



\---



\## 7. Check Result and Defect Persistence



Automated checks were run against the persisted endpoint.



After restarting the application, the dashboard still contained the executed check results and generated defect.



!\[Results and defect persistence](./eilmar-07-results-defect-persistence.png)



The dashboard showed:



\- 1 registered API

\- 1 configured endpoint

\- 6 executed checks

\- 5 passed checks

\- 1 failed check

\- 83.3% pass rate

\- 1 open defect



The failed Server Disclosure check also remained visible after restart.



This provided manual evidence that check results and defects were persisted successfully.



\---



\## 8. Final Regression Suite



Five new SQLite integration and regression tests were added.



!\[Final automated tests](./eilmar-08-final-27-tests.png)



The final suite on my branch contained:



\- original tests: 22

\- new persistence/regression tests: 5

\- total: 27

\- passed: 27

\- failed: 0



The added tests covered:



\- API and endpoint persistence across a new store instance

\- check-result persistence

\- defect persistence

\- case-sensitive endpoint paths

\- HTTP method normalisation and duplicate prevention



\---



\## 9. Copilot Assistance for CI Quality Gates



\### Prompt used



> I have a .NET 8 ASP.NET Core MVC solution with MSTest and 27 passing tests. I want a GitHub Actions quality-gate workflow for ENSE707. It should restore dependencies, build in Release mode, run all automated tests, and perform a basic smoke test by starting the web application and checking that the home page responds. Review the approach for reliability and keep it appropriate for an academic prototype rather than production deployment.



\### Copilot response



!\[Copilot CI summary](./eilmar-09-copilot-ci-summary.png)



\### Human review



Copilot created an initial GitHub Actions workflow containing:



\- dependency restore

\- Release build

\- automated test execution

\- temporary SQLite database

\- HTTP home-page smoke test



The approach passed local validation with 27/27 automated tests and a successful home-page response.



The first GitHub execution revealed that part of the generated temporary database configuration was invalid in the GitHub Actions context.



I manually inspected the workflow error and changed the SQLite configuration to use the runner temporary directory from inside the Bash smoke-test step.



This is an example where AI-generated output required human review and correction before it was reliable.



\---



\## 10. Successful CI Quality Gate



After correcting the GitHub Actions configuration, the workflow completed successfully.



!\[Successful GitHub Actions quality gate](./eilmar-10-ci-quality-gate-passed.png)



The CI workflow verifies:



\- dependency restoration

\- Release build

\- automated test execution

\- application startup

\- HTTP smoke response



The failed first execution and successful corrected execution provide evidence that the workflow itself was tested and debugged rather than only being added to the repository without verification.



\---



\## Contribution Summary



My Assessment 2 development work focused mainly on:



\- adding SQLite persistence infrastructure

\- persisting APIs and endpoints

\- persisting automated check results and defects

\- identifying and correcting SQLite-specific defects

\- correcting endpoint path duplicate handling

\- adding five persistence/regression tests

\- increasing the automated test suite from 22 to 27 tests on my branch

\- creating and debugging a GitHub Actions quality gate

\- using Copilot for review and recommendations while manually evaluating, modifying, accepting, or deferring its suggestions

