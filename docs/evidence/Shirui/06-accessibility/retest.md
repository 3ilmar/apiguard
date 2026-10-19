# Post-merge regression and SQA-20 accessibility retest

Technical execution evidence, not submitted assessment report prose or a human release decision.

## Source and environment

- Branch: `fix/sqa20-accessibility-postmerge`, created directly from fetched `origin/main` at PR #12 merge `7f28ee9239ba728a297ffd729da618abb1c0d415`.
- Initial working tree clean. The already-existing branch name pointed to `8e69c62` with zero unique commits; that pointer was preserved as `codex/backup-sqa20-postmerge-start-8e69c62`. Local `main` remains at `8e69c62`.
- Fixes are **uncommitted**. Identify tested source with [application diff](application-fix.diff), [baseline hashes](baseline-source-sha256.csv), [post-fix hashes](postfix-source-sha256.csv), and each HTTP run's source/DLL hashes. No new commit, push, rebase, merge or CI execution occurred.
- 19 October 2026, Pacific/Auckland (NZDT, UTC+13). Baseline .NET run: 22:52:36–22:53:10 NZDT. Post-fix .NET run: 22:58:57–22:59:34 NZDT. Final browser run: 22:59:37–23:03:22 NZDT. Exact timestamps remain in TRX/JSON/logs.
- Windows `10.0.26200` x64; Intel Core i7-12700H, 20 logical CPUs. SDK `10.0.402`, MSBuild `18.9.15`, target `net8.0`, runtime `.NET 8.0.32`. [SDK details](dotnet-info.txt).
- Installed Google Chrome `155.0.8059.80`, Microsoft Edge `155.0.4283.62`; genuine executables through headless Playwright `1.62.1`, fresh isolated contexts, device scale 1, `1440×900` and `375×812`. Node `24.19.0`, Python `3.12.14`, pinned axe-core `4.10.3`. [Browser environment](after/environment.json).
- Browser fixture: localhost:51920, isolated in-memory store, 2 APIs/2 endpoints/8 synthetic results/4 defects (one per severity), writes/scans disabled. Normal application HTTP regression uses isolated SQLite and owned loopback fixture on 51921/51922. Original app database is untouched. These are distinct test boundaries.

## Actual outcomes

| Check | Baseline | Post-fix | Evidence |
|---|---|---|---|
| Restore / Release build | PASS; 0 warnings/errors | PASS; 0 warnings/errors | [restore](baseline-restore.txt), [baseline build](baseline-build.txt), [post-fix build](postfix-build.txt) |
| Complete solution MSTest suite | 136/136 PASS; 0 failed/skipped | 136/136 PASS; 0 failed/skipped | [baseline TRX](baseline-suite/baseline.trx), [post-fix TRX](postfix-suite/postfix.trx), [test output](postfix-test.txt) |
| Normal SQLite HTTP regression | 29 executed: 28 PASS, 1 FAIL | 29 executed: 28 PASS, 1 FAIL | [baseline](baseline-http-4/summary.json), [final retest](postfix-http-verified/summary.json), [HTTP ledger and preliminary attempts](http-baseline-ledger.md) |
| Eight browser categories × two browsers | 14 PASS, 2 FAIL (badge compactness); 0 blocked/not run | 16 PASS, 0 FAIL/BLOCKED/NOT RUN | [before](before/browser-execution.json), [after](after/browser-execution.json) |
| Original accessible-name/contrast locations | FAIL in both browsers/both widths | 12 page/browser/viewport records PASS | [verified before scans](before-targets/accessibility-targets.json), [after](after-targets/accessibility-targets.json) |
| Original browser CSV downloads | 4 validated exports PASS | 4 validated exports PASS | [before CSV validation](before/csv-validation.json), [after CSV validation](after/csv-validation.json) |

The new severity category explicitly requires compact badges. Historical SQA-20 reported 16/16 for its narrower technical category criteria while separately recording the badge defect. That historical result is unchanged.

The .NET suite includes dashboard performance/edge cases, persistence, RBAC, URL/path validation, check reliability, defect lifecycle and CSV cases. Its passing results do not override the additional HTTP failure. Release coverage in **both** runs: **715/918 lines (77.89%)**, **325/492 branches (66.06%)** in `SecureApiQualityPlatform.Web`. This is separate from historical Debug coverage; the coverage denominators differ with build configuration. Exact files are indexed by [comparison.json](comparison.json).

## Confirmed defects and retest

| Original ID | Reproduced before application edits | Applied change | Actual retest |
|---|---|---|---|
| SQA20-A11Y-01 | Per browser/viewport: axe 22 unnamed controls; conservative DOM audit 26 label gaps, including placeholder-backed fields | Seven `asp-for` labels in API Details; GUID-based unique `for`/`id` pairs for each defect's status/priority/assignee | All 26 original controls per browser/viewport have unique IDs, associated labels, matching accessibility snapshots and correct native label-click behavior; checkbox toggles only its control and is restored. 0 naming rule failures / DOM gaps |
| SQA20-A11Y-02 | URL text `#d63384` on `#f6f8fb`: 4.2303:1. Export resting text `#6c757d`: 4.4076:1 | Scoped `.api-base-url` `#a61e65`; secondary outline-button Bootstrap variables use `#495057`, retaining white interactive-state text and focus styling | URL resting/hover 6.5904:1; export resting 7.6852:1, hover/focus/active 8.1764:1, both browsers/both widths. Noninteractive code focus/active states explicitly N/A |
| SQA20-UI-01 | Defect badges stretched to 45px desktop; narrow badges up to 111px | `align-self-start`, `flex-shrink-0`, flex gap | No stretched badges in either browser/width; severity text/counts and dashboard accessible progress names retained |

The normal-text criterion is at least 4.5:1, following [WCAG 2.2 contrast minimum](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html). Raw target scans, accessibility snapshots, computed composite colors and screenshots are retained under `before-targets/` and `after-targets/`.

Minimal application files changed:

- `SecureApiQualityPlatform.Web/Views/Apis/Details.cshtml`
- `SecureApiQualityPlatform.Web/Views/Defects/Index.cshtml`
- `SecureApiQualityPlatform.Web/wwwroot/css/site.css`

No controller, store, security service, workflow or business-logic change was made.

## Keyboard, compatibility and scanner classification

All six routes (dashboard, API list, both API details, Defects, registration) were exercised in both browsers at both widths. All **24 page/viewport/browser runs** passed forward and reverse traversal, with trusted events and measured focus visibility. Layout checks covered tables, long content, responsive containment, severity text/counts and actual browser CSV download/parse behavior.

Raw axe still flags one narrow endpoint-table wrapper on each API details page, in each browser: **4 raw `scrollable-region-focusable` instances**. Each instance was reached by genuine Tab and Shift+Tab, and ArrowRight changed scrollLeft **0→40px**. No `tabindex` or DOM focus mutation was used. The raw flags remain in JSON; they are classified as unconfirmed scanner findings for these tested browser versions, consistent with historical evidence. There are **0 unresolved rule failures** after this physical verification. Incomplete axe rules remain available for manual review; no full WCAG conformance is claimed.

Before/after screenshot pairs:

| Location | Before | After |
|---|---|---|
| Chrome narrow defects | [before](before/chrome/narrow-defects-viewport.png) | [after](after/chrome/narrow-defects-viewport.png) |
| Edge narrow defects | [before](before/edge/narrow-defects-viewport.png) | [after](after/edge/narrow-defects-viewport.png) |
| Chrome API details | [before](before/chrome/desktop-orders-viewport.png) | [after](after/chrome/desktop-orders-viewport.png) |
| Edge API details | [before](before/edge/desktop-orders-viewport.png) | [after](after/edge/desktop-orders-viewport.png) |

AI-assisted visual inspection covered both browsers' desktop/narrow Defects, API details/dashboard and selected focus screenshots. Automated measurements and screenshots do not substitute for representative-user or assistive-technology testing.

## Preserved attempts and tooling corrections

- First sandbox `dotnet --info` hit Windows Service Control Manager access denial. [Original output](dotnet-info-sandbox-attempt.txt) retained; normal local execution then succeeded.
- Initial full baseline browser run could not load axe because Windows Git CRLF checkout changed the vendored distribution's bytes. The availability error remains in `before/environment.json`. Removing only CRLF conversions reproduced all three exact pinned SHA-256 hashes; [repair record](axe-checkout-integrity-repair.json). Narrow `.gitattributes` rules retain LF for those files. **No integrity verification was bypassed.** Separate `before-targets/` scans then reproduced all naming/contrast failures before application edits. The final full browser run used verified axe throughout.
- Baseline/earlier HTTP CSV assertions were less strict than the final harness. Static review prompted exact ordered-header/raw-width validation with `csv.reader(strict=True)`. The final `postfix-http-verified/` execution uses the corrected harness; older baseline claims are explicitly limited in the [HTTP ledger](http-baseline-ledger.md). Prior runs and runner snapshots remain intact.
- Existing branch guard/provenance and keyboard runner were adapted for this branch and both viewport sizes. Archived historical runners were not edited.
- [Before console](before-browser-console.txt), [after console](after-browser-console.txt), [before target console](before-targets-console.txt), [after target console](after-targets-console.txt) preserve actual execution logs. Fixture launch/stop records and host output are retained separately.

## Remaining issues and review inputs

- **Confirmed pre-existing validation failure:** `ExpectedStatusCode=not-a-number` posts successfully (302) and persists default `200`. `ApisController.AddEndpoint` clears binding errors. [Evidence](postfix-http-verified/findings.json), [parent source](endpoint-validation-parent-source.txt) prove it predates PR #12. Leave open for separate ownership; do not call the overall regression suite fully passing.
- Existing `DEF-010` unconstrained defect transitions remains open. Cookie-selected demo roles are not authentication. SQLite migration/backup handling and production security remain separate risks.
- Both existing workflows already build/test; `dotnet.yml` uploads TRX/coverage, and the quality gate smoke uses an isolated SQLite database. No workflow edit needed. **Remote CI on the eventual submitted commit is not run/verified here.**
- [ ] Human screen-reader/assistive-technology and representative-user acceptance; review axe incomplete findings.
- [ ] Run CI on the approved commit; retain run URL/artifact. Local SDK 10 targeting net8.0 does not prove Linux SDK 8 CI behavior.
- [ ] Eilmar/Lana ownership and triage of numeric validation / DEF-010; team release decision.
- [ ] Preserve the six master-register NOT RUN cases until their final acceptance criteria have separate evidence. HTTP smoke overlap alone does not automatically close them.
- [ ] Shirui's SQA-19 observer approval/provenance review; SQA-18 performance criterion approval. Neither was reassessed by this retest.

**SQA-20: PASS on automated technical retest.** SQA-18 PASS and SQA-19 provisional PASS/caveats are preserved. The accessibility branch is ready for review; this is not overall release approval. All owned test hosts are stopped after evidence capture. No commit/push was made.

Suggested commit title: `fix: resolve SQA-20 accessibility issues after merge`

Suggested description: Associate endpoint/defect form labels, improve API URL/export contrast and keep severity badges compact. Retain original FAIL evidence and add Chrome/Edge retest plus full .NET/isolated SQLite regression evidence. Record the separate pre-existing numeric validation failure and preserve SQA-18/SQA-19 limitations.
