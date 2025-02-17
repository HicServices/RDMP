# DQE Post Load Runner

The DQE post-load runner can be used to automatically perform a DQE update once a data load completes.
The runner attempts to reuse any existing DQE results that have been unaffected by the data load, however this process can still be slow if the catalogue data is large and/or complex.

## Requirements
The DQE post-load runner requires an existing DQE result to exist, otherwise it will fail.

## Configuration
The runner makes a number of queries to the database, the timeout for these commands is configurable via the timeout option.


