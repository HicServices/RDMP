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

![Simple Cohort Identification Configuration showing intersect of two datasets](./images/cic_simple.png)

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



The first time a cohort set is run 
