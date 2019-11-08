# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

...

### Added

- Support for PostgreSql databases

### Changed

- Sql Server `..` syntax is no longer used (now uses `.dbo.` - or whatever the table schema is).  Since references can be shared by users the default schema notation is not good idea.
- Cohort Query Bulder will now connect to the database containing the data rather than the users default database when querying data on a single database
- Flat file Attachers now process files in alphabetical order (case insensitive) when Pattern matches multiple files (previously order was arbitrary / OS defined)

### Fixed

- Fixed handling of credentials where password is blank (allowed)
- Extracting a dataset using Cross Server extraction source now shows the correct SQL in error message when no records are returned by the linkage

## [3.2.1] - 2019-10-30

### Added

- SET containers ([UNION] / [INTERSECT] / [EXCEPT]) now highlight (as a `Problem`) when they will be ignored (empty) or not applied (when they contain only 1 child)

## Fixed

- Fixed bug generating metadata reports that include Catalogues with orphan ExtractionInformation (not mapped to an underlying ColumnInfo)
- Fixed bug in column descriptions pie chart where navigate to CatalogueItem(s) would show all CatalogueItems instead of only those missing descriptions
- Fixed bug in example dataset creation where views (vConditions and vOperations) were not marked IsView

## [3.2.1-rc4] - 2019-10-22

### Added 

- Errors during caching (of cohort builder results) now appear in the results control (previously could generate erro popups)
- Patient Index Tables are no longer allowed to have parameters with the same name (but different values) of tables they are joined against
- Sql Parameters (e.g. `@test_code`) now work properly cross [DBMS] (e.g. MySql / SqlServer) when using a query cache.
- Added menu for inspecting the state of a cohort compiler (view SQL executed, build log, results etc)

### Fixed 

- Fixed ExceptionViewer showing the wrong stack trace under certain circumstances
- Fixed cache usage bug where sql parameters were used in queries (cache would not be used when it should)
- Fixed 'View Dataset Sample' user interface generating the wrong SQL when a patient index table has a column alias (e.g. `SELECT chi,AdmissionDate as fish from MyPatIndexTable`)
- Fixed renaming parameters causing UI to incorrectly ask if you want to save changes

## [3.2.1-rc3] - 2019-10-21

### Fixed 

- Fixed bug in cross server query building when using parameters (@testcode etc)

## [3.2.1-rc2] - 2019-10-18

### Added 

- Added GoTo from cohorts to Extraction Configuration(s)

### Changed

- View ThenVsNow Sql in right click context menu of data extractions is only evaluated when run (improves performance).  This results as the command always being enabled.

### Fixed

- Fixed [bug in cross server query building](https://github.com/HicServices/RDMP/commit/a0c6223d1a7793bde4a67b368ae062e8bec3d960#diff-196fcda7990895e9f656c99602d1972b) (via cache) when joining patient index tables on one server to a main dataset on another

## [3.2.1-rc1] - 2019-10-14

### Added

- Long running processes that previously blocked the UI (e.g. create primary key) now have a small dialog describing task and allowing cancellation.
- Proposed Fix dialog now has standard look and feel of RDMP message boxes (including keywords etc)
- Double clicking an executing task in Cohort Builder now shows cohort build log as well as Exception (if any)

### Changed
 
- Database patching user interface presents clearer information about what version upgrade is occuring and the patches that will be applied.
- Updated to latest version of [FAnsiSql] (0.10.7) for task cancellation
- Data load engine no longer lists dropping columns / anonymising in progress if there are no operations actually being performed (e.g. no ANOTables configured)
- Delete is now disabled for the top level container (e.g. "UNION - Inclusion criteria") of cohort builder configuration

### Fixed

- Database patching user interface no longer suggests restarting if the patching process has failed
- Improved usability of StartupUI when no repository connection strings are not set (previously would report status as 'Broken')
- Fixed bug where `DropTableIfLoadFails` of `ExecuteFullExtractionToDatabaseMSSql` would (under fail conditions) drop the destination table even if the table was created by a previous execution of the same pipeline.
- Fixed bug where adding a Catalogue to a cohort set container would create an extra duplicate copy (which would appear under orphans)
- Improved cross server cohort query building (e.g. combining cohort sets on seperate servers / server types)
- Fixed bug in checks dual reporting some errors when clicking on red angry face icons

### Removed

- Generate test data window no longer shows the output folder in Windows Explorer when done

## [3.2.0] - 2019-09-16

### Added

- Patient Index Tables now use the source column datatype for caching columns (as long as there is no transform declared).

## [3.2.0-rc1] - 2019-09-13

### Added

- Right clicking a mispelled word now offers spelling suggestions
- You can now add new datasets to an extraction configuration directly from the "Core" folder in Execute Extraction window (rather than having to go back to the DataExport tree view)
- MDFAttacher now checks for existing mdf/ldf files in the RAW server data directory.  Existing files will trigger a warning.  After the warning an attempt is still made to overwrite the file(s) (as occured previously)
- Tab key now also works for autocomplete in SQL editor windows (previously only Enter worked)
- Orphan cohort sets (do not belong to any Cohort Identification Configuration) now appear under a top level folder in 'Cohort Builder' collection
- Extraction Category can now be changed directly from a CatalogueItem, ExtractionInformation 
- Extraction Category can be changed for all columns in a Catalogue at once by right clicking the or the CatalogueItemsNode (folder under a Catalogue)
- Right clicking a column allows you to Alter it's type e.g. increase the size of a varchar field

### Changed

- Help documentation for objects no longer uses NuDoq library (now faster and more maintainable)
- Extraction source component `ExecuteCrossServerDatasetExtractionSource` now never drops the temporary cohort database (previously it would drop it if it created it and CreateTemporaryDatabaseIfNotExists was true)
- Updated to latest version of [FAnsiSql] (0.10.4) for better Oracle, localization and type estimation
- Dashboards now appear in tree view instead of application tool strip and are searchable
- CatalogueItem descriptions pie chart has flags for including internal/project specific etc in it's counts
- CatalogueItem descriptions pie chart now lets you navigate directly to problem objects rather than showing a data table

### Fixed 
- Deleting an object now clears the selection in tree views (previously selection would become an arbitrary object).
- Fixed bug where adding/moving cohort sets between containers ([INTERSECT]/[UNION]/[EXCEPT]) could result in 2 objects with the same Order in the same container (resulting in ambiguous order of execution).
- Fixed UI bug where selecting an extractable Catalogue would hide it's extractable (small green e) icon overlay
- Fixed bug where deleting a Pinned object would not unpin the object
- Fixed bug where database tables with brackets in the name could break synchronization (these tables are now ignored by RDMP and cannot be imported).
- Fixed bug deleting multiple objects at once when some objects are parents of others (and cause implicit delete).
- Fixed bug with low resolution monitors and the Create New Cohort Wizard
- Fixed bug with low resolution monitors and collections where leading columns could shrink to be no longer visible
- Adding new filters/containers (AND/OR) now correctly expand and highlight the created object in collections
- Fixed AggregateEditorUI could incorrectly offer to save changes even when no changes had been made
- Clonng a Cohort Identification Configuration now preserves custom set container names e.g. "UNION Inclusion Criteria"
- Fixed bug in DataTableUploadDestination where multiple root (DataLoadInfo) logging entries were created for a single large bulk insert 
- Fixed bug in QueryBuilder when there are multiple IsPrimaryExtractionTable tables (Exception thrown was NullReferenceException instead of QueryBuilderException)
- Fixed bug in generating FROM SQL when there are circular JoinInfo configured between tables used in the query
- Fixed bug where closing the server/database selection dialog with the X instead of cancel could cause error messages (e.g. in Bulk Import TableInfos)
- Fixed bug where searching for "Pipeline" or "Pipe" did not show all pipelines
- Fixed bug caching patient index tables (cohort creation) when there are multiple tables being joined in the query.
- Fixed error when logging very large (over 4000 characters) to the RDMP logging database

### Removed
- Cohort sets no longer appear under Catalogues (Find / GoTo now open the parent cohort identification configuration)
- Removed OnlyUseOldDateTimes option on DataTableUploadDestination as it didn't actually do anything ([DBMS] type decisions are handled in a standard way by FAnsiSql)

## [3.1.0] - 2019-07-31

### Added

- Cohort sets with HAVING sql now support 'View Dataset Sample' (of matched records)
- Added new property IsView to TableInfo
- Added GoTo menu item Catalogue=>TableInfo
- Added user setting for skipping Cohort Creation wizard
- MDFAttacher emits more messages when looking up location on disk to copy MDF file to.
- Added menu option to set IsExtractionIdentifier on a Catalogue without having to open ExtractionInformations directly
- Added the ability to set custom number of patients / rows per dataset when creating example datasets (from command line or when setting up client)
- FlatFileAttacher now issues a warning if TableToLoad isn't one of the tables loaded by the currently executing load (previously it would just say 'table x wasn't found in RAW')
- Added (initially hidden) column Order to cohort query builder to help debugging any issues with order of display

### Changed

- Attempting to generate a graph from a query that returns more than 1,000,000 cells now asks for confirmation.
- Updated to latest version of [FAnsiSql] (0.9.4) for better Oracle support
- Oracle extraction commands no longer generate parameters (e.g. @projectNumber).  Previously invalid SQL was generated.
- Improved layout of message boxes and link highlighting
- Add (Copy Of) cohort set no longer complains about creating a copy of one already in the cohort builder configuration
- Extraction destination property CleanExtractionFolderBeforeExtraction now defaults to false (i.e. do not delete the contents of the extraction directory before extracting)
- Extraction destination property CleanExtractionFolderBeforeExtraction is now implemented in the Checks phase of the component lifecycle rather than on reciept of first batch of records (this prevents accidentally deleting files produced by upstream components)
- 
### Fixed 
- Fixed bug in Catalogue validation setup window (DQE Validation Rules) which resulted in changes not being saved if it had been refreshed after initially loading
- Fixed scrollbars not appearing in Catalogue validation setup window when lots of validation rules are applied to a single column
- Type text dialog prompt now resizes correctly and has a display limit of 20,000 characters for messages
- Fixed bug that prevented exiting if the RDMP directory (in user's application data folder) was deleted while the program was running
- Fixed bug where CatalogueItems created when importing Oracle tables had database qualifiers in the name e.g. "CHI" (including the double quotes)
- Fixed bug where deleting a Filter from a cohort set in a Cohort Identification Query could result in the display order changing to alphabetical (until tab was refreshed).
- Fixed obscure bug in plugins implementing the `ICustomUI` interface when returning a new object in `GetFinalStateOfUnderlyingObject` that resulted in the UI showing a stale version of the object
- Connecting to a non existant server in ServerDatabaseTableSelector now shows the Exception in the RAG icon (previously just showed empty database list)
 
- Fixed bug where adding/removing a column in Aggregate Editor would would reset the Name/Description if there were unsaved changes (to Name/Description)
- Fixed bug where example datasets created would have the text value "NULL" instead of db nulls (only affected initial install/setup datasets)

## [3.0.16-rc2] - 2019-07-17

### Added 

- Example data generated on install can now be given a seed (allows for reproducibility)
- Creating a Query Caching server for an cohort identification AggregateConfiguration now asks you if you want to set it as the default QueryCaching server (if there isn't already one)
- Double clicking a row in SQL query editor user interfaces now shows text summary of the row
- DLE load logs tree view now supports double clicking on messages/errors to see summary
- All RDMP platform objects now have icons even if not visible in the UI (this affects the objects documentation file generation)
- MetadataReport now supports generating data for Catalogues with no extractable columns

### Changed

- Updated to latest version of BadMedicine (0.1.5)
- Improved error message shown when attempting to delete a used patient index table (now lists the users)
- System no longer auto selects objects when there is only 1 option (e.g. when user starts a Release when there is only one Project in the system).  This previously created an inconsistent user experience.
- Dita extraction checks no longer propose deleting non dita files in the output directory
- Improved Find (Ctrl+F) dialog layout and added shortcut codes (e.g. typing "c Bob" will return all Catalogues containing the word "Bob")
- Message boxes now display a limit of 20,000 characters (full text can still be accessed by the copy to clipboard button).
- DLE Debug options (e.g. Skip migrating RAW=>STAGING) now appear as a drop down with more descriptive titles (e.g. StopAfterRAW)
 
### Fixed 

- Fixed bug when cloning a Pipeline called "Bob" when there was already an existing Pipeline called "Bob (Clone)"
- Fixed validation issue in some user interfaces of INamed classes (e.g. Catalogue) where all properties were checked for illegal characters instead of just the Name
- Fixed image scaling in Metadata reports to 100% (previously 133%)
- Governance report now properly escapes newlines and quotes in Catalogue descriptions when outputting as CSV
- Fixed bug in Plugin code generator for tables with a Name property (previously incorrect C# code was generated)
- Fixed bug in SQL query editor user interface when the query returned a table that included binary columns with large amounts of data in
- Clicking a collection button or using GoTo/Show now correctly pops the relevant collection if it is set to auto dock (pinned).
- Application title bar now correctly updates after loading a tab (previously it was left with the caption "Loading...")
- Un Pinning in a collection using X now correctly maintains tree selection (consistent with the context menu Tree=>UnPin)
- Fixed display order of cohort sets in Cohort Query Builder to correctly match the compiler (previously the tree view order was misleading)

## [3.0.16-rc] - 2019-07-08

### Added 

- Forward/backward navigation in LogViewer now preserves text filters / TOP X
- Added the ability to create example datasets and configurations/projects etc during installation / startup
- Objects with names containing problematic characters (e.g. \ ") are highlighted red
- New right click context menu GoTo shows related objects e.g. which ExtractionConfiguration(s) a Catalogue has been used in
- Heatmap hover tool tip now shows more information about the cell value
- 'Other Pipelines' (unknown use case) can now be edited by double clicking.  This prompts user to pick a use case to edit them under
- Creating a Catalogue/TableInfo by importing a file now lets you rename the table after it has been created
- Added new DLE module ExecuteSqlFileRuntimeTask which runs the SQL stored in the RDMP platform database (rather than relying on an sql file on disk like ExecuteSqlFileRuntimeTask)
- RDMP platform database schemas no longer require 100% matching to models.  This allows limited backwards compatibility between minor versions of RDMP in which new fields are added to the database.

### Changed

- Updated to latest version of [BadMedicine] (0.0.1.2)
- Updated to latest version of [FAnsiSql] (0.9.2)
- File=>New now launches modal dialog instead of dropdown menu
- Project objects can now be sorted (previously they always appeared alphabetically)
- Project creation UI now shows duplicate ProjectNumbers as a Warning instead of an Error allowing users to create 2+ Projects with shared cohorts
- Disabled objects in tree views now appear greyed out instead of red
- Improved message shown when cohorts with null descriptions are preventing cohort importing
- Attempting to deleting an Extractable Catalogue no longer shows an error and instead asks if you want to make it non extractable (then delete)
- xmldoc are now shipped inside SourceCodeForSelfAwareness.zip (instead of side by side with the binary).  This avoids an issue where [Squirrel drops xmldoc files](https://github.com/Squirrel/Squirrel.Windows/issues/1323)

### Fixed 

- Fixed bug in CLI (rdmp.exe) where yaml settings would override command line values for connection strings to platform databases
- Disabled smiley controls now render in greyscale
- Fixed bug in Aggregate graphs which included a PIVOT on columns containing values with leading whitespace
- Fixed crash bug in UI responsible for picking the DLE load folder that could occur when when xmldocs are missing
- Fixed bug resolving Plugin dll dependencies where dependencies would only be resolved correctly the first time they were loaded into the AppDomain
- Fixed Culture (e.g. en-us) not being passed correctly in DelimitedFlatFileAttacher
- Fixed bug where Updater would show older versions of RDMP as installable 'updates'

[Unreleased]: https://github.com/HicServices/RDMP/compare/v3.2.1...develop
[3.2.1]: https://github.com/HicServices/RDMP/compare/v3.2.1-rc4...v3.2.1
[3.2.1-rc4]: https://github.com/HicServices/RDMP/compare/v3.2.1-rc3...v3.2.1-rc4
[3.2.1-rc3]: https://github.com/HicServices/RDMP/compare/v3.2.1-rc2...v3.2.1-rc3
[3.2.1-rc2]: https://github.com/HicServices/RDMP/compare/3.2.1-rc1...v3.2.1-rc2
[3.2.1-rc1]: https://github.com/HicServices/RDMP/compare/3.2.0...3.2.1-rc1
[3.2.0]: https://github.com/HicServices/RDMP/compare/v3.2.0-rc1...3.2.0
[3.2.0-rc1]: https://github.com/HicServices/RDMP/compare/3.1.0...v3.2.0-rc1
[3.1.0]: https://github.com/HicServices/RDMP/compare/v3.0.16-rc2...3.1.0
[3.0.16-rc2]: https://github.com/HicServices/RDMP/compare/v3.0.16-rc...v3.0.16-rc2
[3.0.16-rc]: https://github.com/HicServices/RDMP/compare/v3.0.15...v3.0.16-rc
[FAnsiSql]: https://github.com/HicServices/FAnsiSql/
[BadMedicine]: https://github.com/HicServices/BadMedicine/

[DBMS]: ./Documentation/CodeTutorials/Glossary.md#DBMS
[UNION]: ./Documentation/CodeTutorials/Glossary.md#UNION
[INTERSECT]: ./Documentation/CodeTutorials/Glossary.md#INTERSECT
[EXCEPT]: ./Documentation/CodeTutorials/Glossary.md#EXCEPT