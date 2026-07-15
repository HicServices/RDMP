# RdmpCohortBuildBreakdownByGroups plugin (RDMP 9.2.3)

Reproduces the Cohort Builder's per-set / per-container count tree (the FinalCount and cumulative
running totals shown as UNION/INTERSECT/EXCEPT are applied) **split by an arbitrary group column**
(e.g. Scottish health board), labelled and ordered by a user-supplied lookup table. Saved as a wide CSV.

Built against the **released RDMP 9.2.3**. Do not use on a different major.minor RDMP.

## How it works (cache-only, cross-server safe)

It builds the cohort **once** (populating the query cache), then recomposes every count point from the
cached per-set identifier tables and splits each by the group column with one GROUP BY per node. The
cohort-set source queries are never re-run: after the single build only the query-cache server is
queried (each node joins the reference table, which is why it must be on the same server as the query
cache). Nothing is hard-coded in the engine; the SHARE names live only in the plugin's preset.

## Inputs (4 columns; the tables are derived)

- **group column** - the column to break the counts down by (e.g. `SHARE_Demography.Region`). Its table
  is the reference table and must contain exactly one IsExtractionIdentifier column (the CHI), which is
  the join key to the cohort.
- **lookup key column** - the group code as it appears in the group column (e.g. `z_hb_lookup.Region`).
  Its table is the lookup table.
- **lookup label column** - the display name per code (e.g. `z_hb_lookup.HB_Name`).
- **lookup grouping column** (optional) - a higher grouping used to order the output columns (e.g.
  `z_hb_lookup.SafeHaven_Region`; NULL values allowed). Being optional it is never prompted for in
  the GUI; supply it via the preset or the CLI.

Codes present in the data but absent from the lookup go to `Other`; patients missing from the reference
table (or with a NULL group) go to `NotKnown`.

## Requirements

- **One-to-one relationship**: each identifier must belong to AT MOST ONE group in the reference
  table (e.g. one patient, one health board). Multi-group membership double-counts patients and
  invalidates the NotKnown residual and the reference denominator.

- The cohort identification configuration must have a **query caching server** configured.
- The reference table must be on the **same SQL server as the query cache** (checked; refuses if not).

## Install

**GUI:** RDMP desktop, Plugins node, *Add Plugin* (or drag `RdmpCohortBuildBreakdownByGroups.rdmp` onto
it), restart RDMP. **Or** drop the `.rdmp` next to `rdmp.exe` / `ResearchDataManagementPlatform.exe`.

Confirm (CLI): `rdmp.exe cmd ListSupportedCommands` lists `ExportCohortBuildBreakDownByGroups`.

## Use

**GUI:** right-click a Cohort Identification Configuration. Two entries:
- *Export Build Breakdown By Groups (SHARE preset)* - resolves `SHARE_Demography`.`Region` and
  `z_hb_lookup`.`Region`/`HB_Name`/`SafeHaven_Region` by name; prompts only for anything not found.
- *Export Build Breakdown By Groups (choose inputs)* - prompts for all four columns.

**CLI:** the inputs are RDMP objects, mapped by id:
```
rdmp.exe cmd ExportCohortBuildBreakDownByGroups \
    CohortIdentificationConfiguration:<id> ColumnInfo:<group> ColumnInfo:<key> ColumnInfo:<label> ColumnInfo:<grouping> out.csv
```
`out.csv`, the grouping column and the timeout are optional.

## Output (wide format)

One row per count point (name written once), a `Metric` column (Final and Cumulative), a `Total` column
(RDMP's national number), one column per group recognised by the lookup, then `Other` and `NotKnown`.
The column header is repeated above two percentage rows: `% of final cohort` and `% of reference
population` (each group's share of the whole reference table, a cohort-vs-population sanity check).
Groups + Other + NotKnown reconcile to Total on every row.

## Validation

Verified end-to-end on a deterministic synthetic fixture (top EXCEPT over an inclusion INTERSECT minus
four exclusion sets, driven by a synthetic `z_hb_lookup` table with the 14 Scottish boards plus
E/O/K/X): the key national and per-group counts and cumulatives are asserted explicitly, the unfiltered column
equals RDMP's own CohortCompiler counts, and a reconciliation invariant (groups + Other + NotKnown
== Total) is asserted on every row.
