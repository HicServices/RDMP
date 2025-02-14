# RDMP User Manual

This is the user manual for RDMP.  You may also want to read:

- [FAQ](./FAQ.md)
- [NewDevelopers](./../../NoteForNewDevelopers.md)

## Contents

- [Background](#background)
  * [What is RDMP?](#what-is-rdmp)
  * [Why is research data curation important?](#why-is-research-data-curation-important)
- [Getting Started](#getting-started)
  * [Installing RDMP Client Software](#installing-rdmp-client-software)
  * [System Requirements](#system-requirements)
  * [Platform Database Setup](#platform-database-setup)
  * [Example Data](#example-data)
  * [Importing a flat file as a new dataset](#importing-a-flat-file-as-a-new-dataset)
  * [Import Existing Table](#import-existing-table)
  * [Configure Extractability](#configure-extractability)
- [Data Quality](#data-quality)
- [Dashboards](#dashboards)
- [Sql Code Management](#sql-code-management)
  * [Filters](#filters)
    + [Parameters](#parameters)
  * [Extraction Transforms](#extraction-transforms)
    + [Creating an Extraction Transform](#creating-an-extraction-transform)
- [Data Load Engine](#data-load-engine)
  * [RAW Bubble, STAGING Bubble, LIVE Model](#raw-bubble--staging-bubble--live-model)
  * [Error Messages](#error-messages)
  * [Archive Tables](#archive-tables)
  * [Creating a Data Load Configuration](#creating-a-data-load-configuration)
  * [Configuring the load](#configuring-the-load)
    + [Running the load](#running-the-load)
    + [Resizing a column](#resizing-a-column)
    + [Archiving](#archiving)
    + [Data Load Engine Logging](#data-load-engine-logging)
- [Cohort Builder](#cohort-builder)
  * [Identify Cohort](#identify-cohort)
    + [Cohort Containers](#cohort-containers)
  * [Commit Cohort](#commit-cohort)
    + [Saved Cohorts](#saved-cohorts)
    + [Cohort Pipeline](#cohort-pipeline)
- [Release Identifier Allocation](#release-identifier-allocation)
  * [Do Not Allocate Release Identifiers](#do-not-allocate-release-identifiers)
  * [Allocate Release Identifiers Manually](#allocate-release-identifiers-manually)
  * [Allocate Release Identifiers With Plugin](#allocate-release-identifiers-with-plugin)
- [Pipelines](#pipelines)
- [Extraction](#extraction)
  * [Configuring an Extraction](#configuring-an-extraction)
  * [Verification](#verification)
  * [Adding filters](#adding-filters)
  * [Scalability and Adaptability](#scaleability-and-adaptability)
- [Release](#release)

# Background

## What is RDMP?

RDMP is a tool for the curation of research datasets.  This includes many typical ETL tasks but also tools for management of the research lifecycle.  The software focuses on ensuring thorough documentation of datasets, reliable loading of often poorly structured/volatile data, cohort linkage and reproducibility of project extracts.

## Why is research data curation important?
Data management and data curation of long-term study and research databases are time consuming and complex activities that demand the attention of experts with very specific skills. Some of the most costly and complex data management activities emerge from consideration of two common scenarios. The first considers a single cohort used in a longitudinal study accruing data in distinct phases where the new data must be reconciled and merged with the existing data sets. The second scenario occurs when distinct cohorts from different studies of the same disease are merged to create greater scale in the research data. Again the data must be merged and reconciled in order to create an aggregate data set that is valid in its totality.

![Common data analyst questions](Images/UserManual/CommonQuestions.png)

Existing data management approaches are focused on the initial generation and preparation of project research data and on preservation techniques that promote reuse of the data at the end of individual research projects. These approaches do not consider longer term studies and research programmes and fail to account for the key data merge, transformation and enrichment processes that are applied over life-time study lengths and that shape the data to support analysis and results. Failure to capture the project level transformation processes represents a major loss for long lived research data sets, as data improvements identified by individual studies and cohorts are not fed back into larger aggregated data sets to extend the data and improve the data quality. 

Continuing dissatisfaction within the academic community with the lack of transparency in research data management and the inability to reproduce study results and understand the provenance of study data calls for further revision and extension of the research data management techniques. This software aims to resolve the major data management issues associated with long term study data management through a distinct life cycle for research data merge management. It focuses on transformation processes used within research projects brings transparency and reproducibility benefits through process mining. It also accommodates variation in the data and allowing multiple simultaneous versions and potentially conflicting views to exist through the application of competing transformation processes.

# Getting Started

## Installing RDMP Client Software
In a normal RDMP deployment, users access a single shared Metadata database. 

![Platform database setup](Images/UserManual/PlatformDatabases.png)

You can install the client software via the [installer link on the Github site](https://github.com/HicServices/RDMP#install ).  Once installed the software will guide you through the process of setting up platform databases / connecting to existing platform databases (see [Platform Database Setup](#platform-database-setup)).

It is possible to run RDMP as a standalone tool without an Sql Server instance using a [File System Backend](./YamlRepository.md) but this approach is recommended only for single user or standalone systems where RDMP performs a specific activity in isolation (e.g. data load, cohort creation etc).

## System Requirements
You will need an Sql Server instance (unless using a [file system backend](./YamlRepository.md)).  Sql Server Express is free.  Alternatively you can install LocalDb from the Visual Studio Installer:

![Platform database setup](Images/Tests/InstallingLocalDb.png)

You will also need to have your actual dataset data in a relational database.  RDMP supports accessing research data repositories stored in MySql, Sql Server and Oracle databases but stores it's own metadata (platform databases) in Sql Server only.

If your dataset data is currently in flat files then RDMP can load them into a relational database for you via the data load engine.

## Platform Database Setup

The RDMP uses SQL Server databases* to store metadata (dataset/column descriptions, validation rules, validation results, data load configurations etc) as well as to store logging data, caching repository data etc.  These 'platform databases' are separate from your 'data repository' which is the location that you store your live research data.

The first time you start RDMP (ResearchDataManagementPlatform.exe) you will be prompted to create the metadata databases that allow RDMP to function.  The simplest approach is to create them all on a single server, to do this enter your Sql Server name and a prefix for the databases.

Choose `Set Platform Databases...`

If you are a new user at a site where there is already a platform database you can connect to the existing instance.  Otherwise choose 'I want to create new Platform Databases'.

Enter the connection details of your server and any keywords that are required to access (custom ports, certificate validation options etc):

![Platform database setup](Images/UserManual/CreatePlatformDatabases.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp install "(localdb)\MSSQLLocalDB" RDMP_ -e
> ```

This creates 4 databases which are the minimum set of platform databases required for RDMP to function.  The databases fulfil the following roles.

|Database|Role|
|--|---|
| Catalogue| Stores all descriptive, technical, validation, extraction information, attachments etc about your datasets (both externally accessible and internal).  Also stores Cohort Builder and data load configurations. |
| DataExport| Stores all project extraction configurations allowing for long term versioning and reproducibility into a variety destination formats (CSV, extract to database etc).  Also stores which final cohort lists were used with which extraction projects (Actual identifier lists are not stored in this database) |
| Data Quality Engine (DQE)| Stores a longitudinal record of the results of Data Quality Engine runs on your datasets.  This includes how many rows passed validation rules, how many rows there are per month etc. |
|Logging| Stores a hierarchical audit of all flows of data from one place to another.  This includes data being loaded, data being extracted and even internal processes like DQE executions. |

There are a couple of other database types which can be created as and when you need them.  These include:

|Database|Role|
|---|---|
| [Query Caching](./../../Rdmp.Core/CohortCreation/CohortCreation.md)| Improves the performance of complex cohort identification configurations and anonymisation.  Also allows cross server and plugin Cohort Builder elements (e.g. to [REST APIs](./FAQ.md#apis))|
|Anonymisation|Provides a way of performing identifier dropping / substitution on data load for when you want an entirely anonymous data repository|
|[Plugin Databases](./FAQ.md#plugins)| RDMP supports plugins which can in some cases have their own database(s)|

_\* Unless using [file system backend](./YamlRepository.md)_

## Example Data
Example data can be setup on install by ticking the 'Example Datasets' checkbox during platform database setup.

New example data can be generated through the `Diagnostics=>Generate Test  Data` menu. Or with the [SynthEHR](https://github.com/HicServices/SynthEHR) command line tool. 

## Importing a flat file as a new dataset
If you have some CSV files (or Excel/Fixed Width) that you want to load into your database, you can do so with the RDMP bulk import tool.

Alternatively if you already have your data in a database then you can simply [tell RDMP where it is](#import-existing-table).

There are many problems that can occur in the daily handling of research data by data analysts.  It can be helpful to discover how RDMP handles various problems and what problems it cannot handle (and how it communicates this to you as a user).  Files you receive may have an array of issues such as missing/extra separators; primary key duplication; missing/extra columns etc.  It is a good idea to take your time at this stage to explore these issues and see how they manifest in RDMP.

From the Home screen under Catalogue select `New...=>Catalogue From File...`

After entering the target database make sure to click `Confirm Database` then choose a [Pipeline], for now any of the default CSV pipelines should work.

![Importing a file](Images/UserManual/ImportCsv1.png)

RDMP will automatically detect column datatypes as it loads the data.  An initial batch of data is inspected to determine initial datatype but if larger/longer data is encountered later this is updated and an `ALTER COLUMN` command sent to the DBMS.  This streaming approach allows RDMP to load very large files (multiple Gigabytes) without running out of memory.

If the import fails you can click the error icon to see error including the stack trace.

Using the 'manual column-type' [Pipeline] will give you a chance to change the column types created (calculated by RDMP):

![Overriding column datatypes](Images/UserManual/ImportCsv2.png)

You should now proceede to [Configure Extractability] for the Catalogue

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewCatalogueByImportingFile "./Biochemistry.csv" "chi" "DatabaseType:MicrosoftSQLServer:Name:RDMP_ExampleData:Server=(localdb)\MSSQLLocalDB;Integrated Security=true" "Pipeline:*Bulk INSERT*CSV*automated*" null
> ```

## Import Existing Table

If your data is already in a relational database (MySql, Sql Server etc) then you can import it into RDMP.  This will create a metadata object in RDMP storing the location of the data but will not move any data.

From the Home Screen select `New...->Catalogue From Database...`

Enter your servername (and optionally the username/password if using sql authentication). Choose the database and table you want to import.  Importing views (any [DBMS]) and table valued functions (Sql Server only) as [Catalogue] is fully supported.

![Import existing table](Images/UserManual/ImportExistingTable.png)

After importing you wil be prompted to [configure extractability](#configure-extractability)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewCatalogueByImportingExistingDataTable "Table:Biochemistry:DatabaseType:MicrosoftSQLServer:Name:RDMP_ExampleData:Server=(localdb)\MSSQLLocalDB;Integrated Security=true" null 
> ```

If you have many tables in the same database you can bulk import them from the windows client:

![Configure Catalogue column extractability](Images/UserManual/BulkImportExistingTable.png)

## Configure Extractability

Once a new [Catalogue] has been imported you will be presented with a dialog that allows you to make a decisions about which columns should be available to researchers in project extracts (extractable) and which should be kept private.  You will also be asked to select which column(s) are the linkage identifier that can be used to link between datasets (e.g. chi).

![Configure Catalogue column extractability](Images/UserManual/ConfigureExtractability.png)

> **[Command Line]:** You can configure extractability from the CLI for example:
> ```
> ./rdmp ChangeExtractability Catalogue:Biochemistry true
> ./rdmp Delete ExtractionInformation:patient_triage_score
> ```

# Data Quality
There are two summarisation components to RDMP.  The first is the [DQE Data Quality Engine](./Validation.md).  This allows you to create row level validation rules for the columns in your datasets (If column A is populated then column B should also have a value in it, column C must match Regex Z etc).  The results of DQE executions are stored longitudinaly in the DQE database, this allows you to pipoint when your data became corrupt or inspect the differences in quality before and after a data load at any time.

The second summarisation component are [Aggregate Graphs](./Graphs.md).  These are real time charts which provide a live view of the data in your repository.  Aggregate graphs can be reused during cohort identification and data extract building for testing filter configurations.  For example you could build a graph showing  'All drugs prescribed over time' and reuse it in a cohort identification set 'People prescribed painkillers' to confirm that you have configured your query filters correctly.

Graphs can be marked Extractable which allows you to run them on [ExtractionConfigurations].  This provides an overview of the subset of data provided to a project in their extract.

# Dashboards

RDMP Allows you to build modular dashboards from a range of available components that let you monitor the healthiness of your datasets, the extent of your metadata cover etc.  This API is also tied into the [plugins system](./PluginWriting.md) and is intended to be expanded upon with custom components.  

There are 3 components supplied out of the box.  These are

|Component|Code|Role|
|--|--|---|
|GoodBadCataloguePieChart|[Source](../../Rdmp.UI/PieCharts/GoodBadCataloguePieChart.cs) | Provides an overview of metadata completeness (how many columns have descriptions) |
|DatasetRaceway|[Source](../../Rdmp.UI/Raceway/DatasetRaceway.cs) | Provides a vertical chronological view of data quality across all your datasets |
|DataLoadsGraph|[Source](../../Rdmp.UI/Overview/DataLoadsGraph.cs)| Shows the last recorded state of data loads (passing/failing)|


![RDMP Dashboards](Images/UserManual/Dashboards.png)
            

# Sql Code Management

One difficulty facing any long running data management agency is how to preserve and document the code files/tools used for data loading, linkage and extraction.  RDMP avoids the need for lengthy script files by dividing code into conceptual blocks.  Once created, these blocks are stored as reusable documented components in the Catalogue database.  When an analyst needs to use a given set of concepts they can assemble a query using drag and drop without having to worry about the underlying code implementation.

The core blocks that are curated by RDMP are:

|Component| Role|
|--|--|
| [Filter] | Reduce the number of records matched using a boolean query.  Each filter results in a  single line of `WHERE` sql.|
| [FilterContainer]| Allows for combining of multiple Filters.  Each container results in brackets, indentation and either the `AND` or `OR` sql keywords|
| [ExtractionInformation]| Defines governance around an extractable column and whether there is any transform (UPPER etc).  Each of these results in a single line of `SELECT` sql |

There are many macro components (e.g [Catalogue], [AggregateConfiguration], [ExtractionConfiguration]) that are composed of these sub blocks but these are covered elsewhere.  It is the curation of logic as such small, documented, reusable atomic blocks which makes RDMP unique.

## Filters

Extraction Filters are lines of WHERE SQL which can be used as part of cohort creation, data summarisation and project extraction.  Once created and documented a filter can be reused in any context simply by adding a reference to it.  In this example we will create a ‘Diabetic drugs' filter on the prescribing dataset.

You can create a new filter by using the right click context menu on any [ExtractionInformation]:

![Adding a new filter via right click context menu](Images/UserManual/AddFilter.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewFilter ExtractionInformation:FormattedBnfCode null "Diabetic Drugs" "LEFT ( [FormattedBnfCode] , 3) = '6.1'"
> ```

You can test that an [ExtractionFilter] is working correctly by viewing what data it matches

![View Filter Data context menu](Images/UserManual/ViewFilterData.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp ViewFilterMatchData "ExtractionFilter:Diabetic Drugs" Aggregate
> ```

### Parameters

When updating a filters WHERE sql you can enter a parameter (e.g. @bnfCode).  When saved this will create a new parameter for the filter.  By default this will have the datatype `varchar(50)` and a value of `'todo'`.  Enter a placeholder value and a Comment to describe the role that the parameter plays in the filter.

When the filter is deployed in an [ExtractionConfiguration] or [CohortIdentificationConfiguration] the analyst will be able to specify a different parameter each time.  It is even possible to deploy multiple copies of the same filter with the only difference being the value of the parameter (e.g. inside a [FilterContainer]).

## Extraction Transforms

Sometimes you need to be able to provide a given column in more than one format. Or you may want to hold it in one format and release it in another.  The most common use case is sensitive or identifiable data.  RDMP allows you to write SQL code to change how a given column is extracted (or create multiple versions) and assign it an extraction category (Core, Supplemental, Special Approval etc).

For example you may call a scalar function on the underlying column (e.g. `UPPER` or `dbo.MakeAnonymous(mycol)`).  These are referred to as 'Extraction Transforms' because they transform the data before it is provided in a research extract.

In mature agencies such transforms are often already modelled in views and/or table valued functions.  If this is the case then these should simply be imported directly into RDMP as new [Catalogues] (RDMP supports both views and table valued functions).

### Creating an Extraction Transform

When a column is marked extractable in RDMP, an [ExtractionInformation] is created.  This defines what SQL is executed in the `SELECT` block of any SQL query built including that column (e.g. in an [ExtractionConfiguration]).  This SQL defaults to the fully qualified column name of the underlying column (e.g. `[RDMP_ExampleData].[dbo].[Prescribing].[FormattedBnfCode]`).    This means no transformation takes place.

You can update the SQL by opening it.  For example you could update it as follows:

```sql
UPPER( [RDMP_ExampleData].[dbo].[Prescribing].[FormattedBnfCode]) as FormattedBnfCode
```

Make sure that you always provide the column alias when defining a transform (e.g. `as FormattedBnfCode`)

![Open ExtractionInformation to change its SQL](Images/UserManual/CreateTransform.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp Set "ExtractionInformation:FormattedBnfCode" SelectSql "UPPER( [RDMP_ExampleData].[dbo].[Prescribing].[FormattedBnfCode]) AS FormattedBnfCode" 
> ```

# Data Load Engine

Many datasets are poorly formed, change structure over time and/or require significant transform/cleaning before they are useful for statistical analysis.

Often the technical implementation of each datasets ETL is known only to the data analysts who built the original data load (and who might not even work at the agency any more) or the ETL may be built into bespoke legacy applications.  
The RDMP data load engine was developed by the Health Informatics Centre at a time when it was struggling to cope with reliable data loading of a high volume of clinical datasets using a mixture of commercial packages such as SSIS, bespoke dataset specific applications and undocumented SQL stored procedures.  The design core requirements for the Data Load Engine were:

- Eliminate the possibility of duplication by loading the same source rows multiple times
  -DLE requires primary keys that come from the source data provider
- Prevent failed loads from impacting the live dataset (e.g. crashing half way through a load)
  - All data is loaded first into an unstructured unconstrained RAW environment
  - Then loaded into mirror area called STAGING which matches the LIVE environment
  - Finally, the load is committed as a MERGE from STAGING to LIVE
  - Any failure at any step results in zero affected changes to the LIVE dataset
- Centralise load logic into one place that is accessible to every data analyst
- Handle strange file types and bespoke/proprietary file layouts
  - Plugin architecture allows agency specific requirements 
- Usable by data analysts who familiar only with SQL
- Ensure reusability of components where possible (e.g. where previously each load application would use a different FTP library and store credentials in different places or be hard coded or be a manual task!) 
- Be automatable through a single point of automation

It is important to note that these features are designed for the robust loading of routine data (supplied on a regular basis).  For one off loads use the [File Import](#importing-a-flat-file-as-a-new-dataset) feature.

RDMP is primarily a data management tool and therefore does not seek to replace existing ETL tools that you might use for complex data transforms.  It's data load engine is optimised for rapidly and safely loading and versioning data tables in as close a schema to the original source as possible (e.g. if a dataset is produced by a clinical scanner and comes out in with 30 headers then the data should ideally be held in your repository in the same format).  This not only simplifies data load but means there is no bias / interpretation introduced at the data management layer, researchers receive the data exactly the same way you receive it.  The only time extensive transformation is required during ETL is when the data includes duplication / errors that make the data otherwise unloadable.

## RAW Bubble, STAGING Bubble, LIVE Model

Core to the data load process implemented in RMDP is the migration of the data you are trying to load through increasingly structured states (RAW=>STAGING=>LIVE).

![RAW, STAGING, Live model](Images/FAQ/DLEDiagram.png)

The purpose of the RAW/STAGING/LIVE Model is:
- Isolating failed loads from affecting live data
- Divide data load problems into either loading (RAW) or database constraint /anonymization issues (STAGING)
- Allow ALTER / UPDATE logic e.g. to merge columns etc to be done in a safe environment (RAW) with a language the user is familiar with (SQL).
- Freezing failed data in the state in which it failed so analysts can evaluate it

![RAW, STAGING, Live model](Images/UserManual/DLEDiagram2.png)

There are many reasons why a dataset load might fail.  Some of the most common scenarios encountered by HIC include:
1. Data supplier changes the name of a column(s)
    - Is there a semantic change to the data that should be documented?
    - Is the contents of the column still rational with previously loaded data?
2. A column starts containing null values where previously it was fully populated
    - Is the column still conceptually the same as before? 
    - Is it that the provider has started coding 0 values as null?
    - Is it valid that the column contains nulls and the LIVE schema should be updated?
3. Data supplier has primary key duplication within a load file
    - Is it the analyst/dataset which has the wrong primary keys?
    - Is it possible to resolve by merging missing data into a composite record?
    - Is it possible to identify one record which is clearly wrong / out of date?

Of the above cases, the first would break on loading to RAW and require adjustments to the load configuration (which would then work on all subsequent loads).   This is because even though RAW does not have constraints (primary key etc) it follows the same column naming and datatypes as LIVE.  Cases 2 and 3 would fail when trying to move from the unconstrained RAW to the constrained STAGING bubbles.  In both cases after the load crashes the RAW database would be left accessible so the analyst could run SQL queries against it to explore the problem data directly (without having to resort to peering at the source file).

## Error Messages

The DLE tries to give as much information about problems as possible.  Every message generated by the system (either during pre-load checking or loading data) is auditted to the logging server as well as displayed in an interactive data load user interface.  It is intended that the data loader user interface be used to debug failing loads and build new loads but that once a load is stable it be setup as an automated run via the [RDMP CLI].

Commonly encountered errors (missing columns, extra columns, data too wide etc) will always provide context that can be used to help resolve the problem.  For example

```
BulkInsert failed on data row 155, the complaint was about source column <<ApprovedName>> which had value <<SODIUM LAURYL ETHER SULFATE WITH SODIUM LAURYL ETHER SULPHOSUCCINATE>> destination data type was <<varchar(56)>>
Received an invalid column length from the bcp client for colid 9(Source Column <<ApprovedName>> Dest Column <<ApprovedName>> which has MaxLength of 56).
```

Since error messages may contain identifiable data, it is important to secure the logging database with the same rigour as your live data repository.

## Archive Tables

All tables loaded by the DLE must have a primary key.  This prevents duplication and allows for 'UPSERT' of new batches such that only new/different records are loaded.

In order to keep a backup of replaced data the DLE creates an `_Archive` table for each table in a load.  This provides a longitudinal history of each row (by primary key).  It is done to provide traceability and reproducibility of data extracts as well as to investigate changes in data quality etc.  This process is transparent to the analyst building the load and requires only that appropriate primary keys exist in the source data and are enforced in the repository data table.


![Archive tables](Images/UserManual/ArchiveTables.png)

The technical implementation for this process is to issue an INSERT INTO sql command for all unique primary keys and then a MERGE sql command for the remainder.  The versioning of old records is achieved using an SQL Trigger which is applied on UPDATE and inserts the record into the archive table.  This process uses a transaction.  

[Implementations of the archive trigger](../../Rdmp.Core/DataLoad/Triggers/Implementations/TriggerImplementerFactory.cs) exist for all [DBMS] supported by RDMP.


INSERT and MERGE are both very fast operations which scale to very large volumes of data (Gigabytes of records).  The archiving trigger however does not scale linearly with dataset size.  Rather it scales against the volume of records which change in a load.  If data loads are highly volatile (rather than incremental updates) then it may be prudent to disable this feature:

![Disable Archive tables](Images/UserManual/DisableArchiving.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp Set "LoadMetadata:*Prescribing*" IgnoreTrigger true 
> ```

*Note that disabling the trigger will not delete any existing trigger that is already in place*.  This can be manually dropped using SQL appropriate to your [DBMS] e.g.:

```csharp
drop trigger Prescribing_OnUpdate;
```

## Creating a Data Load Configuration

Each data load in RDMP has a single [LoadMetadata] object which acts as the root for the configuration.  A given configuration will load one or more tables (which must have primary keys).  The tables loaded includes all the tables that underly the [Catalogues] which are ­associated with the [LoadMetadata].  For example if a Catalogue 'Biochemistry' includes a join on two tables 'Header' and 'Results' then a [LoadMetadata] which includes 'Biochemistry' would include both the 'Header' and 'Results' tables.

Create a new Biochemistry CSV file with [SynthEHR] and add a primary key to the 'Biochemistry' table in your example data.  The key should be a composite primary key of:

- chi
- SampleDate
- TestCode

![Create primary key from TableInfo context menu](Images/UserManual/CreatePrimaryKey.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp AlterTableCreatePrimaryKey TableInfo:*Biochemistry* ColumnInfo:*Biochemistry*chi* ColumnInfo:*SampleDate* ColumnInfo:*TestCode*
> ```

If your dataset already has a primary key then drop it and Sync the RDMP [TableInfo].  The SQL to do this will depend on your [DBMS].  For example in SQL Server it is:

```sql
select * from sys.indexes
alter table Biochemistry drop constraint PK__Biochemi__91F86285DDF364F8
```

Create a new [LoadMetadata] by clicking New under Data Load on the home screen or the ''Load Configurations' tree collection.

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewLoadMetadata Catalogue:Biochemistry
> ```

All loads require a file system to load data from.  This is the case even if data comes from another source (e.g. remote database table).  Set the load directory to a location on disk.  If you intend to run the load from a separate server (e.g. automation) then this should be a network drive so that data analysts and automation server can both access it.

![Set the load directory](Images/UserManual/ChooseLoadDirectory.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewDataLoadDirectory LoadMetadata:*Biochemistry c:/temp/biochem
> ```

## Configuring the load

All loads in RDMP are composed of 1 or more [ProcessTask].  Each [ProcessTask] operates on a specific stage of the load pipeline and does a single activity (de-duplicate records, load files from CSV etc).  

There are 5 categories of [ProcessTask]
|Category | Purpose|
|--|--|
|Executable | Runs a specified .exe file |
|SQLFile | Runs a specified block of SQL on it's associated load stage database (e.g. RAW) |
| DataProvider| Moves files around e.g. downloading from an FTP server |
| Attacher | Loads records into the RAW database e.g. from CSV or remote table |
| MutilateDataTable | Modifies table data in RAW/STAGING to resolve specific issues (primary key collisions etc) |

The simplest load is one that simply has a CSV attacher.  Add the 'AnySeparatorFileAttacher' to the 'Mounting' stage of the load:

![Adding a delimited file attacher](Images/UserManual/AddAttacher.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewClassBasedProcessTask LoadMetadata:*Biochemistry Mounting AnySeparatorFileAttacher
> ```

Set the following properties:
|Property|Value|
|--|---|
|Separator|`,`|
|FilePattern|`*.csv`|
|TableName|`Biochemistry`|

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp SetArgument "ProcessTask:AnySeparatorFileAttacher" Separator ","
> ./rdmp SetArgument "ProcessTask:AnySeparatorFileAttacher" FilePattern "*.csv"
> ./rdmp SetArgument "ProcessTask:AnySeparatorFileAttacher" TableName "Biochemistry"
> ```

### Running the load

Place the Biochemistry.csv file generated by [SynthEHR] into the ForLoading directory of your load (e.g. `C:\temp\biochem\Data\ForLoading`).

Right click the [LoadMetadata] and select 'Check and Execute'

![Checking the data load](Images/UserManual/CheckAndExecute.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp.exe dle -l "LoadMetadata:*Biochemistry" --command check
> ```

If the checks propose changes such as creating the archive trigger you should accept them.  If you are unsure if it worked you can run the checks again.

When running from CLI check fixes are automatically accepted.

Run the data load engine by pressing the green go button.

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp.exe dle -l "LoadMetadata:*Biochemistry" --command run
> ```

If your load fails and you are able to fix the problem then you must run the checks again before trying to execute the load again (this also applies to CLI use).

Failed loads are deliberately left in the state in which they crash so that the RAW/STAGING databases can be inspected to assist in diagnosis.

![Viewing temporary tables created during the data load (in the event of failure)](Images/UserManual/ViewLoadDiagram.png)

### Resizing a column

One common issue you may encounter is when new data is wider than the existing schema.  If you encounter this you can resize the column in RDMP.  You can use Ctrl+F to quickly find a column by name and jump to it in the tree collection.

![Altering the column type](Images/UserManual/AlterColumnType.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp.exe AlterColumnType ColumnInfo:*QuantityUnit* "varchar(20)"
> ```

### Archiving

After a successful load all files in 'ForLoading' are zipped up and put into 'ForArchiving'.  The ID of the load is the file name of the zip.  This allows you to track an entry in the load audit logs to the file that was loaded at the time.

### Data Load Engine Logging

All messages created during the load are stored in the hierarchical logging database along with counts of the number of 

![Access the View Logs command from the right click context menu of the LoadMetadata](Images/UserManual/ViewLogs.png)


> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp.exe ViewLogs LoadMetadata:*Biochemistry
> ```

# Cohort Builder

Cohorts are lists of identifiers which uniquely identify subjects and can be used to for dataset linkage, anonymisation and extraction.

Cohort identification is often the most complicated parts of meeting projects' extraction needs.  It is a sensitive task with considerable risk potential (for example incorrectly contacting patients about conditions they don't actually have).

There are 2 stages to creating a cohort before it can be used to generate an extraction.

|Stage| Purpose|
|---|---|
|Identify Cohort| Build a query or locate a file that contains unique person identifiers |
|Commit Cohort| Create a permanent record of the results of the query / contents of the file and allocate any anonymous mappings|

## Identify Cohort

Each Cohort starts life either as a list of identifiers in a file or a [CohortIdentificationConfiguration] (Query Builder).  

If identifiers are in a file then they can be [directly committed](#commit-cohort) into the Saved Cohorts store.  If additional restrictions must be placed on the identifier list (e.g. removing Opt outs, people with specific conditions) then the identifiers should be [imported as a new Catalogue](#importing-a-flat-file-as-a-new-dataset) and a [CohortIdentificationConfiguration] should be built using it.  If an identifier list is imported as a [Catalogue] you can mark as 'Project Specific' which will tie it to a single [Project] and ensure it is not mistakenly used in other projects/extracts.

If you need to assemble a query to identify the cohort then you will need to create a [CohortIdentificationConfiguration] in the Cohort Builder.

![Create a new Cohort Identification Configuration from home](Images/UserManual/NewCohortBuilderFromHome.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewCohortIdentificationConfiguration "My First Cohort"
> ```

Add the Biochemistry dataset to the 'Inclusion Container'.

![Add Catalogue to Cohort Identification Configuration](Images/UserManual/AddCatalogueToCohort.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp AddCatalogueToCohortIdentificationSetContainer CohortAggregateContainer:6 Catalogue:Biochemistry chi
> ```
_The above query uses the ID of the container (6).  We cannot use the name (unless [running in a script](./RdmpCommandLine.md#scripting)) because there are likely to be multiple containers that share that name_
 
You can disable the red 'problem' indicators by disabling them in user settings.  This is worth doing as they will prevent the cohort from being committed.

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp SetUserSetting StrictValidationForCohortBuilderContainers false
> ```

Add a filter to the dataset.  Either an existing one (if you have any [Catalogue] level [Filters]) or a new empty one (if empty provide some WHERE sql).

![Adding a filter](Images/UserManual/AddFilterToAggregateConfiguration.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
>  ./rdmp CreateNewFilter "AggregateConfiguration:People in Biochemistry" null "Creatinine" "TestCode like '%CRE%'"
> ```

Execute the cohort.  You can access various view options from the right click context menu:

![View data from a run aggregate](Images/UserManual/AggregateConfigurationViewData.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
>  ./rdmp ViewData "AggregateConfiguration:People in Biochemistry" TOP_100
>  ./rdmp ViewData "AggregateConfiguration:People in Biochemistry" All
> ```

### Cohort Containers

New [CohortIdentificationConfigurations] are created with 3 containers:

- A Root Container
  - Inclusion Criteria
  - Exclusion Criteria

The first container is an [EXCEPT] while the other two are [UNION].  For more information on how these work see the [Cohort Builder FAQ](./FAQ.md#cohort-builder-overview) or [Cohort Creation](../../Rdmp.Core/CohortCreation/CohortCreation.md).

## Commit Cohort

Cohort Committing is the process of 'finalising' a list of identifiers.  This is important for 
reproducibility and anonymisation.

To commit a cohort you will need

- A source of the identifiers (e.g. [CohortIdentificationConfiguration] or file containing identifiers)
- A place to store the identifiers

### Saved Cohorts

Identifier lists are shown in the 'Saved Cohorts' collection accessible from the top toolbar of RDMP

![Saved Cohorts collection showing source and 1 stored list](Images/UserManual/SavedCohorts.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
>  ./rdmp describe ExtractableCohort
> ```

Assuming you created example datasets when installing RDMP you should see the [ExternalCohortTable] RDMP_ExampleData.
If you open this store you can see that it stores lists where the 'private identifier' is `chi` and the anonymous
identifier is `ReleaseId`.

![Database location of cohort db](Images/UserManual/SavedCohortsTable.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
>  ./rdmp describe ExternalCohortTable:RDMP_ExampleData
> ```

Each identifier format that you use for linkage requires its own [ExternalCohortTable].  For example if you perform 
cohort building / linkage with both `NHSNumber` and `SocialSecurityNumber` then you would need 2 [ExternalCohortTable].

You can create a new [ExternalCohortTable] with the wizard

![Creating a new ExternalCohortTable with wizard via context menu](Images/UserManual/CreateNewSavedCohortsTable.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
>  ./rdmp CreateNewCohortStore "DatabaseType:MicrosoftSQLServer:Name:RDMP_ExampleCohorts2:Server=(localdb)\MSSQLLocalDB;Integrated Security=true" false chi "varchar(10)"
> ```

### Cohort Pipeline

To put the identifiers into the [ExternalCohortTable] you will need to run a 'Cohort Creation' [Pipeline].

A list of all [Pipelines] can be seen in the in the 'Tables (Advanced)' collection:

![Viewing what Pipelines exist](Images/UserManual/ViewPipelines.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
>  ./rdmp ls Pipeline
> ```

The easiest place to commit a cohort from is the 'Project' collection New button:

![Run a cohort pipeline](Images/UserManual/CommitCohortContextMenu.png)

Choose your [CohortIdentificationConfiguration] and the 'RDMP_ExampleData' destination.

When prompted to choose a [Project] pick the existing example [Project] 'Lung Cancer Project'.
Select 'New Cohort' and give it a name.

![Enter details of the cohort you are trying to create](Images/UserManual/CommitCohortDescribeIt.png)

Preview and then Run the [Pipeline].  Ensure that you have the correct Pipeline for the task (e.g. if uploading a file, use the 'From CSV File').

If you get an error and are using a [CohortIdentificationConfiguration], open it in Cohort Builder and ensure that it runs correctly there and that there are no lines highlighted red.  You may need to disable StrictValidationForCohortBuilderContainers (see [Identify Cohorts](#identify-cohorts)).

![Run Pipeline](Images/UserManual/RunCohortCommitPipeline.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewCohortByExecutingACohortIdentificationConfiguration "CohortIdentificationConfiguration:my new cic" ExternalCohortTable:RDMP_ExampleData "My test cohort" null "Pipeline:CREATE COHORT*Cohort Identification Configuration"
> ```

After the pipeline completes you should see your new cohort under 'Saved Cohorts'.

# Release Identifier Allocation

If you view the data in an [ExtractableCohort] you can see two columns.  The first will be whatever column in your dataset you use for linkage (e.g. chi) and the second will be the anonymous representation (e.g. 'ReleaseId').

![View TOP 100 on cohort](Images/UserManual/ViewCohortTop100.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp ViewData ExtractableCohort:1 ALL
> ```

You can tailor how these identifiers are allocated in several ways.  For example

- [Do not allocate at all](#do-not-allocate-release-identifiers)
- [Allocate Manually](#allocate-release-identifiers-manually)
- [Provide custom allocation method (i.e. a Plugin)](#allocate-release-identifiers-with-plugin)

## Do Not Allocate Release Identifiers

If your data is already anonymised or is not sensitive then you may not want separate release identifiers at all.  Alternatively you may have a separate process for allocating identifiers (e.g. a find/replace script).

In this case you can disable release identifier allocation by editing the [ExternalCohortTable] such that the 'Private Identifier Field' is the same as the 'Release Identifier Field':

![Setting private and release identifier fields to the same](Images/UserManual/IdentifiableExtractions1.png)

You will also need to update your User Settings to allow for this:

![Update R004 to Success](Images/UserManual/IdentifiableExtractions2.png)

Finally you will need to update the schema of the Cohort table to allow nulls.  The code will vary but this is Sql Server code for the default example [ExternalCohortTable] created on install:

```sql
ALTER TABLE RDMP_ExampleData..Cohort ALTER COLUMN ReleaseId varchar(300)
```

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp set ExternalCohortTable:RDMP_ExampleData ReleaseIdentifierField "[RDMP_ExampleData]..[Cohort].[chi]"
> ./rdmp SetUserSetting R004 Success
> ```
_Note that UserSettings are not shared between CLI and Windows application_


## Allocate Release Identifiers Manually

You can leave identifiers blank in the [ExternalDatabaseServer] table by editing the [Pipelines] and changing the `ReleaseIdentifierAllocation` property on the destination component to [NullAllocateReleaseIdentifiers](./../../Rdmp.Core/CohortCommitting/Pipeline/Destinations/IdentifierAllocation/NullAllocateReleaseIdentifiers.cs).

![Edit Pipeline and change ReleaseIdentifierAllocation property](Images/UserManual/EditPipelineNoAllocate.png)

As before you will need to ensure that the database allows nulls in the release identifier column (See above).

## Allocate Release Identifiers With Plugin

RDMP supports both [Plugin] destination components and allocation methods.  If you simply want to change what identifiers are allocated then define a new class implementing [IAllocateReleaseIdentifiers](../../Rdmp.Core/CohortCommitting/Pipeline/Destinations/IdentifierAllocation/IAllocateReleaseIdentifiers.cs).

If you have something more complicated in mind you can create a new class implementing [IPluginCohortDestination](../../Rdmp.Core/CohortCommitting/Pipeline/IPluginCohortDestination.cs). 

In both cases you will need to package your code as a plugin, see [Plugin Writing](./PluginWriting.md).

# Pipelines

[Pipelines] provide flexibility when moving data from place to place (e.g. when [creating a cohort](#cohort-pipeline) or [uploading a file as a new table](#importing-a-flat-file-as-a-new-dataset).

A [Pipeline] consists of a source component generating data (e.g. reading a file) and a series of components operating on the data then a destination component (e.g. writing final data into a database).  

In some situations the source and/or destination may be fixed by the use case.  When this happens the [Pipeline] cannot specify it's own.

[Pipelines] are usually configured once and then reused for the task from then on.  You may find that you define multiple [Pipelines] for the same task (e.g. File Import) that are configured slightly differently (e.g. one for comma separated and another for tab separated).

Each [PipelineComponent] has its own arguments that can be configured.  For example the BULK INSERT [Pipeline] source has settings like `Separator` and `ThrowOnEmptyFiles`.

![Expanded tree showing all arguments on DelimitedFlatFileDataFlowSource (e.g. Culture)](Images/UserManual/PipelineTree.png)

When you edit a [Pipeline] you are able to select and delete components as well as drag new components into the middle (or source/destination) to perform specific activities.  For example if you need to replace values in the pipeline with a mapping you can use [ColumnSwapper](../../Rdmp.Core/DataLoad/Modules/DataFlowOperations/Swapping/ColumnSwapper.cs).

Defining your own custom components is fun and easy and covered in [PluginWriting.md](./PluginWriting.md).

# Extraction

Data export in RDMP allows you to link a cohort to one or more dataset(s).  This will extract only those records in a dataset that belong to people in your cohort.  This linkage is done through the user interface by creating a [Project] and one or more [Catalogue].  A [Project] can have one or more [ExtractionConfiguration] (e.g. cases and controls).

In order to be included in an [ExtractionConfiguration] a [Catalogue] must be marked extractable and have a column marked [IsExtractionIdentifier].  When added you can define [Filters] to further constrain what records are extracted (e.g. prescriptions for a specific drug group only).

RDMP preserves all [ExtractionConfiguration] separate from [Catalogue] definitions.  This ensures reproducibility of extracts even at a far later date (e.g. years).

[ExtractionConfiguration] can be frozen after release to prevent accidental changes.  They can even be cloned for later reuse (e.g. in a data refresh after 6 months).

## Configuring an Extraction

Once you have [created a cohort and Project](#commit-cohort) you are ready to create a new [ExtractionConfiguration].  This can be done from the right click context menu on any [Project] or the 'Extraction Configurations' node. 

Provide a name for the [ExtractionConfiguration] then pick your cohort and initial [Catalogues].

![Add an ExtractionConfiguration to a Project via context menu](Images/UserManual/AddExtractionConfiguration.png)


> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewExtractionConfigurationForProject "Project:Lung Cancer Project" "My test config"
> ./rdmp ChooseCohort "ExtractionConfiguration:My test config" "ExtractableCohort:MyCohortVersion1"
> ./rdmp AddDatasetsToConfiguration ExtractableDataset:Biochemistry "ExtractionConfiguration:My test config"
> ./rdmp AddDatasetsToConfiguration ExtractableDataset:Prescribing "ExtractionConfiguration:My test config"
> ```

Now check and execute the extraction:

![Run an ExtractionConfiguration via context menu](Images/UserManual/ExecuteExtractionConfiguration.png)


> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp extract -e "ExtractionConfiguration:My test config" -p "Pipeline:DATA EXPORT*CSV" --command check
./rdmp extract -e "ExtractionConfiguration:My test config" -p "Pipeline:DATA EXPORT*CSV" --command run
> ```
_Check then execute an ExtractionConfiguration.  Note that when specifying the Pipeline name we use the wild card '*'.  In this case it is to avoid the ':' which cannot appear in name pattern on CLI_

## Verification

You can confirm that an extraction was successful by opening the extraction location on disk or by running the Extraction SQL.

![Check extraction matches expectations](Images/UserManual/CheckExtraction.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp ViewExtractionSql "ExtractionConfiguration:My test config" Catalogue:Prescribing
> ```
_Note that this will view both the SQL query and all the data returned_


## Adding filters

You can add or import [Filters] to reduce the scope of the extract.  For example if a study only requires specific types of records (e.g. Diabetic Drug Prescriptions).  The cohort ensures that only records for specific people are extracted.  Filters further reduce that according to WHERE SQL in them.

![Adding a filter to Prescribing](Images/UserManual/AddFilterToExtraction.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp CreateNewFilter "SelectedDatasets:22" null "Diabetic Drugs" "LEFT ( [FormattedBnfCode] , 3) = '6.1'"
> ```
_Note that this will view both the SQL query and all the data returned_

## Scalability and Adaptability

Since data repositories can contain hundreds of millions of records and cohorts can be equally large, RDMP data export is designed for scalability.   Datasets are extracted in parallel and results are streamed in batches (to reduce RAM overhead or out of memory errors).

As seen above the CLI can be used to execute [ExtractionConfigurations].  This allows for a workflow where data analysts build and check the configuration but the running is then done by a powerful automation server overnight.

Extractions are executed through a [Pipeline] (See [Pipelines chapter](#Pipelines)). This allows for extraction to database or any other destination for which a component exists.  Plugin components can be written and added to extraction pipelines e.g. to perform further identifier substitutions (see [Plugin Writing]).

# Release

An [ExtractionConfiguration] can have many [Catalogues].  These can be extracted at different times.  Some may acquire modifications over time (adjusting filters etc).  Some may fail to extract (e.g. bad SQL in filter) or have extracted files lost (e.g. disk backup restore).

The Release process of RDMP confirms that all expected extraction artifacts are in place and that the extraction audit matches the current live configuration of the [ExtractionConfiguration] (no changes have been made but not run).

Releases use the [Pipeline system](#Pipeline) which allows for customization.  For example a [Plugin] could enable releases to an SFTP server etc.

You can start a release of one or more [ExtractionConfigurations] via the [Project] context menu.

![A starting a release](Images/UserManual/RunRelease.png)

> **[Command Line]:** This can be done from the CLI using:
> ```
> ./rdmp release -c "ExtractionConfiguration?Project:Lung Cancer Project" -p "Pipeline:RELEASE*To Directory" --command check --skipreleased
> ./rdmp release -c "ExtractionConfiguration?Project:Lung Cancer Project" -p "Pipeline:RELEASE*To Directory" --command run --skipreleased
> ```
_The question mark after ExtractionConfiguration means match all where the Project matches the pattern ('Lung Cancer Project')_


[AggregateConfiguration]: ./Glossary.md#AggregateConfiguration
[SynthEHR]: https://github.com/HicServices/SynthEHR
[Catalogue]: ./Glossary.md#Catalogue
[Catalogues]: ./Glossary.md#Catalogue
[CohortIdentificationConfiguration]: ./Glossary.md#CohortIdentificationConfiguration
[CohortIdentificationConfigurations]: ./Glossary.md#CohortIdentificationConfiguration
[Command line]: ./RdmpCommandLine.md
[DBMS]: ./Glossary.md#DBMS
[EXCEPT]: ./Glossary.md#EXCEPT
[ExternalCohortTable]: ./Glossary.md#ExternalCohortTable
[ExtractableCohort]: ./Glossary.md#ExtractableCohort
[ExtractionConfiguration]: ./Glossary.md#ExtractionConfiguration
[ExtractionConfigurations]: ./Glossary.md#ExtractionConfiguration
[ExtractionFilter]: ./Glossary.md#ExtractionFilter
[ExtractionInformation]: ./Glossary.md#ExtractionInformation
[FilterContainer]: ./Glossary.md#FilterContainer
[Filter]: ./Glossary.md#ExtractionFilter
[Filters]: ./Glossary.md#ExtractionFilter
[IsExtractionIdentifier]: ./Glossary.md#IsExtractionIdentifier
[PipelineComponent]: ./Glossary.md#PipelineComponent
[PipelineComponents]: ./Glossary.md#PipelineComponent
[Pipeline]: ./Glossary.md#Pipeline
[Pipelines]: ./Glossary.md#Pipeline
[Plugin Writing]: ./PluginWriting.md
[ProcessTask]: ./Glossary.md#ProcessTask
[Project]: ./Glossary.md#Project
[RDMP CLI]: ./RdmpCommandLine.md
[TableInfo]: ./Glossary.md#TableInfo
[UNION]: ./Glossary.md#UNION