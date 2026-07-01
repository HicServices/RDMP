# RdmpCohortBuildHealthBoardBreakdown plugin (RDMP 9.2.3)

Reproduces the Cohort Builder's per-set / per-container count tree (the FinalCount and cumulative
running totals shown as UNION/INTERSECT/EXCEPT are applied) **split by Scottish health board**, plus
an unfiltered baseline. Saved as a long-format CSV.

Built against the **released RDMP 9.2.3**. Do not use on a different major.minor RDMP.

## How it works (cache-only, cross-server safe)

It builds the cohort **once** (populating the query cache), then recomposes every count point from the
cached per-set identifier tables and splits each by `SHARE_Demography.Region` with one GROUP BY per
node. It never re-runs the source catalogues per board, and never touches the source servers after the
single build — only the query-cache server (which is why the demography catalogue must be on the same
server as the query cache).

## Requirements

- The cohort identification configuration must have a **query caching server** configured (the
  breakdown works only on cached results; it refuses otherwise).
- `SHARE_Demography` (with a `Region` health-board cipher column and a CHI IsExtractionIdentifier
  column) must be on the **same SQL server as the query cache** (the command checks and refuses if not).

## Install

**GUI:** RDMP desktop → Plugins node → *Add Plugin* (or drag `RdmpCohortBuildHealthBoardBreakdown.rdmp`
onto it) → restart RDMP. **Or** drop the `.rdmp` next to `rdmp.exe` /
`ResearchDataManagementPlatform.exe`.

Confirm (CLI): `rdmp.exe cmd ListSupportedCommands` lists `ExportCohortBuildHealthBoardBreakdown`.

## Use

**GUI:** right-click a Cohort Identification Configuration → *Export ... Build Health Board Breakdown*
→ choose a CSV path.

**CLI:**
```
rdmp.exe cmd ExportCohortBuildHealthBoardBreakdown CohortIdentificationConfiguration:<id> out.csv "SHARE_Demography" "Region"
```
Args after the CIC are optional (defaults: `<cic>-build-healthboard.csv`, `SHARE_Demography`, `Region`).

## Output (long format, board-grouped)

Columns: `Board, Node, Order, Type, Name, Container, SetOperation, FinalCount, CumulativeCount`. The
`Unfiltered` tree first (RDMP's own numbers), then each health board's full tree (boards partition the
cohort, 1 patient ↔ 1 board), then an `Unknown` board (patients with no / unmapped region). `FinalCount`
is the node's own count; `CumulativeCount` is the running total within the parent container (blank for
the first child, as in the UI). Boards (+ Unknown) reconcile to the unfiltered total at every node.

## Validation

Verified end-to-end on a deterministic synthetic fixture (top EXCEPT over an inclusion INTERSECT minus
four exclusion sets, cohort partitioned across 3 boards): every national and per-board FinalCount and
cumulative was asserted cell-by-cell, the unfiltered column equals RDMP's own CohortCompiler counts, and
the boards sum to national at every node.
