# Custom Metadata Substitutions

For a simple example of this functionality see [FAQ 'Custom Metadata Report'](./FAQ.md#custom-metadata-report)

## Contents

- [Background](#background)
- [For Each Catalogue](#for-each-catalogue)
- [For Each CatalogueItem](#for-each-catalogueitem)
- [Substitution Tokens](#subsitution-tokens)
  - [Catalogue Substitutions](#catalogue-substitutions)
  - [CatalogueItem Substitutions](#catalogueitem-substitutions)

## Background

This file describes the substitution tokens that you can use with the `ExtractMetadata` command line.

Use `./rdmp describe ExtractMetadata` to display CLI arguments:

```
Name: ExecuteCommandExtractMetadata

Description:
Extract metadata from one or more Catalogues based on a template file using string replacement e.g. $Name for the catalogue's name

USAGE:
./rdmp.exe ExtractMetadata <catalogues> <outputDirectory> <template> <fileNaming> <oneFile> <newlineSub> <commaSub>

PARAMETERS:
catalogues      Catalogue[]
outputDirectory DirectoryInfo       Where new files should be generated
template        FileInfo            Template file in which keys such as $Name will be replaced with the corresponding
                                    Catalogue entry
fileNaming      String              How output files based on the template should be named.  Uses same replacement
                                    strategy as template contents e.g. $Name.xml
oneFile         Boolean             True to append all outputs into a single file.  False to output a new file for every
                                    Catalogue
newlineSub      String              Optional, specify a replacement for newlines when found in fields e.g. <br/>.
                                    Leave as null to leave newlines intact.
commaSub        String              Optional, specify a replacement for the token $Comma (defaults to ',')
```

The `oneFile` setting determines whether a single markdown file containing all [Catalogues] is generated or 1 file per [Catalogue].

## For Each Catalogue

If running in `oneFile` mode then you can use `$foreach Catalogue` to iterate all Catalogues.  Within the loop you can use `$foreach CatalogueItem` (see next section).

End the loop with `$end`

For example:

```
# Datasets

These are the datasets we hold:

$foreach Catalogue
- $Name
$end
```

## For Each CatalogueItem

When running with `oneFile` off (or inside a `$foreach $Catalogue` block) you can iterate each [CatalogueItem] in the 
[Catalogue] using a `$foreach CatalogueItem` block.

End the loop with $end.

For example:

```
# Datasets

These are the datasets we hold:

$foreach Catalogue
- $Name
$foreach CatalogueItem
  - $Name
$end
$end
```

## Substitution Tokens

### Catalogue Substitutions

When running with `oneFile` off (or inside a `$foreach $Catalogue` block) the following tokens are available:

|Token| Description|
|--|--|
|$API_access_URL| User provided |
|$Access_options| User provided |
|$Administrative_contact_address| User provided |
|$Administrative_contact_email| User provided |
|$Administrative_contact_name| User provided |
|$Administrative_contact_telephone| User provided |
|$Attribution_citation| User provided |
|$Background_summary| User provided |
|$Browse_URL| User provided |
|$Bulk_Download_URL| User provided |
|$Contact_details| User provided |
|$Country_of_origin| User provided |
|$Data_standards| User provided |
|$DatasetStartDate| User start date for the dataset|
|$Description|User provided description of the dataset, can be multi-line |
|$Detail_Page_URL|User provided |
|$Ethics_approver|User provided |
|$Explicit_consent|User provided, Yes or No (or blank)|
|$Geographical_coverage|User provided |
|$Granularity|User provided |
|$ID|RDMP allocated number uniquely identifying this object instance in this RDMP installation|
|$IsDeprecated|True or False flag, configurable in RDMP|
|$IsInternalDataset|True or False flag, configurable in RDMP|
|$Last_revision_date|User provided|
|$LoggingDataTask|RDMP Logging/Audit database task name|
|$Name|Name of the Catalogue|
|$Periodicity|User provided category|
|$PivotCategory_ExtractionInformation_ID||
|$Query_tool_URL|User provided|
|$Resource_owner|User provided|
|$Search_keywords|User provided|
|$Source_URL|User provided|
|$Source_of_data_collection|User provided|
|$SubjectNumbers|User provided value for expected number of individuals in dataset|
|$Ticket| Ticketing system entry for tracking this data item (Jira / GitHub etc)|
|$Time_coverage|User provided data estimating data range|
|$Type|User provided category|
|$Update_freq|User provided|
|$Update_sched|User provided|

The following [TableInfo] level elements are also available.  If there are multiple [TableInfo]
below the [Catalogue] (e.g. with SQL JOINs) then the one marked `IsPrimaryExtractionTable` will be used (or the first configured if none are so marked).

|Token| Description|
|--|--|
|$Database|Database that the underlying table below [Catalogue] exists on|
|$DatabaseType|[DBMS] type that data exists on|
|$IsPrimaryExtractionTable|True the table is marked the first of several underlying the [Catalogue]|
|$IsTableValuedFunction|True if the [Catalogue] is pointed at an Sql Server Table Valued Function|
|$IsView|True if the [Catalogue] is pointed at a View|
|$Schema|The schema the table/view exists in (can be blank if default/not supported by [DBMS])|
|$Server|Server name that the underlying table below [Catalogue] exists on (e.g. localhost)|

Lastly if the DQE has been run on the [Catalogue] then the following elements are available:

|Token| Description|
|--|--|
|$DQE_CountTotal        |Total number of records read last time DQE ran|
|$DQE_DateOfEvaluation  |Date DQE was last ran|
|$DQE_DateRange         |Range of data found when DQE was run after discarding outliers|
|$DQE_EndDate           |Ending date of data found (discarding outliers)|
|$DQE_EndDay            |Day portion of EndDate|
|$DQE_EndMonth          |Month portion of EndDate|
|$DQE_EndYear           |Year portion of EndDate|
|$DQE_StartDate         |Starting date of data found (discarding outliers)|
|$DQE_StartDay          |Day portion of StartDate|
|$DQE_StartMonth        |Month portion of StartDate|
|$DQE_StartYear         |Year portion of StartDate|


### CatalogueItem Substitutions

When running with `oneFile` on (or inside a `$foreach $CatalogueItem` block) the following tokens are available:

|Token| Description|
|--|--|
|$Agg_method| User provided|
|$Comments|User provided|
|$Description|User entered free text description of the column.  Can be multi-line|
|$ID|RDMP allocated number uniquely identifying this object instance in this RDMP installation|
|$Limitations|User provided|
|$Name|Name of the [CatalogueItem]|
|$Periodicity|User provided|
|$Research_relevance|User provided|
|$Statistical_cons|User provided|
|$Topic|User provided|

The following [ColumnInfo] level elements are also available

|Token| Description|
|--|--|
|$Collation|Column collation configured in the [DBMS] or blank of not applicable (e.g. to numerical columns)|
|$Data_type|The SQL datatype of the column (e.g. `varchar(100)`)|
|$Digitisation_specs|User provided|
|$Format|User provided|
|$IgnoreInLoads|True if this column is ignored by RDMP data loads|
|$IsAutoIncrement|True if configured as auto incrementing in the [DBMS]|
|$IsPrimaryKey|True if part or all of the primary key on the parent table|
|$Source|User provided|
|$Status|User provided|

Lastly if the DQE has been run on the [Catalogue] then the following [CatalogueItem] level result elements are available:

|Token| Description|
|--|--|
|$DQE_CountCorrect|Total number of values (including nulls) that passed all validation rules in DQE for this column|
|$DQE_CountDBNull|Total number of values that were null (regardless of validation) when DQE was last run|
|$DQE_CountInvalidatesRow|Total number of values (including nulls) that failed a validation rule with Consequence InvalidatesRow|
|$DQE_CountMissing|Total number of values (including nulls) that failed a validation rule with Consequence Missing|
|$DQE_CountTotal|Total number of values read from table|
|$DQE_CountWrong|Total number of values (including nulls) that failed a validation rule with Consequence Wrong|
|$DQE_PercentNull|The proportion of values in the column that were null when DQE was last run|


[Catalogue]: ./Glossary.md#Catalogue
[Catalogues]: ./Glossary.md#Catalogue
[TableInfo]: ./Glossary.md#TableInfo
[DBMS]: ./Glossary.md#DBMS
[ColumnInfo]: ./Glossary.md#ColumnInfo
[Project]: ./Glossary.md#Project
[LoadMetadata]: ./Glossary.md#LoadMetadata
[CatalogueItem]: ./Glossary.md#CatalogueItem
