# SQA-19 human usability evidence and session procedure

Work item: **SQA-19** · Test case: **TC-12** · Requirement: **NFR-USA-01**

**Current outcome: SQA-19 PASS — observer approved; final scenario decision 5/5 PASS.** Shirui explicitly confirmed in this conversation that she reviewed and approved Eilmar's assessment, including all five scenario outcomes. This authorises recording her decision; no signature or precise approval timestamp was supplied or inferred. The [unchanged Eilmar submission](observation-sheet-session-eilmar.md) records session `SQA19-20261019-EILMAR`, tester `EILMAR-P01`, observer Shirui, on 19 October 2026 from 15:00 to 16:00 NZDT. The [final observer assessment](observer-assessment-session-eilmar.md) records the five accepted interpretations and explanations, approval provenance and continuing evidence limitations.

The tester reports forming the interpretations before asking ChatGPT to compare and lightly polish their wording. The supplied text is assisted wording, not a fully unassisted verbatim transcript; original unpolished responses are not supplied. No session-specific screenshot was identified. The existing [18 October screenshot](automated-setup/dashboard-preparation.jpg) remains setup evidence only. The [AI-assisted heuristic inspection](../SQA-20/playwright-20261019T035608Z/sqa19-heuristic-review.md) and automated message tests are supplementary evidence, distinct from the human submission.

The submitted observation sheet is preserved byte-for-byte, including its historical blank observer fields and pending labels. The separate final observer assessment supersedes those fields for the current outcome; the tester's text has not been rewritten. The [finalisation verification record](final-approval-20261019/README.md) preserves the earlier proposed assessment and records automated document checks. The blank master sheets, facilitator key and historical automated preparation records remain unchanged. The original NOT RUN figures below describe the reusable template/preparation baseline, not the current reported execution. Synthetic results demonstrate application wording, not live endpoint outcomes. These are QA evidence and procedures, not submitted assessment report prose.

After integrating main's SQLite and demo-RBAC changes, the separate fixture executable explicitly selects its private in-memory store before any database initialization. The normal application retains SQLite and default Viewer role enforcement. Role-selector submissions are blocked by this read-only host. The [integration ledger](../integration-20261019T092140Z/README.md) records fresh technical validation without changing the submitted human answers or granting observer approval.

## Environment and scope

- Dashboard URL: **http://localhost:51919/**
- Start/restart the isolated development host from the repository root: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Sqa19.ManualHost\Start-Sqa19.ps1`.
- The original background instance was stopped before integrating main. A fresh instance passed the integration HTTP smoke and was then stopped; no fixture host is left running by that validation. Start it using the launcher above when needed. To stop a running instance, use `powershell -NoProfile -File .\tools\Sqa19.ManualHost\Stop-Sqa19.ps1`; this checks the listening process belongs to this repository's SQA-19 host. For a foreground launch, Ctrl+C also stops it. Stop before restarting; the launcher never terminates an existing listener.
- Automated setup evidence is in [automated-setup/README.md](automated-setup/README.md). To repeat its HTTP checks while the host is running: `powershell -NoProfile -File .\tools\Sqa19.ManualHost\Verify-Sqa19.ps1`. A successful setup check is not a human usability result.
- The host uses the application's dashboard and five synthetic failed results. It is separate from any normally running application instance. Do not run checks or register APIs during this session.
- The fixture order in **Recent checks**, top to bottom, is SQA19-01 through SQA19-05. Verify that five FAIL rows are visible before the participant joins. If the rows are missing or reordered, stop and correct preparation before collecting answers.
- Seeding is local and synthetic. This usability session does not execute external API requests, validate an endpoint's real availability/security, or prove production readiness.
- Restarting the isolated host restores its fixture data; retain observation files and screenshots separately. Keep any original application instance and its data separate.
- This host allows reading the original dashboard and stylesheet only. Other navigation links are outside this session; writes are rejected. Five check results are seeded directly without defect-lifecycle processing, so the dashboard's zero defects are fixture setup, not a statement that failed checks would produce no defects during a normal run. Displayed check times are synthetic ordering times at startup; 1001 ms is supplied test data, not a measured request duration.
- The unchanged application layout fetches Bootstrap CSS from jsDelivr. Confirm styling has loaded before the session and record browser/zoom conditions; this is a normal stylesheet fetch, not an API check.

## Reusable session record — blank master

| Field | Value to record |
|---|---|
| Session identifier | |
| Tester identifier (use a pseudonymous ID) | |
| Representative role and relevant API/testing familiarity | |
| Facilitator/observer identifier | |
| Session start date/time with timezone | |
| Session end date/time with timezone | |
| Software commit SHA | |
| Working branch and uncommitted source state/diff reference | |
| .NET SDK and runtime version | |
| OS/version | |
| Browser/version | |
| Viewport size and browser zoom | |
| Test data | Five synthetic failed results in the isolated SQA-19 host |
| Test URL | http://localhost:51919/ |
| Automated setup/verification evidence reference | |
| CI run reference (or N/A — local manual session) | |
| Observation file and screenshot folder | |

Record the current software state, rather than attributing uncommitted preparation changes to the base commit. Setup/build/browser checks are separate from the representative human session.

## Facilitator procedure

1. Start the isolated host and open the URL. Confirm the page is the API Quality Dashboard with all five failures under Recent checks. Capture a preparation screenshot if useful and label it **setup evidence**, not participant evidence.
2. Copy [observation-sheet.md](observation-sheet.md) to a new session-specific file; use [observation-sheet.csv](observation-sheet.csv) if a table is more convenient. Keep the blank originals. Fill the environment/session record above in the session record copy. Store screenshots under a session-specific subdirectory.
3. Before the participant joins, read [facilitator-key.md](facilitator-key.md). Keep this key, fixture source, this procedure and any expected-answer material out of the participant's view. Give the participant only the application and [participant-instructions.md](participant-instructions.md).
4. Read the participant script exactly. For each scenario, direct attention only to its row in top-to-bottom order. Allow reading, scrolling and browser zoom. Do not explain HTTP codes, thresholds, authentication, content types or timeout terminology; do not demonstrate an answer, point out a missed value, or confirm correctness between scenarios.
5. Record each interpretation, cited evidence and wording comment **verbatim**, including uncertainty. A blank is not evidence of understanding. Use “No response” only if that is an observer note; do not present it as the tester's words. Preserve original notes/recordings; obtain any recording agreement required by the course/session before recording.
6. If clarification is requested, say: “Please describe your current understanding from what is shown. It is okay to say you are unsure.” Record the request and this prompt separately. If any additional assistance is given, record its exact wording and timing; do not count a coached answer as an unaided pass. Interruptions, display failures or a participant unable to continue can be recorded as BLOCKED with a reason.
7. Complete the separate observer assessment after collecting the answer. Compare meaning and cited evidence with the facilitator key; accept equivalent everyday wording. Do not rewrite the participant answer to make it match. Record FAIL for an incorrect/insufficient unaided interpretation and identify the missing or mistaken evidence. A clearly understood failure with wording criticism can still PASS; retain the criticism as a usability finding.
8. Capture screenshot references for the rows as actually displayed during the session and link notes to those references. A dashboard screenshot alone establishes displayed content, not participant understanding. Record any screenshot changes, browser changes or assistance.
9. Retain corrections as dated addenda alongside the unchanged original answer. Any later explanation/debrief happens after all five answers are captured; keep it distinct from unaided session evidence.
10. Complete the execution totals and outcome below only from the recorded session. Record follow-up defects with the affected scenario, verbatim feedback and evidence. Do not mark SQA-19 PASS based on automated setup checks.

## Historical preparation outcome — blank master

| Measure | Initial value | Actual after session |
|---|---:|---|
| Planned scenarios | 5 | |
| Executed scenarios (PASS + FAIL) | 0 | |
| Passed scenarios | 0 | |
| Failed scenarios | 0 | |
| Blocked scenarios | 0 | |
| Not-run scenarios | 5 | |

The final five scenario statuses must total five across PASS, FAIL, BLOCKED and NOT RUN. Use this session rule: TC-12/SQA-19 passes only when all five displayed messages are understood and their observed evidence identified without coaching. Any failed scenario makes this session FAIL; if none fail but one remains BLOCKED or NOT RUN, the session is incomplete. Overall NFR-USA-01 coverage remains limited to these five messages; this session does not establish every possible failure message is usable.

| Outcome field | Actual entry |
|---|---|
| SQA-19 / TC-12 session status | NOT RUN |
| Assessment made by / date/time | |
| Usability issues and linked defect references | |
| Limitations or assistance/deviations | |
| Follow-up session/evidence reference | |

