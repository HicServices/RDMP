# Validation fixture — per-health-board build breakdown (docker, synthetic)

Fully deterministic. 1 patient ↔ 1 board, so the 3 boards **partition** the cohort and sum to the
national number at *every* node (set total and cumulative). Exercises INTERSECT, EXCEPT, cumulative
through the tree, and the "no region" Unknown bucket.

## People (120 ids: P001–P120)

| Group | IDs | Count | Region |
|---|---|---|---|
| Tayside | P001–P050 | 50 | T |
| Glasgow | P051–P080 | 30 | G |
| Fife | P081–P100 | 20 | F |
| Registry-only (no demography row) | P101–P120 | 20 | — (Unknown) |

## Catalogues / tables (all on one docker server)

- **BB_Demography** (`chi, Region`): P001–P100 with their Region (the only place Region lives; the
  reference table). 100 rows.
- **BB_Registry** (`chi`): P001–P120 (the cohort source; includes 20 people with no demography row). 120 rows.
- **BB_Excl1..4** (`chi`): disjoint exclusion subsets, all within P001–P100 (each = exactly the people it removes):

| Excl | Tayside | Glasgow | Fife | Total |
|---|---|---|---|---|
| Excl1 | P001–P010 (10) | P051–P056 (6) | P081–P084 (4) | 20 |
| Excl2 | P011–P018 (8) | P057–P060 (4) | P085–P087 (3) | 15 |
| Excl3 | P019–P020 (2) | P061–P062 (2) | P088 (1) | 5 |
| Excl4 | P021 (1) | P063 (1) | — (0) | 2 |

## CIC structure

```
ROOT  (EXCEPT)
├─ Inclusion  (INTERSECT)
│    ├─ BB_Registry          set total 120
│    └─ BB_Demography        set total 100   ⇒ Inclusion = 100
├─ BB_Excl1                  set total 20
├─ BB_Excl2                  set total 15
├─ BB_Excl3                  set total  5
└─ BB_Excl4                  set total  2     ⇒ ROOT (national cohort) = 58
```

Child order matters (EXCEPT/cumulative): Inclusion first, then Excl1..4 in order.

## Expected count tree (what the plugin must reproduce)

`FinalCount` = the node's own set/container count. `CumulativeCount` = running total within the
container (null for the first child, per the UI).

### Inclusion container (INTERSECT)
| Node | Final (Nat) | Cum (Nat) | T | G | F | Unknown |
|---|---|---|---|---|---|---|
| BB_Registry (set) | 120 | — | 50 | 30 | 20 | **20** |
| BB_Demography (set) | 100 | 100 | 50 | 30 | 20 | 0 |
| Inclusion (total) | 100 | — | 50 | 30 | 20 | 0 |

(The 20 registry-only people show up under Unknown on the BB_Registry set, then the INTERSECT with
demography drops them — demonstrating the Unknown bucket and that no-region people don't leak.)

### ROOT container (EXCEPT) — cumulative is the key check
| Node | Final (Nat) | Cum (Nat) | Cum T | Cum G | Cum F |
|---|---|---|---|---|---|
| Inclusion (child 0) | 100 | — | 50 | 30 | 20 |
| BB_Excl1 | 20 | **80** | 40 | 24 | 16 |
| BB_Excl2 | 15 | **65** | 32 | 20 | 13 |
| BB_Excl3 | 5 | **60** | 30 | 18 | 12 |
| BB_Excl4 | 2 | **58** | 29 | 17 | 12 |
| ROOT (total) | 58 | — | 29 | 17 | 12 |

National cumulative: 100 → 80 → 65 → 60 → 58.
Per-board cumulative diverges (Tayside −21, Glasgow −13, Fife −8) and **T+G+F = national at every row**
(40+24+16=80, 32+20+13=65, 30+18+12=60, 29+17+12=58). That cross-check is the automated assertion.

## What this validates

- INTERSECT (120 ∩ 100 = 100) and EXCEPT cumulative down the tree.
- Per-board cumulative correctness via distributivity (boards partition → sum to national everywhere).
- The Unknown / not-in-demography bucket (the 20 registry-only ids).
- The unfiltered column equals RDMP's own `CohortCompiler` counts (separate assertion).

## Test mechanics (docker)

`DatabaseTests` fixture: create the 6 tables with the data above on the docker server, import as
catalogues, set BB_Demography.chi + BB_Registry.chi + each Excl.chi as IsExtractionIdentifier, set
BB_Demography.Region; build the CIC tree above; set `QueryCachingServer = TEST_QueryCache`; run the
command; assert the full table above (national + T/G/F + Unknown) cell-by-cell, and that every node's
T+G+F(+Unknown) sums to its national value.
