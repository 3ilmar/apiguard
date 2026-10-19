# PM-VALIDATION-01: endpoint numeric-input assessment

Technical triage evidence for a separate application fix; no application logic or historical execution evidence is changed by this assessment. Source reviewed: `e43fe52373d3d74b617610e2dbee418ac9b3f9b4`. No new HTTP execution is claimed.

## Root cause

Source analysis of [`ApisController.AddEndpoint`](../../../SecureApiQualityPlatform.Web/Controllers/ApisController.cs) explains the saved HTTP failure. [`ApiEndpoint.ExpectedStatusCode`](../../../SecureApiQualityPlatform.Web/Models/ApiEndpoint.cs) is an `int`, initialized to `200`, with `[Range(100, 599)]`. Binding `not-a-number` cannot assign that property and records a model-binding error; the property retains `200`. The action copies the bound numeric value into a new endpoint, calls `ModelState.Clear()`, and then `TryValidateModel(endpoint)`. Clearing removes the failed conversion and attempted input. Annotation validation sees valid integer `200`, so the action calls `TryAddEndpoint` and redirects after persistence.

The existing range check therefore handles `99` but cannot recover the erased conversion error. This is an MVC validation defect, not a SQLite conversion error or an accessibility regression. The same pattern can affect other initialized numeric fields such as `MaxResponseTimeMs`; that extension requires its own regression execution and is not a new confirmed result here.

The controller and endpoint-model SHA-256 hashes match the [saved verified HTTP source identity](../SQA-20/retest-20261019T095150Z/postfix-http-verified/source-identity.json). The [parent controller snapshot](../SQA-20/retest-20261019T095150Z/endpoint-validation-parent-source.txt) and [history](../SQA-20/retest-20261019T095150Z/endpoint-validation-history.txt) establish that the clearing behavior predates PR #12.

## Reproduction and actual result

Use a disposable local application and isolated SQLite database, an Admin session, an existing registered API and a fresh antiforgery token from its Details form. Do not reuse the normal application database or scan a public target.

1. POST `/Apis/AddEndpoint/{apiId}` as `application/x-www-form-urlencoded` with the real antiforgery token and these fields:

   ```text
   Name=Malformed numeric probe
   Path=/numeric-probe
   Method=GET
   ExpectedStatusCode=not-a-number
   MaxResponseTimeMs=60000
   ExpectedContentType=application/json
   RequiresAuthentication=true
   ```

2. Record the response without following redirects, then inspect the API's endpoints in the isolated database.
3. Expected: validation view with an actionable numeric-input error; no endpoint persisted and no successful redirect.
4. Observed in the saved execution: HTTP `302` to API Details; one `/numeric-probe` endpoint persisted with `ExpectedStatusCode=200`.

The [request ledger](../SQA-20/retest-20261019T095150Z/postfix-http-verified/http-requests.json) preserves the submitted fields and response headers. The [finding](../SQA-20/retest-20261019T095150Z/postfix-http-verified/findings.json) preserves the persisted row. That run's [summary](../SQA-20/retest-20261019T095150Z/postfix-http-verified/summary.json) remains **29 executed: 28 PASS, 1 FAIL**. Baseline and preliminary-attempt limitations remain in the [HTTP ledger](../SQA-20/retest-20261019T095150Z/http-baseline-ledger.md).

## Recommended correction in a separate branch and PR

Reject invalid incoming `ModelState` before reconstructing the endpoint or clearing any entries. Return the existing Details view model with the validation errors and do not call the store. The smallest correction is an early invalid-model-state guard after the API existence check, before normalization/reconstruction; retain the subsequent normalized-object validation. Confirm that rejecting initial annotation failures before trimming is acceptable for existing normalization behavior.

If validation must continue after normalization, replace the blanket clearing strategy with validation that preserves original binding errors and attempted values. Do not parse invalid input into a default or treat `TryValidateModel` as a replacement for binding validation. Verify that redisplayed errors and attempted input use the form's actual keys: posted names are `ExpectedStatusCode`/`MaxResponseTimeMs`, while the Razor expressions use `NewEndpoint.*`. Keep label associations intact.

## Required regression tests and current gaps

- Add an `AddEndpoint` controller test with a numeric model-binding error already in `ModelState`, a default-valued endpoint and a valid path. Assert a Details validation response, retained error and no store write. Passing a typed `ApiEndpoint` alone cannot simulate a failed text-to-integer conversion.
- Add real MVC HTTP tests with valid Admin/antiforgery setup for `not-a-number`, integer overflow, empty/missing input and malformed `MaxResponseTimeMs`; assert actionable error output and unchanged SQLite row count for rejected requests. Define missing-field behavior explicitly rather than assuming defaults are acceptable.
- Verify valid boundaries (`100`/`599` status, `50`/`60000` ms), invalid ranges (`99`/`600`, `49`/`60001` ms), ordinary valid creation and existing trimming/path normalization.
- Retain unsafe-path and duplicate rejection, nonexistent API handling, role enforcement and antiforgery protection. Check that a failed submission does not produce a success redirect or a persisted row after restart.
- Promote the saved malformed-input HTTP probe into the normal automated regression gate, then rerun the complete .NET suite and isolated HTTP suite. Do not mark this defect fixed until the probe passes with persistence assertions.

Existing [`RegistrationValidationTests`](../../../SecureApiQualityPlatform.Tests/RegistrationValidationTests.cs) test duplicate handling through the store; [`SqlitePlatformStoreIntegrationTests`](../../../SecureApiQualityPlatform.Tests/SqlitePlatformStoreIntegrationTests.cs) test persistence directly. [`EndpointPathValidatorTests`](../../../SecureApiQualityPlatform.Tests/EndpointPathValidatorTests.cs) cover path safety. [`RequireRoleAttributeTests`](../../../SecureApiQualityPlatform.Tests/RequireRoleAttributeTests.cs) inspect action role declarations. None of those tests exercises malformed numeric form binding through `AddEndpoint`. The existing [HTTP runner](../../../tools/PostMergeRegression/run_http_regression.py) already exposes this gap with its explicit nonfatal failed check; the historical 136/136 .NET PASS does not override it.

## Ownership and triage

The current [defect register](../../DefectLog.md) lists **PM-VALIDATION-01: Open; pre-existing before PR #12; Unassigned; coordinate Eilmar/Lana**. The read-only [GitHub issues listing](https://api.github.com/repos/3ilmar/apiguard/issues?state=all&per_page=100) returned 12 items, all pull requests, with no standalone issue or matching numeric-validation entry. No assignment to another teammate is evidenced by the records reviewed; unrecorded coordination cannot be ruled out. No issue, assignment, application fix or external message was created here. Agree ownership separately before implementing the correction.
