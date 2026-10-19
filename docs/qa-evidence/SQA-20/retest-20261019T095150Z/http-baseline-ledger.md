# Normal SQLite application: post-merge HTTP baseline

Supporting execution facts only; this is not a student-authored assessment report.

The same runner also completed against the post-fix Release application:
`postfix-http/summary.json` and `postfix-http/checks.json` record **29 executed,
28 PASS and 1 FAIL**, matching the baseline. The remaining failure is the same
pre-existing malformed numeric binding issue. No additional HTTP regression was
observed. Its process records confirm all three normal application processes
were stopped after testing; the owned fixture was shut down in `finally`.

The complete baseline is `baseline-http-4/summary.json` and `checks.json`:
**29 executed checks, 28 PASS and 1 FAIL**. The sole failure is the existing
malformed numeric endpoint-input defect. All other checks completed, including
CSV parsing and exact API/endpoint/check-result/defect row equality across an
application process restart. All application and fixture processes were stopped.

The normal built Release application used explicit isolated SQLite databases and
the owned HTTP fixture on ports 51921/51922. The initial process verified private
target rejection with the safety override disabled. Later processes enabled the
override solely to call the owned loopback fixture. No public API was scanned.
Actual UTC start/end timestamps, source/DLL hashes, process IDs, HTTP responses,
CSV downloads and JSON row snapshots are in the run directory. The normal
application database and original SQA-20 evidence were not changed.

## Confirmed pre-existing endpoint validation issue

The malformed numeric probe posts `ExpectedStatusCode=not-a-number` through the
real MVC action with a valid antiforgery token. It returns HTTP 302 and persists
the synthetic `/numeric-probe` endpoint with `ExpectedStatusCode=200`.
See `baseline-http-4/findings.json` and the corresponding request/response in
`baseline-http-4/http-requests.json`.

`ApisController.AddEndpoint` clears model binding errors before revalidating the
bound object. The numeric property retains its initializer when binding fails.
`endpoint-validation-parent-source.txt` records the same `ModelState.Clear()`
in the first parent of merge commit `7f28ee9`; the issue predates PR #12.
`endpoint-validation-history.txt` records relevant controller history. This
business-logic limitation is left for separate ownership; it is not attributed
to the accessibility changes or silently marked PASS.

## Preserved preliminary attempts

| Directory | Actual outcome |
|---|---|
| `baseline-http` | Setup failure: sandbox development DataProtection key writes denied; HTTP readiness did not complete. No product check executed. App/fixture stopped. |
| `baseline-http-2` | 21 checks passed; runner comparison of SQLite uppercase GUIDs to lowercase CSV GUIDs incorrectly failed. This was a harness assertion issue, not a CSV defect. The corrected runner normalizes GUID case. |
| `baseline-http-3` | 27 core checks passed; the numeric issue was separately recorded as confirmed and overall status FAIL. Full restart and CSV checks completed. |
| `baseline-http-4` | Final baseline runner: 28 PASS, 1 FAIL; numeric issue is an explicit nonfatal failed check and default private-target rejection is included. |

The successful executions used normal local host permissions after the sandbox
setup failure. Python 3.12.14 was used; the .NET build/test environment is recorded
separately by the parent regression assessment. Source identity includes the
uncommitted harness/evidence state; no commit or push is implied.

## Independent CSV harness review and verified post-fix execution

The earlier baseline and `postfix-http` runs used `csv.DictReader`, checked header
count, and checked dictionary length. Those assertions are weaker than exact CSV
schema validation: renamed columns and missing trailing raw fields could escape
the structural checks. Their recorded outputs remain unchanged, but their CSV
PASS claims must be read with this limitation. No replacement baseline run was
performed after the application fixes.

The corrected runner uses `csv.reader(strict=True)`, exact expected header names
and ordering, and raw logical-record widths before mapping records to names. It
also verifies the specific accepted values and omission guidance for the two
malformed report filter requests. The correction is retained as
`http-runner-schema-review.diff`, with the pre-review and verified runner source
snapshots alongside it.

Actual execution against the post-fix application is
`postfix-http-verified/summary.json`: **29 executed, 28 PASS, 1 FAIL**, with no
execution error. Both strengthened CSV checks and both malformed-filter checks
passed. The sole failed check is still the pre-existing malformed numeric endpoint
binding issue. The verified source identity now includes the runner SHA-256.
Application/fixture processes were stopped after completion.
