# Implementation plan — per-health-board cohort *build* breakdown (cache-only, approach A)

Companion to `BUILD-BREAKDOWN-FEASIBILITY.md` (decisions locked in §10 there). Reproduces the Cohort
Builder's per-set / per-container *total* + *cumulative* count tree, once per health board + an
unfiltered baseline, operating purely on the query cache, saved as a long-format CSV. Ships as a NEW,
self-contained plugin `RdmpCohortBuildHealthBoardBreakdown`. CIC-only.

## 0. Strategy

Develop + test in `Rdmp.Core` first (so the docker NUnit harness can exercise it, like the final-list
feature), then copy into the new plugin and build the 9.2.3 `.rdmp`. Reuses `HealthBoardLookup` and the
param-hoist / CSV / demography-resolution patterns from the final-list work.

## 1. Files

**Core (dev + test):**
- `Rdmp.Core/CohortCreation/CohortBuildHealthBoardBreakdownReport.cs` — long-format projection → CSV.
- `Rdmp.Core/CommandExecution/AtomicCommands/ExecuteCommandExportCohortBuildHealthBoardBreakdown.cs`
  — the command (CIC input).
- (reuses existing `Rdmp.Core/CohortCreation/HealthBoardLookup.cs`.)

**Tests:** `Rdmp.Core.Tests/CohortCreation/CohortBuildHealthBoardBreakdownTests.cs`.

**New plugin (phase 2):** `proposals/cohort-healthboard-breakdown/build-plugin/` —
`RdmpCohortBuildHealthBoardBreakdown.csproj`/`.nuspec`, a `PluginUserInterface` (right-click a CIC),
plus copies of `HealthBoardLookup.cs`, the report and the command.

## 2. Command flow (`Execute`)

```
ExecuteCommandExportCohortBuildHealthBoardBreakdown(
    IBasicActivateItems activator,
    CohortIdentificationConfiguration cic,
    FileInfo toFile = null,                         // <cic>-build-healthboard.csv
    string demographyCatalogue = "SHARE_Demography",
    string regionColumn = "Region",
    int timeout = 5000)
```

Construction-time `SetImpossible` guards:
- `cic` null or no root container.
- `cic.QueryCachingServer == null` → "needs a query caching server (cohort spans servers)".
- demography catalogue / `Region` / IsExtractionIdentifier column missing (same resolution as final-list).
- **co-location:** `cic.QueryCachingServer.Server` != `SHARE_Demography` `TableInfo.Server` → impossible
  with a clear message (the recompose+join runs on the cache server).

Execute:
1. **Build once / refresh cache + baseline.** `compiler = new CohortCompiler(activator, cic){
   IncludeCumulativeTotals = true }; new CohortCompilerRunner(compiler, timeout){ RunSubcontainers =
   true }.Run(token)`. This populates every per-set cache table and gives the baseline
   `FinalRowCount` / `CumulativeRowCount` per node (our Unfiltered column + reconciliation source).
   If any set crashes, surface it (don't emit a wrong tree).
2. **Enumerate count points** by walking `cic.RootCohortAggregateContainer` recursively via
   `GetOrderedContents()` (respect `Order`, `Operation`, skip `IDisabled`). For each container, in order:
   for each child a *set-total* (sets) or *container-total* (sub-containers) point, plus a *cumulative*
   point for every non-first enabled child. Carry (Order, Type, Name, ContainerName, SetOperation) — the
   same shape as `CohortCountReport`.
3. **Per count point, build the identifier-list SQL directly from the cache tables (AS BUILT).** Every
   node is recomposed by hand from the per-set cache tables — `CohortQueryBuilder` is NOT used for the
   recompose (only `CohortCompiler` runs once, to populate the cache + give the baseline). This avoids
   parameter hoisting entirely (cache tables are bare identifier lists) and guarantees cache-server-only
   SQL.
   - set cache table: `CachedAggregateConfigurationResultsManager.GetLatestResultsTableUnsafe(agg,
     IndexedExtractionIdentifierList)` → `SELECT <col> AS id FROM <cacheTable>` (`CachedSetSql`).
   - set total → that set SQL.
   - container total → `Compose(container, enabledOrderedChildren)` = `(child0) <Op> (child1) ...` with
     `<Op>` = the container's UNION/INTERSECT/EXCEPT, each arm recursing into `IdSql`.
   - cumulative k → `Compose(parentContainer, children.Take(k+1))`.
   (No globals / no params needed — the arms are `SELECT id FROM <cacheTable>`.)
4. **Split by board in one query per count point:** then
   ```sql
   SELECT d.[Region] AS Region, COUNT(DISTINCT i.id) AS n
   FROM ( <body> ) i
   JOIN <SHARE_Demography> d ON d.chi = i.id
   GROUP BY d.[Region]
   ```
   run on the **cache server** (`DataAccessPortal.ExpectDatabase(cacheServer DB)`). One query → all boards.
5. **Assemble long rows.** For each count point: emit one row per board present (mapped via
   `HealthBoardLookup`), an `Unknown` row = baseline count − Σ known boards (patients not in demography /
   unmapped region), and an `Unfiltered` row = baseline count. `CumulativeCount` filled the same way from
   the cumulative query (null where RDMP's cumulative is null — first child / container totals as RDMP does).
6. **Reconcile (assert + report):** for every count point, Σ board FinalCount (+Unknown) must equal the
   baseline `FinalRowCount`; Unfiltered must equal baseline. Same for cumulative. Mismatch → warn loudly in
   the summary.
7. Write CSV; `BasicActivator.Show` a summary (nodes, boards, any reconciliation drift).

## 3. Output (long format, board-grouped)

Columns: `Board, Node, Order, Type, Name, Container, SetOperation, FinalCount, CumulativeCount`.
Row order: **board-major** — `Unfiltered` block first (the exact UI tree), then each board T, G, … each a
full tree, then `Unknown` last. Within a board, rows follow the tree `Order` (so it reads like the UI count
table repeated per board). Reuses the `CohortCountReport` CSV escaper.

## 4. Why this is cache-only and cross-server-safe

- Every node (set, container, cumulative) is recomposed by hand from the per-set cache tables, so all
  recompose SQL references only cache-server objects.
- The only non-cache object touched is `SHARE_Demography`, required (by the co-location check) to be on the
  cache server, so every query is single-server. The source catalogue servers are never touched after step 1
  (the one `CohortCompiler` build).

## 5. Tests (docker)

**No-DB:** report projection (long format, board-major ordering, Unknown/Unfiltered rows, reconciliation
helper); the GROUP-BY-Region wrapper string builder.

**DB end-to-end (`DatabaseTests`, model on `CohortQueryBuilderWithCacheTests` + the final-list DB test):**
build 2–3 small synthetic catalogues with data + a `SHARE_Demography(chi,Region)` table on the docker
server; set the CIC's `QueryCachingServer` to `TEST_QueryCache`; build an EXCEPT-over-INTERSECT CIC; run the
command. Assert:
- Unfiltered Final/Cumulative per node == values from a direct `CohortCompiler` run (parity with UI).
- Per-board + Unknown sums == Unfiltered at every node (reconciliation).
- A hand-computed board (e.g. all cohort members in Tayside except one) matches at the leaf and after the
  EXCEPT (proves distributivity through the tree).
Run: `bash mac-test-env/run-tests.sh "FullyQualifiedName~CohortBuildHealthBoard"`.

> Note: the Mac+docker SQL TLS limitation blocks a full *CLI* data run (as before); execution correctness
> is proven by these NUnit DB tests through the real query cache + CohortCompiler stack.

## 6. Plugin + 9.2.3 + upload (phase 2)

- New plugin `RdmpCohortBuildHealthBoardBreakdown` (right-click a CIC). Self-contained (copies of the 3
  sources). Built against the v9.2.3 worktree (core lacks the classes → no CS0433), packaged `.rdmp`,
  verified to load (ListSupportedCommands shows `ExportCohortBuildHealthBoardBreakdown`).
- Upload to a **new** OneDrive folder `onedrive:rdmp/healthboard_build_breakdown/` + INSTALL.md, byte-verified.

## 7. Effort

Core command + report + tree walk + per-point query: ~1.5 days. Tests (no-DB + docker cache E2E): ~1 day.
Plugin + 9.2.3 + upload (pipeline exists): ~0.5 day. **≈ 3 days.**

## 8. Checklist

- [x] `CohortBuildHealthBoardBreakdownReport` (long CSV) + no-DB tests
- [x] count-point tree walk (parity with `CohortCountReport` enumeration)
- [x] command: build-once + co-location/cache guards + per-point GROUP BY Region on cache server
- [x] reconciliation asserts (Unfiltered == RDMP; boards+Unknown == Unfiltered)
- [x] docker DB end-to-end (cache + EXCEPT/INTERSECT) green
      (`CohortBuildHealthBoardBreakdownTests` — the fixture in BUILD-BREAKDOWN-TEST-FIXTURE.md:
      national 100→80→65→60→58, Tayside 50→40→32→30→29, Glasgow→17, Fife→12, Unknown=20,
      INTERSECT cumulative=100, partition-sums-to-national at every node; passes)
- [x] new plugin + UI hook + 9.2.3 `.rdmp` load-verified
      (`RdmpCohortBuildHealthBoardBreakdown` — right-click a CIC; built vs vanilla v9.2.3 worktree;
      `.rdmp` loads `ExportCohortBuildHealthBoardBreakdown` into the 9.2.3 CLI, absent without it)
- [x] upload to onedrive:rdmp/healthboard_build_breakdown/ (byte-verified, 15302 bytes) + INSTALL.md
