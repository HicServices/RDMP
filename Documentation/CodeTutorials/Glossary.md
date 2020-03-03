# Glossary

## Catalogue ![Catalogue Icon](../../Rdmp.Core/Icons/Catalogue.png)

The central class for the RDMP, a Catalogue is a virtual dataset e.g. 'Hospital Admissions'. 

A Catalogue can be a merging of multiple underlying tables and exists independent of where the data is actually stored (look at other classes like [TableInfo] to see the actual locations of data).  

As well as storing human readable names/descriptions of what is in the dataset it is the hanging off point for Attachments ([SupportingDocument]), validation logic, extractable columns, ways of filtering the data, aggregations to help understand the dataset etc.

Catalogues are always flat views although they can be built from multiple relational data tables underneath.

## ExternalCohortTable ![ExternalCohortTable Icon](./../../Rdmp.Core/Icons/ExternalCohortTable.png)

Records where to store linkage cohorts (see [ExtractableCohort]).  

Since every agency handles cohort management differently RDMP is built to support diverse cohort [DBMS] table schemas.  There are no fixed datatypes / columns for cohort databases. 

An ExternalCohortTable stores:

- What table contains your cohort identifiers
- What table describes the cohorts (e.g. description, version etc)
- Which column is the private identifier
- Which column is the release identifier
 
Both the cohort and custom table names table must have a foreign key into the definition table.  You are free to add additional columns to these tables or even base them on views of other existing tables in your database. 

You can have multiple ExternalCohortTable sources in your database for example if you need to support different identifier datatypes / formats.

## ExtractableCohort ![ExtractableCohort Icon](./../../Rdmp.Core/Icons/ExtractableCohort.png)

Records the location and ID of a cohort in an [ExternalCohortTable] database. 

This allows RDMP to record which cohorts are part of which ExtractionConfiguration in a Project without having to move the identifiers into the RDMP application database.  

Each ExtractableCohort has an OriginID, this field represents the id of the cohort in the CohortDefinition table of the [ExternalCohortTable]. Effectively this number is the id of the cohort in your cohort database while the ID property of the [ExtractableCohort] (as opposed to OriginID) is the RDMP ID assigned to the cohort. This allows you to have two different cohort sources both of which have a cohort id 10 but the RDMP software is able to tell the difference. In addition it allows for the unfortunate situation in which you delete a cohort in your cohort database and leave the ExtractableCohort orphaned - under such circumstances you will at least still have your RDMP configuration and know the location of the original cohort even if it doesn't exist anymore.

## SupportingDocument ![SupportingDocument Icon](./../../Rdmp.Core/Icons/supportingDocument.png)

Describes a document (e.g. PDF / Excel file etc) which is useful for understanding a given dataset ([Catalogue]). This can be marked as Extractable in which case every time the dataset is extracted the file will also be bundled along with it (so that researchers can also benefit from the file).  You can also mark SupportingDocuments as Global in which case they will be provided (if Extractable) to researchers regardless of which datasets they have selected e.g. a PDF on data governance or a copy of an empty 'data use contract document'.

## TableInfo ![TableInfo Icon](../../Rdmp.Core/Icons/tableinfo.png)

Describes an sql table (or table valued function) on a given [DBMS] Server from which you intend to either extract and/or load / curate data.  A TableInfo represents a cached state of the live database table schema.  You can synchronize a TableInfo at any time to handle schema changes (e.g. dropping columns)

## DBMS
Database Management System.  Refers to a specific proprietary engine e.g. Oracle, MySql, SqlServer.  

A feature that is compatible with multiple DBMS is one which works regardless of the database engine hosting the data and may support drawing data from multiple providers/instances at once.

## UNION

Mathematical set operation which matches unique (distinct) identifiers in **any** datasets being combined (e.g. SetA UNION SetB returns any patient in **either SetA or SetB**).  

## INTERSECT

Mathematical set operation which matches unique (distinct) identifiers  **only** if they appear in **all** datasets being combined (e.g. SetA INTERSECT SetB returns patients who appear in **both SetA and SetB**).

## EXCEPT

Mathematical set operation which matches unique (distinct) identifiers  **in the first dataset** only if they **do not appear in any of the subsequent datasets** being combined (e.g. SetA EXCEPT SetB returns patients who appear in **SetA but not SetB**).

## IsExtractionIdentifier

Indicates that a column contains patient identifier(s) e.g. a CHI / NHS number etc.  Although unusual, you can have more than one in a given dataset e.g. ParentCHI, BabyCHI,TwinBabyCHI,TripletBabyCHI.  All IsExtractionIdentifiers should have the same/compatible datatypes to prevent problems doing linkage between datasets/cohorts.

An RDMP instance can host multiple types of identifier e.g. CHI / NHS number but datasets cannot be joined / cohorts built between datasets that only contain different identifier types from one another.

When building cohorts the distinct values in the chosen IsExtractionIdentifier column are used for set operations (INTERSECT / UNION etc) and join operations (e.g. with patient index tables).

IsExtractionIdentifier columns do not have to have the same name (although it helps) e.g. mixing columns called "Chi" and "PatientChi" would be fine as long as both contained identifiers in the CHI format (e.g. `varchar(10)`).

When joining between datasets on different [DBMS] IsExtractionIdentifier columns are compatible as long as the distinct datatypes are semantically similar (e.g. Oracle `varchar2(10)` could be INTERSECTED with Sql Server `varchar(10)` - providing a query cache was used)

[DBMS]: #DBMS
[Catalogue]: #Catalogue
[TableInfo]: #TableInfo
[SupportingDocument]: #SupportingDocument
[ExternalCohortTable]: #ExternalCohortTable
[ExtractableCohort]: #ExtractableCohort