# Actual SQA-20 screenshots

**NOT RUN — no manual screenshots captured.** Store only images actually captured during the user's Chrome or Microsoft Edge session.

Actual automated browser screenshots are stored with their execution: [Chrome](../playwright-20261019T035608Z/chrome/) and [Edge](../playwright-20261019T035608Z/edge/). They are real Playwright captures, explicitly separate from human evidence.

Use `screenshots/<session-id>/chrome/` and `screenshots/<session-id>/edge/`. Suggested filenames: `desktop-dashboard.png`, `focus-navigation.png`, `375-dashboard.png`, `375-api-details.png`, `375-defects.png` and `download-confirmation.png`. Create these files only when the corresponding capture exists.

Link each capture from the observation row with the browser version, actual date/time, viewport and zoom. Include focused controls in focus evidence. Do not use a generated image, automated HTTP document, setup screenshot or another browser's capture as a manual Chrome/Edge result. Preserve original captures; identify any cropped or annotated copies separately.
