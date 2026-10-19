# Isolated post-merge HTTP regression

Build `SecureApiQualityPlatform.sln` in Release, then run Python 3.12 or later:

```powershell
python tools/PostMergeRegression/run_http_regression.py --output docs/qa-evidence/SQA-20/<new-run-directory>
```

The standard-library-only runner starts the built normal application on loopback
port 51921 with an explicit isolated SQLite database. An owned synthetic HTTP
fixture runs on 51922. Both ports must be unused. A nonempty output directory is
rejected, and every started process is tracked and stopped in `finally`.
The normal application's database is never used.

The first process verifies default private-target rejection. Only subsequent
test processes enable private targets, so the application can call the owned
loopback fixture. Real forms, antiforgery tokens, role cookies, MVC actions, Razor
views and SQLite persistence are used. The runner never invokes an external API.

The checks cover default Viewer denial, Tester/Admin permissions, registration
validation and duplicates, encoded traversal/range rejection, malformed numeric
binding, check execution and defects, assignment and passing retest closure,
dashboard rendering, results/defects CSV structure/filtering/escaping, malformed
report filters, and persistence across an application process restart. Browser
layout, accessibility, human acceptance and production authentication are outside
this runner's boundary.

CSV structure is checked with strict parsing, exact ordered headers and raw
logical-record widths before mapping field names. Malformed report-filter checks
require HTTP 400 plus the specific accepted values and omission guidance.

`summary.json`, `checks.json`, `findings.json`, HTTP response bodies, request
metadata, fixture requests, application logs, process records, source identity and
SQLite row snapshots are retained. The binary synthetic database remains local
and is excluded by the repository's existing `*.db` ignore rule. A nonzero exit
code can represent a recorded nonfatal product failure even when the remaining
checks complete; inspect `summary.json` and `checks.json`.

The baseline reproduced a pre-existing validation defect: posting
`ExpectedStatusCode=not-a-number` stores the endpoint with default status 200.
The runner counts this as a failed check and continues. It does not alter
application logic or treat the behavior as an accessibility fix.
