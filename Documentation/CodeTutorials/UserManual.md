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
You will need an Sql Server instance (unless using a [file system backend](./YamlRepository.md)).  If you do not already have one, you can use the Express edition for free which is available from microsoft.com (https://www.microsoft.com/en-us/sql-server/sql-server-editions-express ).  Alternatively you can use [LocalDb](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver16) which is a Sql Server development tool installable with Visual Studio that allows for instances to be started when needed and shutdown automatically.

You will also need to have your actual dataset data in a relational database.  RDMP supports accessing research data repositories stored in MySql, Sql Server and Oracle databases but stores it’s own metadata (platform databases) in Sql Server only.

If your dataset data is currently in flat files then RDMP can load them into a relational database for you via the data load engine.

## Platform Database Setup

The RDMP uses SQL Server databases* to store metadata (dataset/column descriptions, validation rules, validation results, data load configurations etc) as well as to store logging data, caching repository data etc.  These ‘platform databases’ are separate from your ‘data repository’ which is the location that you store your live research data.

The first time you start RDMP (ResearchDataManagementPlatform.exe) you will be prompted to create the metadata databases that allow RDMP to function.  The simplest approach is to create them all on a single server, to do this enter your Sql Server name and a prefix for the databases.

Choose 'Set Platform Databases...'

If you are a new user at a site where there is already a platform database you can connect to the existing instance.  Otherwise choose 'I want to create new Platform Databases'.

Enter the connection details of your server and any keywords that are required to access (custom ports, certificate validation options etc):

![Platform database setup](Images/UserManual/CreatePlatformDatabases.png)

> **Command Line:_**  Installation can also be performed from the [command line] using:
> ```
> ./rdmp install "(localdb)\MSSQLLocalDB" RDMP_ -e
> ```

This creates 4 databases which are the minimum set of platform databases required for RDMP to function.  The databses fulfil the following roles.

|Database|Role|
|--|---|
| Catalogue| Stores all descriptive, technical, validation, extraction information, attachments etc about your datasets (both externally accessible and internal).  Also stores Cohort Builder and data load configurations. |
| DataExport| Stores all project extraction configurations allowing for long term versioning and reproducibility into a variety destination formats (CSV, extract to database etc).  Also stores which final cohort lists were used with which extraction projects (Actual identifier lists are not stored in this database) |
| Data Quality Engine (DQE)| Stores a longitudinal record of the results of Data Quality Engine runs on your datasets.  This includes how many rows passed validation rules, how many rows there are per month etc. |
|Logging| Stores a hierarchical audit of all flows of data from one place to another.  This includes data being loaded, data being extracted and even internal processes like DQE executions. |

There are a couple of other database types which can be created as and when you need them.  These include:

|Database|Role|
|---|---|
| [Query Caching](./../../Rdmp.Core/CohortCreation/Readme.md)| Improves the performance of complex cohort identification configurations and anonymisation.  Also allows cross server and plugin Cohort Builder elements (e.g. to [REST APIs](./FAQ#apis))|
|Anonymisation|Provides a way of performing identifier dropping / substitution on data load for when you want an entirely anonymous data repository|
|[Plugin Databases](./FAQ#plugins)| RDMP supports plugins which can in some cases have their own database(s)|

_\* Unless using [file system backend](./YamlRepository.md)_

## Example Data
Example data can be setup on install by ticking the 'Example Datasets' checkbox during platform database setup.

New example data can be generated through the `Diagnostics=>Generate Test  Data` menu. Or with the [BadMedicine](https://github.com/HicServices/BadMedicine) command line tool. 

## Importing a flat file as a new dataset
If you have some CSV files (or Excel/Fixed Width) that you want to load into your database, you can do so with the RDMP bulk import tool.

Alternatively if you already have your data in a database then you can simply [tell RDMP where it is](#import-existing-table).

There are many problems that can occur in the daily handling of research data by data analysts.  It can be helpful to discover how the RDMP handles various problems and what problems it cannot handle (and how it communicates this to you as a user).  Files you receive may have an array of issues such as missing/extra separators; primary key duplication; missing/extra columns etc.  It is a good idea to take your time at this stage to explore these issues and see how they manifest in RDMP.

From the Home screen under Catalogue select `New...=>Catalogue From File...`

![Importing a file](Images/UserManual/ImportCsv1.png)

RDMP will automatically detect column datatypes as it loads the data.  An initial batch of data is inspected to determine initial datatype but if larger/longer data is encountered later this is updated and an `ALTER COLUMN` command sent to the DBMS.  This streaming approach allows RDMP to load very large files (multiple Gigabytes) without running out of memory.

Using the 'manual column-type' [Pipeline] will give you a chance to change the column types created (calculated by RDMP):

![Overriding column datatypes](Images/UserManual/ImportCsv2.png)

Importing a file can also be done from the [command line]:

```
./rdmp CreateNewCatalogueByImportingFile "./Biochemistry.csv" "chi" "DatabaseType:MicrosoftSQLServer:Name:RDMP_ExampleData:Server=(localdb)\MSSQLLocalDB;Integrated Security=true" "Pipeline:*Bulk INSERT*CSV*automated*" null
```

## Import Existing Table



[Command line]: ./RdmpCommandLine.md
[Pipeline]: ./Glossary.md#pipeline