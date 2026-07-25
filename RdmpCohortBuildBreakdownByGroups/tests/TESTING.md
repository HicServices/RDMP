# Tests for RdmpCohortBuildBreakdownByGroups

`CohortBuildBreakdownByGroupsTests.cs` in this folder is a copy of the canonical test file so the suite
is reviewable alongside the plugin source. The canonical version (which is what was executed) lives on
the fork feature branch:

https://github.com/mtinti/RDMP/blob/feature/cohort-healthboard-breakdown/Rdmp.Core.Tests/CohortCreation/CohortBuildBreakdownByGroupsTests.cs

These tests are NOT wired into this repository's solution or CI (the plugin package is additive and the
GitHub checks on this PR cover CodeQL and links only). To run them, the test file sits in
`Rdmp.Core.Tests/CohortCreation/` on a checkout where the four engine sources are present in
`Rdmp.Core` (the fork feature branch above has exactly that layout).

## What the suite covers

No-database unit tests:
- `GroupLookup_RejectsDuplicateAndReservedLabels` - lookup validation (duplicate labels, reserved header collisions)
- `IsTransformedIdentifier_WhitelistsPlainColumnReferencesOnly` - identifier whitelist incl. alias handling
  ("UPPER(chi) AS chi" rejected, "[db]..[tbl].[chi] AS PatientId" accepted)
- `PostgreSqlSameDatabaseError_NormalizesWrappedNames` - wrapped-vs-plain accept, different reject,
  case-sensitivity reject, blank reject (via the FAnsi PostgreSql syntax helper)
- `SetOperationSql_MapsExceptToMinusOnOracle` - per-DBMS set operators
- `CleanName_StripsStackedCicPrefixes` - display-name cleaning
- `Split_SeparatesRecognisedOtherAndNotKnown` and `ToCsv_WideHeaderAndMetricRows` - report projection

Database integration tests:
- `BuildBreakdown_Fixture_NationalAndThreeBoards(MicrosoftSQLServer)` and `(PostgreSql)` - the full
  end-to-end deterministic fixture: a top EXCEPT over an inclusion INTERSECT (Registry 120 members
  intersect Demography 100 = 100) minus four exclusion sets (removing 20/15/5/2 -> final cohort 58),
  three groups partitioning the cohort via a synthetic z_hb_lookup table, plus 20 registry-only
  patients (NotKnown) and an enabled-but-empty container (skipped, matching RDMP under non-strict
  validation). Asserts the key national and per-group counts and cumulatives explicitly (national
  100 -> 80 -> 65 -> 60 -> 58; Tayside 50 -> 40 -> 32 -> 30 -> 29; etc), asserts the unfiltered column
  equals RDMP's own CohortCompiler counts, and asserts groups + Other + NotKnown == Total on every row.
  The query cache is created in the same database as the data (required on PostgreSQL).
- `GroupLookup_LoadFrom_RejectsDuplicateKeys` - duplicate lookup keys rejected at load

## How to run (macOS, docker)

```bash
# SQL Server platform + scratch databases (one-time)
bash mac-test-env/setup.sh

# optional: PostgreSQL (and Oracle) containers + test config lines
bash mac-test-env/multidb.sh up

dotnet build Rdmp.Core.Tests/Rdmp.Core.Tests.csproj -c Debug -p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"'
dotnet test Rdmp.Core.Tests/Rdmp.Core.Tests.csproj -c Debug --no-build \
  -p:WarningsNotAsErrors='"NU1902;NU1903;NU1904"' \
  --filter "FullyQualifiedName~CohortBuildBreakdownByGroups"
```

Without the PostgreSql line in `Tests.Common/TestDatabases.txt` the PostgreSql fixture case skips.

## Last executed results (2026-07-15, macOS arm64)

- SQL Server 2022 (docker, amd64 under Rosetta): 10/10 passed, fixture ~11 s
- PostgreSQL 16 (docker, native arm64): fixture case passed end-to-end, ~5 s
- Oracle: not runnable - RDMP's own `QueryCachingCrossServerTests.Create_QueryCache(Oracle)` fails
  upstream ("Table name 'CachedAggregateConfigurationRe' is too long for the DBMS (Oracle supports
  maximum length of 30)"): FAnsi caps Oracle identifiers at 30 characters while the cache bookkeeping
  table name is 35 (modern Oracle allows 128; verified the table creates fine via sqlplus directly).
