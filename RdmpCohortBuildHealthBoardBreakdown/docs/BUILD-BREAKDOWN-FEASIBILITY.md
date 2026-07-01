# Feasibility — per-health-board cohort *build* breakdown (UI count tree × health board)

Discussion doc. Extends the committed health-board breakdown (`FEASIBILITY.md` / `PLAN.md`,
final-list counts only) to reproduce the **Cohort Builder's whole count tree** — the per-set and
per-container *total* and *cumulative running total* shown as UNION/INTERSECT/EXCEPT are applied —
**once per health board**, plus the unfiltered baseline. Goal: "see how the number shrinks through
the tree, for each board." Saved to file.

## TL;DR

Feasible and **cheap**, because (a) board-restriction distributes over the set operations, so each
per-board number is just the unrestricted identifier set at that point ∩ board; and (b) RDMP already
caches each cohort set's identifier list as a queryable table. So we **build once** (populating the
cache) and then run **one `GROUP BY Region` query per count point** — every board in that one query.
No per-board rebuild, no source-DB hits beyond the single baseline build.

---

## 1. The maths that removes the N-rebuild cost

Restricting the cohort to board `H` is intersection with a fixed patient set, and `∩ H` distributes
over every container operation:

```
(A ∪ B) ∩ H = (A∩H) ∪ (B∩H)
(A ∩ B) ∩ H = (A∩H) ∩ (B∩H)
(A \ B) ∩ H = (A∩H) \ (B∩H)        (EXCEPT too; order preserved)
```

Therefore the count RDMP shows at **any** node (a set total, a container total, or a
cumulative-up-to-child-k) equals `(unrestricted identifier set at that node) ∩ H`. We never have to
re-run the build under a board filter — we take the unrestricted identifier list at each node and
split it by `Region`.

(Relies on board membership being a per-patient property, 1 board ↔ 1 patient — already confirmed —
and the board filter being a pure post-hoc intersection, not something that changes a set's internal
logic. Both hold.)

## 2. What RDMP caches (verified in source)

- **Cached, queryable:** each cohort *set* (`AggregateConfiguration`) → an indexed single-column
  identifier table `IndexedExtractionIdentifierList_AggregateConfiguration<ID>` in the query-cache
  DB; patient-index tables → `JoinableInceptionQuery_...`. Fetch via
  `CachedAggregateConfigurationResultsManager.GetLatestResultsTable(agg, IndexedExtractionIdentifierList, sql)`
  → fully-qualified table name. (The set's cached list is its FINAL list — post-filters, post-PIT-join.)
- **NOT cached:** container totals, cumulative/running totals. `AggregationContainerTask` is not a
  `CacheableTask`; cumulative is computed by a throwaway `CohortQueryBuilder` over the parent
  container with `StopContainerWhenYouReach = childK`, run only to count rows in memory, then discarded.
- Counts in the UI are `DataTable.Rows.Count` of the pulled identifier list (not SQL `COUNT`).
  `FinalRowCount` = the node's own count; `CumulativeRowCount` = running total within its container
  (null for the first child / when cumulative totals were off).

**Consequence:** the cache gives us exactly the per-set identifier tables. Container/cumulative points
must be *recomposed* — but RDMP will generate that composition SQL for us (reading from cache), or we
recompose in memory. Either way the expensive source queries run once (the baseline build).

## 3. Count points to reproduce (mirror the UI exactly)

Walk `CohortAggregateContainer.GetOrderedContents()` recursively (respect `Order`, `Operation`,
skip disabled), and for each container emit:
- one **set total** row per child set (`FinalRowCount`),
- one **cumulative** row per non-first child (`CumulativeRowCount` = container up to & incl. child k),
- one **container total** row.

This is the same enumeration the existing `CohortCountReport` produces; we reuse its row shape and add
a board dimension.

## 4. Two implementations (both cache-leveraged; recommend A)

### A. Server-side recompose via RDMP's own query builder (recommended)
For each count point, ask RDMP for its identifier-list SQL — it already splices in the cache tables:
- set total → `CohortQueryBuilder(aggregate, globals, childProvider)`
- container total → `CohortQueryBuilder(container, globals, childProvider)`
- cumulative k → `CohortQueryBuilder(parentContainer, …){ StopContainerWhenYouReach = childK }`

Then wrap (params hoisted exactly like the committed-cohort command already does):
```sql
SELECT d.Region, COUNT(DISTINCT i.id) AS n
FROM ( <RDMP identifier-list SQL, reads cache> ) i
JOIN SHARE_Demography d ON d.chi = i.id
GROUP BY d.Region
```
- **One query per count point, all boards at once.** ~`2·sets + containers` queries total (tens, not
  hundreds) — independent of board count.
- **Fidelity:** uses RDMP's exact composition SQL, so it can't drift from the UI semantics
  (order/EXCEPT/disabled/PITs all handled by RDMP).
- **New code is small:** tree walk + the `GROUP BY Region` wrapper + assembling the matrix.
- **Requirement:** the query-cache DB and `SHARE_Demography` must be co-queryable (same server, or
  3-part/linked). Needs confirming (the final-list feature already assumes same server for demography).

### B. Client-side recompose (fallback / no cross-server)
Fetch each set's identifiers once with Region attached (`SELECT t.id, d.Region FROM <cacheTable> t JOIN
SHARE_Demography d …`), then replay the container set-algebra in memory per board (hash sets), mirroring
`CohortCompiler`. Produces every total + cumulative for every board and the unfiltered baseline in one
pass, **no cross-server join**. Cost: pulls all set identifiers to the client (heavy for very large
cohorts). Good fallback when cache and demography live on different servers.

> Recommendation: **A** for fidelity + scale; keep **B** as the fallback when cache/demography aren't
> co-located. Both avoid the N-board rebuild.

## 5. Build-once + cache

1. Ensure the CIC has a `QueryCachingServer` and run `CohortCompilerRunner` once with
   `IncludeCumulativeTotals = true`. This (a) populates every per-set cache table and (b) gives the
   **baseline** `FinalRowCount`/`CumulativeRowCount` per node straight from RDMP.
2. If the cache is already fresh (user built it in the UI), step 1 is a no-op fast path — we can read
   the cache tables directly without re-running source queries.
3. All per-board work in §4 then reads only the cache (+ demography), never the source databases.

## 6. Built-in correctness check

The **unfiltered** column must equal RDMP's own `FinalRowCount`/`CumulativeRowCount` from the baseline
build, and the per-board counts (+ an `Unknown`/not-in-demography bucket) must **sum to the unfiltered**
at every node. Both are cheap asserts that catch any composition/order mistake automatically.

## 7. Output options (for discussion)

Rows = count points in tree order (Order, Type, Name, Container, SetOperation). Then either:
- **Long:** add `Board`, `Node`, `FinalCount`, `CumulativeCount` columns (one row per count-point ×
  board). Most flexible; easy to pivot. ← suggested default.
- **Wide:** a `FinalCount`/`CumulativeCount` pair of columns per board. Closest to "the UI table with a
  column per board" but wide and awkward with ~15 boards × 2.
- **One file per board** (+ an `_unfiltered` file): each is exactly today's `CohortCountReport` CSV.

All reuse `HealthBoardLookup` (Region→board/node) and the `Unknown` bucket from the existing feature.

## 8. Scope / caveats

- **CIC-only.** This needs the build tree; a committed `ExtractableCohort` has no tree (final-list
  breakdown already covers that case).
- Ships as a second command in the existing `RdmpHealthBoardBreakdown` plugin (e.g.
  `ExportCohortBuildHealthBoardBreakdown`), reusing the demography resolution, param-hoisting, CSV and
  `HealthBoardLookup` already written.
- Cross-server (approach A) and cache-presence are the two real requirements — both checkable up front
  with a clear `SetImpossible` message.
- Disabled sets/containers and patient-index tables: handled for free in A (RDMP's SQL); must be
  replicated in B.

## 9. Effort (rough)

- Tree walk + count-point enumeration (reuse `CohortCountReport` shape): ~0.5 day
- Approach A wrapper + param hoist + run/collect + matrix assembly: ~1 day
- Baseline build + reconciliation asserts: ~0.5 day
- No-DB unit tests (composition/ordering/Unknown) + a docker DB end-to-end (small CIC with a cache,
  EXCEPT over INTERSECT, assert unfiltered == RDMP and boards sum to total): ~1 day
- Approach B fallback (optional): ~1 day
- Plugin wiring + 9.2.3 build (pipeline already exists): ~0.5 day

**≈ 3–3.5 days** (A only), +1 day for the B fallback.

## 10. Decisions (locked 2026-06-25)

1. **Approach A only** (server-side recompose via `CohortQueryBuilder`, reading the cache). No client-side
   B fallback — the cohort spans servers, so the cache is the single consolidation point and we operate
   purely on it.
2. **New, separate plugin** (`RdmpCohortBuildHealthBoardBreakdown`), not an addition to the existing
   final-list plugin. Reuses `HealthBoardLookup` + the demography-resolution / param-hoist / CSV patterns
   by copying them in (plugin must be self-contained for the 9.2.3 build).
3. **Output = long format, grouped clearly by health board.** One row per (board × count-point), columns:
   `Board, Node, Order, Type, Name, Container, SetOperation, FinalCount, CumulativeCount`. Rows ordered
   board-major (all of board T's tree, then board G's, …), with an `Unfiltered` pseudo-board first and an
   `Unknown` board last so each node reconciles.
4. **Cache is REQUIRED.** Catalogues are on different servers, so without a populated query cache the
   composition can't run. `SetImpossible` if the CIC has no `QueryCachingServer` or the per-set caches
   are missing/stale (offer to run one baseline build to populate).
5. **Cross-server demography:** the recompose + `GROUP BY Region` join runs on the **cache server**, so
   `SHARE_Demography` must be reachable from there. The plugin checks `QueryCachingServer.Server` ==
   `SHARE_Demography` `TableInfo.Server` at construction and `SetImpossible`s with a clear message if not.
   (Likely same server, but verified at runtime — see note below.)
6. **Cumulative semantics:** reproduce RDMP's "cumulative within container, from the 2nd child" exactly,
   by using RDMP's own `StopContainerWhenYouReach` query (guarantees parity with the UI).
7. Boards come only from `SHARE_Demography.Region` (no per-board published filters needed for counting).

> **Co-location note:** the first (final-list) plugin proved `SHARE_Demography` is on the same server as
> the cohort/data store; it did NOT exercise the query-cache server (a separate `ExternalDatabaseServer`).
> So co-location of cache + demography is *probable* but not proven from that work — hence the runtime
> check in decision 5. If they turn out to be on different servers, the mitigation is to materialise a
> small `(chi, Region)` tag table on the cache server once per run and join to that instead (out of scope
> unless the check fails).
