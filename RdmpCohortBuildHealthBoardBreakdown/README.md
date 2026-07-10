# RdmpCohortBuildHealthBoardBreakdown (RDMP 9.2.3 plugin)

Reproduces the Cohort Builder's per-set / per-container count tree (the `FinalCount` and cumulative
running totals shown as UNION/INTERSECT/EXCEPT are applied) **split by region** (e.g. Scottish health
board), plus an unfiltered national total, and writes it to a wide CSV. The region-to-name/node mapping
is read from a user-supplied lookup table (nothing is hard-coded).

This folder is a self-contained package: the ready-to-install plugin, install/usage notes, and the source.

## Contents

| Path | What it is |
|---|---|
| `RdmpCohortBuildHealthBoardBreakdown.rdmp` | the built plugin (drop into RDMP / add via the Plugins node) |
| `INSTALL.md` | install + usage (GUI right-click and CLI), including the lookup-table schema |
| `src/` | plugin source (command, report, region lookup, models, UI hook, csproj, nuspec) |

## How it works (in one paragraph)

It builds the national cohort once (which populates RDMP's query cache), then recomposes every count
point purely from the cached per-set identifier tables and splits each by the region column with one
`GROUP BY` per node, all regions at once. No per-region rebuild, and no hits on the source catalogues
after the single build (so it is cross-server safe). Inputs are RDMP objects (an `ICatalogue` for
demography, a `ColumnInfo` for the region column, a `TableInfo` for the lookup); requires a query-caching
server, with the demography catalogue on the same server as the cache.

## Output

Wide CSV: one row per count point (name once), a `Metric` column (Final + Cumulative), a `Total`
column (RDMP's national number), one column per region recognised by the lookup, then `Other` (present
codes the lookup does not recognise) and `NotKnown` (not in demography / null region). The column header
is repeated above a `% of final cohort` row and a `% of demography` row (each region's share of the whole
demography population, for a cohort-vs-population sanity check). Regions + Other + NotKnown reconcile to
Total on every row.

## Validation

Verified end-to-end against a deterministic synthetic fixture (top EXCEPT over an inclusion INTERSECT
minus four exclusion sets, driven by a synthetic lookup table): every national and per-region
`FinalCount` / cumulative is asserted cell-by-cell, the unfiltered column equals RDMP's own
`CohortCompiler` counts, and the regions sum to national at every node.

## Build from source (optional)

`src/` builds against RDMP 9.2.3 (`Rdmp.Core`, `Private=false`). Package the resulting DLL + nuspec into
a `.rdmp` zip (`<nuspec>` at root, DLL under `lib/net10.0/`).
