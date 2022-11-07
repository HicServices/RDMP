# Custom Metadata Substitutions

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
|$API_access_URL||
|$Access_options||
|$Administrative_contact_address||
|$Administrative_contact_email||
|$Administrative_contact_name||
|$Administrative_contact_telephone||
|$Attribution_citation||
|$Background_summary||
|$Browse_URL||
|$Bulk_Download_URL||
|$Contact_details||
|$Country_of_origin||
|$Data_standards||
|$DatasetStartDate||
|$Description||
|$Detail_Page_URL||
|$Ethics_approver||
|$Explicit_consent||
|$Geographical_coverage||
|$Granularity||
|$ID||
|$IsColdStorageDataset||
|$IsDeprecated||
|$IsInternalDataset||
|$Last_revision_date||
|$LiveLoggingServer_ID||
|$LoadMetadata_ID||
|$LoggingDataTask||
|$Name||
|$Periodicity||
|$PivotCategory_ExtractionInformation_ID||
|$Query_tool_URL||
|$Resource_owner||
|$Search_keywords||
|$Source_URL||
|$Source_of_data_collection||
|$SubjectNumbers||
|$Ticket||
|$TimeCoverage_ExtractionInformation_ID||
|$Time_coverage||
|$Type||
|$Update_freq||
|$Update_sched||

The following [TableInfo] level elements are also available

|Token| Description|
|--|--|
|$Database||
|$DatabaseType||
|$IsPrimaryExtractionTable||
|$IsTableValuedFunction||
|$IsView||
|$Schema||
|$Server||

Lastly if the DQE has been run on the [Catalogue] then the following elements are available:

|Token| Description|
|--|--|
|$DQE_CountTotal        ||
|$DQE_DateOfEvaluation  ||
|$DQE_DateRange         ||
|$DQE_EndDate           ||
|$DQE_EndDay            ||
|$DQE_EndMonth          ||
|$DQE_EndYear           ||
|$DQE_StartDate         ||
|$DQE_StartDay          ||
|$DQE_StartMonth        ||
|$DQE_StartYear         ||


### CatalogueItem Substitutions

When running with `oneFile` on (or inside a `$foreach $CatalogueItem` block) the following tokens are available:

|Token| Description|
|--|--|
|$Agg_method||
|$Comments||
|$Description||
|$ID||
|$Limitations||
|$Name||
|$Periodicity||
|$Research_relevance||
|$Statistical_cons||
|$Topic||

The following [ColumnInfo] level elements are also available


|Token| Description|
|--|--|
|$Collation||
|$Data_type||
|$Digitisation_specs||
|$Format||
|$IgnoreInLoads||
|$IsAutoIncrement||
|$IsPrimaryKey||
|$RegexPattern||
|$Source||
|$Status||

Lastly if the DQE has been run on the [Catalogue] then the following [CatalogueItem] level result elements are available:

|Token| Description|
|--|--|
|$DQE_CountCorrect||
|$DQE_CountDBNull||
|$DQE_CountInvalidatesRow||
|$DQE_CountMissing||
|$DQE_CountTotal||
|$DQE_CountWrong||
|$DQE_PercentNull||


[Catalogue]: ./Glossary.md#Catalogue
[Catalogues]: ./Glossary.md#Catalogue
[TableInfo]: ./Glossary.md#TableInfo

[Project]: ./Glossary.md#Project
[LoadMetadata]: ./Glossary.md#LoadMetadata
[CatalogueItem]: ./Glossary.md#CatalogueItem