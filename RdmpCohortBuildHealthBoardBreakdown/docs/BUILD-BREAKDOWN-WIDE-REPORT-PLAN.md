# Plan — wide ("horizontal") build-breakdown report

Changes the cohort-build health-board breakdown output from the current **long** format (one row per
count-point × board) to a **wide/horizontal** matrix: one row per count point with the container/set
name once, a `Total` column, one column per health board, plus an explicit split of the old catch-all
`Unknown`. Adds a bottom **% contribution** block and keeps the **breakdown-sums-to-national** check.

Status: PLAN (not yet implemented). Affects only `CohortBuildHealthBoardBreakdownReport` (the
projection/CSV) and a few assertions; the cache-only recompose engine in the command is unchanged.

## 1. Layout

One row per count point (tree order), name written once. Columns:

```
Order | Type | Name | Container | SetOperation | Metric | Total | <Board1..BoardN> | Other | NotKnown
```

- `Total` — the national (non-breakdown) number for that node = RDMP's own count. Kept as a column.
- `<Board1..BoardN>` — one column per Scottish health board (the 15 from HealthBoardLookup; only those
  that appear, ordered by node then name).
- `Other` — patients whose region code IS present in demography but is **not** one of the 15 Scottish
  boards (non-Scottish / unmapped codes). NEW — split out of the old Unknown so non-Scottish boards are
  visible. (Optionally each distinct other code as its own column — see §4 decision.)
- `NotKnown` — residual = `Total − Σ(boards) − Other` = patients **not in demography** + **NULL region**.
  This is the "we genuinely can't place them" bucket.
- `Metric` — `Final` or `Cumulative` (see §4 decision on whether we keep both).

Every data row satisfies: `Σ(boards) + Other + NotKnown == Total`.

### Sample (national cohort = 58, fixture numbers)

```
Order Type       Name   Container SetOp   Metric     Total Tayside Glasgow Fife Other NotKnown
0     Container  Root             EXCEPT  Cumulative   58      29      17   12     0        0
1     Container  Inclusion Root   INTER   Final       100      50      30   20     0        0
2     Cohort Set Registry  Inclusion       Final      120      50      30   20     0       20
...
```

## 2. Bottom block — % contribution to the total

After the data rows, a separator then a `% of final cohort` section: for the **final cohort** (the root
node), each board's share = `board / Total × 100`. One row:

```
% of final cohort                                          100.0  50.0   29.3  20.7  0.0   0.0
```

(Tayside 29/58 = 50.0%, Glasgow 17/58 = 29.3%, Fife 12/58 = 20.7%.) Percentages computed from the
chosen Metric's root row. Option to also emit a per-row `%` block (each node's board split) — see §4.

## 3. The "Unknown" split (answers the non-Scottish question)

Old behaviour: `Unknown = Total − Σ(15 boards)` — merged non-Scottish codes + NULL region + not-in-
demography into one number. New behaviour, using data we already fetch (the GROUP BY returns every
present code):

| Bucket | Definition | Source |
|---|---|---|
| board columns | the 15 Scottish ciphers | GROUP BY rows where `HealthBoardLookup.Resolve(code)` is a real board |
| `Other` | present region codes NOT in the 15 (non-Scottish / unmapped) | GROUP BY rows where Resolve → Unknown node |
| `NotKnown` | `Total − Σ(boards) − Other` | residual = not-in-demography + NULL region |

So non-Scottish boards are no longer hidden — they land in `Other` (or their own columns, §4), and
`NotKnown` becomes a clean "no usable location" figure. (NULL region stays inside `NotKnown`; splitting
NULL from not-in-demography is possible but low value — noted, not planned.)

## 4. Reconciliation check (kept + strengthened)

Keep "breakdown sums to the national non-breakdown search":
- `Total` per node already = RDMP's `CohortCompiler` count (the non-breakdown national number).
- New genuine check: `Σ(boards) + Other` is computed from the independent GROUP BY query; assert it is
  `≤ Total`, and define `NotKnown = Total − that` (so the row always reconciles, and a negative
  `NotKnown` would flag a key/join bug). The DB test additionally asserts every cell against the
  hand-derived fixture, so the equality is a real check, not a tautology.

## 5. Decisions (confirmed 2026-06-27)

1. **Metric = Both** — a `Metric` column; two rows per node (`Final` then `Cumulative`, the latter only
   where RDMP has a cumulative). Node name repeats across its two metric rows.
2. **Other = one combined column** — all present non-Scottish/unmapped codes summed into a single `Other`
   column; `NotKnown` is the separate residual (not-in-demography + NULL region).
3. **Percentages = bottom row only** — a single `% of final cohort` row (each board's share of the root
   final cohort), after a blank separator.

Final column order: `Order, Type, Name, Container, SetOperation, Metric, Total, <boards…>, Other, NotKnown`.

## 6. Scope / effort

- Change is isolated to `CohortBuildHealthBoardBreakdownReport` (new wide `BuildWide`/`ToCsv`) + the
  command's drift/reconcile note + the DB test assertions (now read columns instead of board rows).
- The final-list report (`HealthBoardBreakdownReport`) is left as-is unless you also want it widened.
- Est. ~0.5–1 day incl. updated docker test, then refresh the build-plugin `.rdmp` + OneDrive.
