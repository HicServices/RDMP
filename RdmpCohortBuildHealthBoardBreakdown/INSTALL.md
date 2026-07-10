# RdmpCohortBuildHealthBoardBreakdown plugin (RDMP 9.2.3)

Reproduces the Cohort Builder's per-set / per-container count tree (the FinalCount and cumulative
running totals shown as UNION/INTERSECT/EXCEPT are applied) **split by region** (e.g. Scottish health
board), plus an unfiltered national total. Saved as a wide CSV.

Built against the **released RDMP 9.2.3**. Do not use on a different major.minor RDMP.

## How it works (cache-only, cross-server safe)

It builds the cohort **once** (populating the query cache), then recomposes every count point from the
cached per-set identifier tables and splits each by the region column with one GROUP BY per node. It
never re-runs the source catalogues per region, and never touches the source servers after the single
build, only the query-cache server (which is why the demography catalogue must be on the same server as
the query cache). The region-to-name/node mapping is read from a user-supplied lookup table, so nothing
is hard-coded.

## Requirements

- The cohort identification configuration must have a **query caching server** configured (the
  breakdown works only on cached results; it refuses otherwise).
- The **demography catalogue** (with a CHI IsExtractionIdentifier column and the region column) must be
  on the **same SQL server as the query cache** (the command checks and refuses if not).
- A **lookup table** mapping each region code to a name and node, with columns:
  `Region` (the code as it appears in the demography data), `HB_Name` (display name), `SafeHaven_Region`
  (grouping node, may be NULL). An `HB_Code` column may be present but is ignored. One row per code.

## Install

**GUI:** RDMP desktop, Plugins node, *Add Plugin* (or drag `RdmpCohortBuildHealthBoardBreakdown.rdmp`
onto it), restart RDMP. **Or** drop the `.rdmp` next to `rdmp.exe` /
`ResearchDataManagementPlatform.exe`.

Confirm (CLI): `rdmp.exe cmd ListSupportedCommands` lists `ExportCohortBuildHealthBoardBreakdown`.

## Use

**GUI:** right-click a Cohort Identification Configuration, choose the export command. It prompts for the
demography catalogue, the region column, and the lookup table.

**CLI:** the inputs are RDMP objects, mapped by id:
```
rdmp.exe cmd ExportCohortBuildHealthBoardBreakdown \
    CohortIdentificationConfiguration:<id> Catalogue:<demography> ColumnInfo:<region> TableInfo:<lookup> out.csv
```
`out.csv` and the timeout are optional; the object arguments are required (no hard-coded defaults).

## Output (wide format)

One row per count point (name written once), a `Metric` column (Final and Cumulative), a `Total` column
(RDMP's national number), one column per region recognised by the lookup, then `Other` (present codes the
lookup does not recognise) and `NotKnown` (not in demography, or NULL region). The column header is
repeated above two percentage rows: `% of final cohort` and `% of demography` (each region's share of the
whole demography population, a cohort-vs-population sanity check). Regions + Other + NotKnown reconcile to
Total on every row.

## Validation

Verified end-to-end on a deterministic synthetic fixture (top EXCEPT over an inclusion INTERSECT minus
four exclusion sets, cohort partitioned across regions, driven by a synthetic lookup table): every
national and per-region FinalCount and cumulative was asserted cell-by-cell, the unfiltered column equals
RDMP's own CohortCompiler counts, and the regions sum to national at every node.
