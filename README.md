# Secure API Quality and Vulnerability Management Platform

ENSE707 mid-project prototype written in C# / ASP.NET Core MVC.

## Prototype scope

- Register a public HTTP(S) API and endpoint expectations.
- Run non-destructive checks for availability, expected HTTP status, response time, content type, HTTPS, unauthenticated access to endpoints marked as protected, permissive CORS, and Server-header disclosure.
- Create and prioritise defects from failed checks.
- Assign defects and move them through a lightweight lifecycle.
- Retest checks; a passing retest can close an existing resolved/retest defect.
- View dashboard metrics and recent quality evidence.
- Export check and defect evidence to CSV.
- Block localhost/private network targets by default to reduce SSRF risk.

## Deliberate limitations of the initial prototype

- In-memory storage only; data resets when the process restarts.
- No production authentication/RBAC yet.
- No credential or secret storage.
- No active exploitation, fuzzing, or high-volume rate-limit testing.
- Security checks are a selected, non-destructive subset and are not a claim of OWASP Top 10 compliance.

## Run locally

1. Install .NET 8 SDK (or Visual Studio 2022 with ASP.NET and web development).
2. You can open `SecureApiQualityPlatform.sln` in Visual Studio, or from the repository root:
   - `dotnet restore SecureApiQualityPlatform.Web/SecureApiQualityPlatform.Web.csproj`
   - `dotnet run --project SecureApiQualityPlatform.Web`
3. Open the HTTPS localhost URL shown in the terminal.
4. Register a public test API you are authorised to test.
5. Add endpoints and click **Run all checks**.

## Run tests

`dotnet test SecureApiQualityPlatform.Tests/SecureApiQualityPlatform.Tests.csproj --collect "XPlat Code Coverage"`

## Safety and authorisation

Only test APIs that you own or are authorised to test. The prototype intentionally uses low-impact requests and blocks private-network targets by default. Do not disable the protection unless your team has an approved local test environment and understands the SSRF implications.

## Evidence folders

See `docs/` for requirements, test strategy, test plan, test cases, RTM, defect management, governance, Copilot evidence template, and next-step plan.
