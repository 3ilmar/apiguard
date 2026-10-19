# Reproducible SQA-20 findings

Execution: 19 October 2026 NZDT; genuine installed Chrome `155.0.8059.80` and Edge `155.0.4283.62`; desktop 1440 × 900 and narrow 375 × 812; isolated application `http://localhost:51920/`. All findings below are automated/AI-assisted technical observations; no human participant response is inferred. Application source was not modified.

## SQA20-A11Y-01 — visible labels are not programmatically associated

| Field | Observed value |
|---|---|
| Status | OPEN / reproduced in both browsers at both viewports |
| Product severity | High — controls lack dependable names for assistive technology |
| Scanner impact | axe `critical` for `label` and `select-name`; this is an accessibility rule impact, not a Critical security vulnerability |
| Pages | Both API details pages and `/Defects` |
| Actual | Adjacent visible labels exist, but omit the `for` association and do not wrap their controls; controls lack an alternative explicit accessible name |
| Expected | Every form control has a persistent associated label/accessible name conveying its purpose |

Reproduction:

1. Open `/Apis/Details/20200000-0000-0000-0000-000000000001` or `/Apis/Details/20200000-0000-0000-0000-000000000002`.
2. Inspect Add endpoint controls and their labels, or run the archived axe wrapper. Examples: `#NewEndpoint_Name`, `#NewEndpoint_ExpectedStatusCode`, `#NewEndpoint_MaxResponseTimeMs`, `#NewEndpoint_RequiresAuthentication`, `#NewEndpoint_Method`.
3. Open `/Defects`. For all four defect cards, inspect `select[name="status"]`, `select[name="priority"]` and `input[name="assignedTo"]` and their adjacent labels.

Deduplicated across the two viewports, each browser's axe results contain **22 control instances**: 12 `label` targets (four in each API form and four Assigned to controls), plus 10 `select-name` targets (one Method per API form plus eight Status/Priority controls). The conservative DOM audit contains **26 instances** lacking associated names, including four additional placeholder-backed Path/Expected content type inputs across the two API forms. Axe accepts those placeholders as a fallback; placeholders do not establish an association with the visible label. The measurements overlap and must not be added as distinct defects.

Evidence:

- Chrome: [orders axe](chrome/axe-desktop-orders.json), [catalogue axe](chrome/axe-desktop-catalogue.json), [Defects axe](chrome/axe-desktop-defects.json), [narrow Defects DOM](chrome/layout-narrow-defects.json).
- Edge: [orders axe](edge/axe-desktop-orders.json), [catalogue axe](edge/axe-desktop-catalogue.json), [Defects axe](edge/axe-desktop-defects.json), [narrow orders DOM](edge/layout-narrow-orders.json).
- Corresponding `axe-narrow-...json` files retain the independent narrow scans. Physical focus results remain available in each page's `keyboard-...json`; receiving focus does not correct the missing name.

Recommended correction: use MVC `label asp-for` or a matching unique `for`/`id` pair for each endpoint field. Give every repeated defect-card field a unique id and its own associated label. Preserve meaningful visible labels and verify the resulting accessible names with axe and browser accessibility information. Apply and retest through a separately authorized application fix; this evidence run leaves the issue reproducible.

## SQA20-A11Y-02 — normal text contrast below 4.5:1

| Field | Observed value |
|---|---|
| Status | OPEN / reproduced in both browsers at both viewports |
| Product severity | Medium |
| Scanner impact | axe `serious`, `color-contrast` |
| Expected | At least 4.5:1 contrast for the measured normal text |
| Unique affected targets per browser | Three: one base URL on each API details page, plus the Defects CSV link |

| Page / selector | Foreground | Background | Size | Actual ratio | Required |
|---|---|---|---|---:|---:|
| Orders detail, `div > code` | `#d63384` | `#f6f8fb` | 14px normal | 4.23:1 | 4.5:1 |
| Catalogue detail, `div > code` | `#d63384` | `#f6f8fb` | 14px normal | 4.23:1 | 4.5:1 |
| Defects, `.btn-outline-secondary` | `#6c757d` | `#f6f8fb` | 16px normal | 4.4:1 | 4.5:1 |

Reproduction: open the API detail URLs or `/Defects`, leave the target in its resting state, and run axe with the recorded configuration. Read `color-contrast` node data rather than estimating the ratio from a screenshot.

Evidence: [Chrome orders](chrome/axe-desktop-orders.json), [Chrome catalogue](chrome/axe-desktop-catalogue.json), [Chrome Defects](chrome/axe-desktop-defects.json), [Edge orders](edge/axe-desktop-orders.json), [Edge catalogue](edge/axe-desktop-catalogue.json), [Edge Defects](edge/axe-desktop-defects.json). The same targets are retained in narrow scan results.

Recommended correction: adjust foreground/background combinations so normal-state text meets 4.5:1, and recheck hover/focus/active states and focus visibility after the change. Keep the failure open until actual retest evidence is available.

## SQA20-UI-01 — stretched severity badges on Defects

| Field | Observed value |
|---|---|
| Status | OPEN / reproducible cosmetic issue |
| Product severity | Low |
| Page | `/Defects` |
| Conditions | Both browsers; desktop and narrow layouts |
| Actual | The flex-row severity badge stretches vertically with the neighboring defect title/description; badge height exceeds twice its normal line height |
| Usability assessment | Severity text remains readable and geometrically contained in saved screenshots/DOM; no obstruction of adjacent controls was measured |
| Expected | A compact text badge with height appropriate to its content |

Reproduction: open `/Defects` at 1440 × 900, then at 375 × 812. Inspect the gray Critical/High/Medium/Low badges beside each defect description and compare their height with their line height. This issue is not limited to the narrow viewport.

Evidence: [Chrome desktop screenshot](chrome/desktop-defects-full.png), [Chrome narrow screenshot](chrome/narrow-defects-full.png), [Chrome narrow measurement](chrome/layout-narrow-defects.json), [Edge desktop screenshot](edge/desktop-defects-full.png), [Edge narrow screenshot](edge/narrow-defects-full.png), [Edge desktop measurement](edge/layout-desktop-defects.json).

Recommended correction: give the badge an appropriate flex self-alignment, for example `align-self-start`, then verify wrapping/spacing. The current cosmetic issue alone does not invalidate the measured responsive geometry or severity-text/count criteria.

## Investigated raw flag — scrollable region keyboard focus

Axe's narrow API scans include `scrollable-region-focusable`. This is retained as a raw scanner finding, but **not confirmed as a product defect in the tested browser versions**. Genuine Tab and Shift+Tab reached the overflowing endpoint-table wrappers despite `tabIndex === -1` and no explicit `tabindex` attribute; ArrowRight changed scrollLeft from **0 to 40** in both browsers on both API detail pages. No DOM focus call or tabindex mutation was used to establish that behavior.

Evidence: [settled verification](narrow-keyboard-settled-verification.json), [Chrome settled orders](chrome/narrow-settled/keyboard-narrow-orders.json), [Chrome settled catalogue](chrome/narrow-settled/keyboard-narrow-catalogue.json), [Edge settled orders](edge/narrow-settled/keyboard-narrow-orders.json), [Edge settled catalogue](edge/narrow-settled/keyboard-narrow-catalogue.json), and corresponding raw `axe-narrow-orders.json`/`axe-narrow-catalogue.json` files in each browser directory.

The first narrow follow-up sampled focus position before Bootstrap smooth scrolling settled. Its transient visibility flags and screenshots remain in the original evidence paths. The settled follow-up waits at least 180 ms, checks stable geometry and passes forward/reverse navigation without missing controls or focus failures. This correction concerns measurement timing, not a product change.

## Outcome boundary

The 16 category executions report **16 PASS, 0 FAIL, 0 BLOCKED, 0 NOT RUN** for their stated technical checks. The additional confirmed label and contrast failures make **overall automated SQA-20 FAIL**. The cosmetic finding remains open. Neither an automated scan nor these screenshots prove full WCAG conformance, representative-user comprehension or release readiness.
