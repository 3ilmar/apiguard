# Actual SQA-20 browser downloads

**NOT RUN — no manual browser CSV downloads collected.** Save original attachments downloaded in the user's session under `<session-id>/chrome/` and `<session-id>/edge/`.

Actual automated browser downloads were collected independently in [Chrome](../playwright-20261019T035608Z/chrome/downloads/) and [Edge](../playwright-20261019T035608Z/edge/downloads/). [Strict CSV validation](../playwright-20261019T035608Z/csv-validation.json) records their hashes and checks; they are distinct from earlier HTTP-client setup samples.

Each browser should supply an `api-check-results-*.csv` and a `defects-*.csv`. Record the actual names, download actions and inspection observations in the session sheet. Expected fixture structure is 15 columns/8 result records and 21 columns/4 defect records. A CSV-aware viewer should retain comma/quote-containing text in its original field.

Keep original downloads unchanged. Save any imported, annotated or resaved files separately and identify the tool used. HTTP setup exports are preparation evidence and do not demonstrate a successful browser download.
