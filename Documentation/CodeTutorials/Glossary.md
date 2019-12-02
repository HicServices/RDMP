# Glossary

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