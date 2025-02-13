# Cohort Committing

A Cohort is a collection of unique person identifiers which can be linked against datasets during an extraction.  This namespace covers saving a list of identifiers into a cohort database.

See [Cohort Creation](../CohortCreation/CohortCreation.md) for building queries that identify cohorts from your database based on inclusion/exclusion criteria.

## Cohort Storage

Cohorts are persisted in your database in a set of cohort tables.  In order to support diverse identifier formats (e.g. varchar(10), int) or extracting from existing tables the schema of these stores is not rigidly defined.  

The schema is documented in an [ExternalCohortTable]

## Example Tests

[Create Cohort](../../Rdmp.Core.Tests/CohortCommitting/CommitCohortExample.cs)

[ExternalCohortTable]: ../../Documentation/CodeTutorials/Glossary.md#ExternalCohortTable
