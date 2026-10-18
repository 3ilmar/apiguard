# GitHub Copilot Evidence: Lana (RBAC)

Screenshots are in `docs/evidence/lana/`. Prompts marked "drafted with help from an AI assistant (Claude)" were written with that assistant's help and then run unchanged in Copilot Chat in Visual Studio.

## Entry L-1: role selector (RoleController and layout)
- **Prompt (Copilot Chat, Visual Studio):** "Add a RoleController with a POST SetRole action that stores the chosen role in a cookie and redirects back only to local URLs. Add a role dropdown to Views/Shared/_Layout.cshtml that posts to it, and show a red alert for TempData["Error"]. Keep the existing navbar."
- **Prompt origin:** drafted with help from an AI assistant (Claude), then run unchanged in Copilot Chat.
- **What Copilot produced:** a `RoleController` (+39 lines) with enum validation, an anti-forgery attribute and a safe local redirect, and three edits to `_Layout.cshtml` adding a role dropdown and an error alert. Screenshot: lana-copilot-03-selector-build-failed.png
- **What I checked:** the layout failed to build when I built it, so I investigated the errors. I then rebuilt, ran the tests, ran the app, switched roles and refreshed the page. After refresh the dropdown kept the chosen role (screenshots: lana-04-selector-admin-after-refresh.png, lana-05-selector-viewer-after-refresh.png). The page before the change is in lana-02-dashboard-before.png.
- **Useful:** the safe redirect (`Url.IsLocalUrl`), the anti-forgery attribute and the enum validation were correct.
- **Problems found:**
  1. The layout failed to build (RZ1031): C# expressions (`@selViewer` and similar) were placed inside the `<option>` tags. I found this from the build error. The Error List screenshot was not retained; the error code and lines were recorded from the build output at the time.
  2. Copilot's own run shows "Build failed" after its layout edits, a further layout edit, then "Skipped building solution", and it still reported the task as done. Its final layout state was never confirmed by a build; when I built it myself it failed with RZ1031. Screenshot: lana-copilot-03-selector-build-failed.png
  3. The controller hard-coded its own `"apiguard_role"` string instead of using `CookieRoleProvider.CookieName`.
  4. The role cookie was set with `HttpOnly = false` and no `SameSite`.
  5. The layout had a redundant second anti-forgery token, three hard-coded options and a null-conditional on an injected service.
  Findings 3-5 came from a review of the pushed code with an AI assistant (Claude); I then verified and fixed them.
- **What I changed and why:** replaced the options with a `foreach` and `selected="@(condition)"`; reused the shared constant; set `HttpOnly = true` and `SameSite = Lax`; removed the redundant token and code.
- **Decision:** accepted with modifications.
- **Quality risks:** incorrect syntax, completion reported without a passing build, insecure defaults, duplicated constant (maintainability), unnecessary code.

## Entry L-2: role enum, provider and tests
- **Prompts (drafted with help from an AI assistant, Claude):**
  1. "Add a UserRole enum with Viewer, Tester and Admin to this ASP.NET Core MVC project. File-scoped namespace, no new packages."
  2. "Add an IRoleProvider with a CurrentRole property, and a CookieRoleProvider that reads a cookie named apiguard_role via IHttpContextAccessor. Default to Admin when the cookie is missing or invalid so existing behaviour and tests keep working. No new packages, no database, no real login."
  3. "Write MSTest tests for CookieRoleProvider: no cookie returns Admin, a Viewer cookie returns Viewer, a garbage cookie returns Admin, and the numeric value 7 returns Admin. Assert the returned role."
- **Origin of the files:** Copilot Chat created `IRoleProvider.cs` (+8) and `CookieRoleProvider.cs` (+31) in response to prompt 2 (screenshot: lana-copilot-01-provider-prompt.png) and edited `Models/Enums.cs` (+7) in response to prompt 1 rather than creating a separate file (screenshot: lana-copilot-02-enum-prompt-edited-enums.png). `CookieRoleProviderTests.cs` came from Copilot (not shown in the screenshots). `RoleProviderTests.cs`, `UserRole.cs` and the empty `RoleProvider.cs` came from reference code supplied by an AI assistant (Claude).
- **What I checked:** built after each change and ran the tests. Baseline before my changes: 22 passing (lana-01-baseline-22-tests.png).
- **Problems found:**
  1. The role-provider types were defined in two files (CS0101, CS0111, CS0229), found by the build. The Error List screenshot was not retained; the error codes were recorded from the build output at the time.
  2. `UserRole` was not an enum at that point, so `Admin` and `Enum.IsDefined` failed to compile (CS0117, CS0453), found by the build. Screenshot: lana-defect-02-CS0117-CS0453-build.png
  3. The test count showed 30 instead of 26 because a second test file duplicated four tests. Screenshot: lana-defect-04-tests-30-duplicates.png
  4. The unused `using Microsoft.Extensions.FileProviders;` and the stray change to `Models/Enums.cs` were found in a review of the pushed code with an AI assistant (Claude).
  5. The provider failed open: a missing, invalid or undefined role cookie defaulted to Admin (DEF-006). A teammate found this in peer review of the pull request. This default came from the instruction in prompt 2, which was drafted with help from an AI assistant (Claude) and which I ran unchanged, so it was a design choice in the prompt rather than a Copilot error. Copilot followed the instruction as written.
- **What I changed and why:** deleted the duplicate definitions and the duplicate test file, restored `Enums.cs`, removed the unused using. Retest: 26 tests passing (lana-06-tests-26-final.png). After peer review I changed the fallback role from Admin to Viewer (fail closed), replaced the "Viewer cookie" provider test with an "Admin cookie" test (a Viewer cookie can no longer be told apart from the default), and documented in the code that the cookie is a demo mechanism, not authentication. Retest: 26 tests passing; deleting the cookie in DevTools shows Viewer in the selector (screenshot: lana-06b-default-viewer-no-cookie.png).
- **Decision:** accepted after modification; the default role was changed after peer review.
- **Quality risks:** duplicate code, tests that inflate counts, changes outside the requested scope, unused imports, and an insecure fail-open default introduced through the prompt instead of the tool, which shows that a prompt can encode a risky design decision and that human peer review caught it.

## Entry L-3: tool availability
- **What happened:** my first Copilot Chat prompt returned `BadRequestFailure` (exception type `CopilotApiException`, HTTP 400 BadRequest, correlation ID f84f422d-08e3-470d-a660-0c42236a9efc). The screenshot was not retained; the exception text was recorded.
- **What I did:** Copilot Chat worked again for the later prompts above. I did not record what resolved it.
- **Risk:** the team can't rely on AI tooling being available, so every change must be understandable and doable without it.

## Entry L-4: Copilot review of the original role cookie code (written by me)
- **Prompt (written by me):** "this is RoleController.cs, its an ASP.NET core MVC prototype, the code sets a cookie storing users role. what security issues does it have? rank by risk and which lines. as a scope limit, no new libraries, no login system, keep fixes small." followed by the code with line numbers.
- **Input:** the original `RoleController.cs` from commit 3e47ab8, pasted into a new chat as text so Copilot saw the unfixed version.
- **What Copilot said:** six ranked findings with line numbers and fixes. Screenshots: lana-copilot-04-security-review-1.png, -2.png, -3.png
- **What it caught:** HttpOnly=false (High), no Secure flag (High), no SameSite (Medium), one-year expiry (Medium), and the forgeable client-side role (ranked Critical).
- **What it missed or got wrong:** it didn't see the duplicated cookie-name constant because it only had one file. It flagged `IsEssential = true` as a consent bypass, which doesn't apply here because the app has no consent middleware. Its main fix (signing the cookie with IDataProtection) doesn't stop a user choosing Admin through the unauthenticated SetRole action, and it also goes beyond the "keep fixes small" limit I gave it. Its code snippet used an undefined `protectedValue`.
- **What I checked:** I compared each finding to the code, searched the project and found no cookie-consent or authentication middleware, and confirmed the fix in the browser over HTTPS: DevTools shows `apiguard_role` with HttpOnly ticked, Secure ticked, SameSite=Lax and a session expiry. Screenshot: lana-07-cookie-flags.png
- **Decision:** accepted HttpOnly, SameSite=Lax, Secure (`Request.IsHttps`) and a session cookie. Rejected IDataProtection signing, as it doesn't address the real problem and adds scope. Kept IsEssential=true. The forgeable cookie remains a known prototype limitation; real authentication is future work.
- **Quality risks:** the same tool that generated the insecure defaults identified them when asked to review, so security prompts matter; its suggested top fix addressed the symptom (cookie tampering) rather than the cause (no authentication); AI review can drift outside the stated scope.