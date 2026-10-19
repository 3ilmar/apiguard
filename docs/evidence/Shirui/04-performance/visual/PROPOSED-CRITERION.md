# SQA-18 proposed operational acceptance criteria v1

Proposed by the user in this chat on 18 October 2026. **Not formally approved by the team.** The existing NFR-PERF-01 in `docs/Requirements.md` remains provisional/TBC; this proposal supplies a measurable local test procedure without changing that requirement or the team's RTM.

1. Exactly 2,000 deterministic check results.
2. Correct dashboard statistics and severity distribution.
3. Measure navigation start until the populated dashboard is visually ready, including stylesheet loading, layout and paint readiness.
4. Run five independent browser reload measurements under consistent conditions.
5. PASS if all five visual-readiness measurements are below 2,000 ms and the dashboard values are correct.

Operational endpoint used for this run: the browser's navigation-relative `performance.now()` immediately after a viewport screenshot has been captured and returned. Before capture, require document load, both stylesheet objects plus successful HTTP 200 responses and computed styles, fonts ready, correct cards/severity/lists, all content inside the fixed viewport, and stable element geometry across three consecutive animation-frame callbacks. Inspect each retained screenshot before assigning PASS. This measures a **conservative verified visual-readiness upper bound**, including screenshot/automation overhead; it does not claim to identify the first complete frame or equate Network Finish/FCP/LCP with complete readiness.

Each retained sample is an actual `page.reload()` in its own new browser context/page following an unmeasured preparation navigation. Cache is cleared and disabled, service workers blocked, CPU rate 1 and network throttling disabled; viewport 1440 × 1600, scale 1, locale en-NZ and timezone Pacific/Auckland are held constant. Server is already warmed. Five measured reloads are retained without discarding slow/failed samples. No PASS is assigned until measurements and screenshot review complete.
