# Historical PT-01 coverage attempt — 18:37 NZDT

Dedicated uninstrumented PT-01 checks passed 2/2 and their original JSON/HTML records were successfully retained. The full-suite coverage attempt failed: Windows Application Control blocked the instrumented Web assembly (`0x800711C7`), causing 36 reported execution failures and incomplete test discovery. The runner preserves exit code 1; see test-full-suite.txt and execution-status.json. This is not a successful full-suite coverage run.

The [final validated execution ledger](../pt01-20261018T053834379Z-eb0590/README.md) records the subsequent 71/71 full-suite run without optional coverage instrumentation. No Windows security settings were changed.
