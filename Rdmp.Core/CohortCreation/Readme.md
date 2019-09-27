# Cohort Creation

A Cohort is a collection of unique person identifiers which can be linked against datasets during an extraction.  This namespace covers the creation of queries that identify lists of patients (based on inclusion / exclusion criteria).

See [CohortComitting](../CohortCommitting/Readme.md) for committing (saving) a final list of patients (or for generating cohorts directly from a file etc).



## Query Caching

## Background

A complicated cohort can easily include 10 or more subsets.  To speed up performance and to persist results a query cache can be used.  A query cache also get's around DBMS limitations e.g. MySql not supporting Set operations (UNION / INTERSECT / EXCEPT)

## Cache Hit/Miss

Consider the following cohort

- INTERSECT 
  - People in Biochemistry with an HBA1C
  - People in Biochemistry with an NA

![Simple Cohort Identification Configuration showing intersect of two datasets](./Images/cic_simple.png)

If we have no cache then the following SQL would be executed:

```sql
SELECT
distinct
[TEST_ExampleData].dbo.[Biochemistry].[chi]
FROM 
[TEST_ExampleData]..[Biochemistry]
WHERE
(
/*TestCode is NA*/
[TEST_ExampleData].dbo.[Biochemistry].[TestCode] = 'NA'
)

INTERSECT

SELECT
distinct
[TEST_ExampleData].dbo.[Biochemistry].[chi]
FROM 
[TEST_ExampleData]..[Biochemistry]
WHERE
(
/*TestCode is HBA1c*/
[TEST_ExampleData].dbo.[Biochemistry].[TestCode] like '%HBA%'
)
```

A caching database can be used to store the results of each subcomponent.  The final list of identifiers is stored in an indexed table in the caching database along with the SQL used to fetch the results.  This indexed tables can be used by RDMP to run the final container (the INTERSECT).  

Caching provides the following benefits:

- Parallel processing of subsets
- Faster execution of set containers
- Allows combining sets built on different servers / DBMS (e.g. INTERSECT an Oracle dataset with an Sql Server dataset)
- Use set operations (UNION / INTERSECT / EXCEPT) on a DBMS that doesn't support it (MySql)

The following flow chart describes the process RDMP uses to build a SET container (and determine where to execute the query):

![Flowchart showing when/if RDMP will use a cache fetch in an SQL query](./Images/flowchart.png)

_* If you are using credentials to access a table (e.g. username and password rather than integrated security) then differing credentials is treated as different servers (since a connection cannot be openned to both objects)_

When run from the cache the above query would be:

```sql
(
/*Cached:cic_10_Biochem NA result*/
select * from [RDMP_QCache]..[IndexedExtractionIdentifierList_AggregateConfiguration68]

INTERSECT

/*Cached:cic_10_Biochem HBA1C*/
select * from [RDMP_QCache]..[IndexedExtractionIdentifierList_AggregateConfiguration67]

)
```

Caching happens automatically after executing an uncached query.  If you make a change to a cohort the corresponding cache entry is automatically cleared (and the table dropped).

When using the execute all button, execution will start with each subquery in order to maximise cache usage.  This is especially important when cumulative totals is enabled (which results in more component combinations being executed at once).

## Code

The cache usage flow chart is implemented by the `CohortQueryBuilderResult` class.  The following states can be determined:

|State|Description|
|-|-|
|MustUse| The cache must be used and all Dependencies must be cached.  This happens if dependencies are on different servers / credentials.  Or the query being built involves SET operations which are not supported by the DBMS of the dependencies (e.g. MySql UNION / INTERSECT etc).|
|Opportunistic|All dependencies are on the same server as the cache.  Therefore we can mix and match where we fetch tables from (live table or cache) depending on whether the cache contains an entry for it or not. |
|AllOrNothing|All dependencies are on the same server but the cache is on a different server.  Therefore we can either run a fully cached set of queries or we cannot run any cached queries|

The following classes play a role in building and executing cohort building queries

|Class|Role|
|-|-|
|CohortCompiler| Runs multiple set containers / cohort subcomponents at once and stores states|
|ICompileable| A subcomponent that could be executed (or encountered an error while building) | 
|CohortIdentificationTaskExecution | The current state of an `ICompileable` including result / crashed etc |
|CohortQueryBuilder | Manages configuration/tailoring of cohort sets e.g. run as normal or run TOP x only etc |
|CohortQueryBuilderResults | Identifies all subcomponents in the container / cohort and makes descisions about cache usage |
|CohortQueryBuilderDependency| Stores the uncached and cached (if available) SQL for the subcomponent|
|CohortQueryBuilderHelper| Builds the uncached SQL for each atomic subcomponent (uses an `AggregateBuilder` to do most of the work)|

## Class Diagram

![Class Diagram of cohort building](./Images/classdiagram.png)
