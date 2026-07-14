# RdmpCohortBuildBreakdownByGroups (RDMP 9.2.3 plugin)

Reproduces the Cohort Builder's per-set / per-container count tree (the `FinalCount` and cumulative
running totals shown as UNION/INTERSECT/EXCEPT are applied) **split by an arbitrary group column**
(e.g. Scottish health board, GP practice, age band), plus an unfiltered national total, and writes it
to a wide CSV. Groups are labelled and ordered by a user-supplied lookup table; nothing is hard-coded
in the engine (the SHARE-specific names live only in the plugin's preset file).

This folder is a self-contained package: the ready-to-install plugin, install/usage notes, and the source.

## Contents

| Path | What it is |
|---|---|
| `RdmpCohortBuildBreakdownByGroups.rdmp` | the built plugin (drop into RDMP / add via the Plugins node) |
| `INSTALL.md` | install + usage (GUI right-click and CLI), including the lookup-table expectations |
| `src/` | plugin source (command, report, group lookup, models, SHARE preset, UI hook, csproj, nuspec) |

## How it works (in one paragraph)

It builds the national cohort once (which populates RDMP's query cache), then recomposes every count
point purely from the cached per-set identifier tables and splits each by the group column with one
`GROUP BY` per node, all groups at once. No per-group rebuild, and no hits on the source catalogues
after the single build (so it is cross-server safe). Inputs are four `ColumnInfo` objects: the group-by
column (its table is the reference table, whose single IsExtractionIdentifier column is the join key to
the cohort) and the lookup table's key/label/optional-grouping columns. Requires a query-caching server,
with the reference table on the same server as the cache.

## The SHARE preset

`src/SharePreset.cs` is deliberately the only place any deployment-specific name lives: it resolves
`SHARE_Demography`.`Region` and `z_hb_lookup`.`Region`/`HB_Name`/`SafeHaven_Region` by name at runtime.
The GUI offers two right-click entries on a cohort identification configuration: "(SHARE preset)"
(one click; prompts only for anything the preset cannot resolve) and "(choose inputs)" (always prompts).

## Output

Wide CSV: one row per count point (name once), a `Metric` column (Final + Cumulative), a `Total`
column (RDMP's national number), one column per group recognised by the lookup, then `Other` (present
codes the lookup does not recognise) and `NotKnown` (not in the reference table / null group). The
column header is repeated above a `% of final cohort` row and a `% of reference population` row (each
group's share of the whole reference table, for a cohort-vs-population sanity check). Groups + Other +
NotKnown reconcile to Total on every row.

## Validation

Verified end-to-end against a deterministic synthetic fixture (top EXCEPT over an inclusion INTERSECT
minus four exclusion sets, driven by a synthetic z_hb_lookup table): every national and per-group
`FinalCount` / cumulative is asserted cell-by-cell, the unfiltered column equals RDMP's own
`CohortCompiler` counts, and the groups sum to national at every node.

## Build from source (optional)

`src/` builds against RDMP 9.2.3 (`Rdmp.Core`, `Private=false`). Package the resulting DLL + nuspec into
a `.rdmp` zip (`<nuspec>` at root, DLL under `lib/net10.0/`).
