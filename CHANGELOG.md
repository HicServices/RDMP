# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

...

### Fixed

- reduced memory overhead during refreshes

### Added

- Pipeline ID and Name now recorded in logs for Data Extractions

## [4.1.9] - 2020-09-17

### Added

- Added ExplicitDateTimeFormat property to flat file attachers and pipeline sources.  Allows custom parsing of dates e.g. where no delimiters exist (e.g. 010120)

## [4.1.8] - 2020-08-17

### Fixed 

- Fixed progress logging still not being allowed to go backwards when logging to database

## [4.1.7] - 2020-08-14

### Changed

- Schema names (Sql Server) are now wrapped correctly e.g. `[My Cool Schema]`
- Progress logged (e.g. done x of y files) can now go backwards.

### Added

- New command `SetArgument` for easier changing of values of modules (e.g. [PipelineComponent]) from command line
- Support for `DescribeCommand` help text on `NewObject` and other commands that take dynamic argument lists (command line)

## [4.1.6] - 2020-08-04

### Added

- Added 'Save Changes' prompt when closing tabs
- Added Import command for bringing in one or more [CohortIdentificationConfiguration] into an existing container (like Merge / UnMerge but for existing configurations)
- Added checks for LoadProgress dates being in sensible ranges during DLE

### Fixed

- Fixed [bug when parsing lists of ints in CLI](https://github.com/HicServices/RDMP/issues/84)

## [4.1.5] - 2020-07-14

### Added

- Added Merge command, for combining two or more configurations in cohort builder into one
- Added Un Merge command for splitting one cohort builder configuration into multiple seperate ones
- Improved error messages in extraction checking when there are:
  -  2+ columns with the same name
  -  2+ columns with the same location in extraction order
  -  Cohort and dataset are on different servers
- Added ability to search by ID in find dialog

### Changed

- Unhandled Application/Thread exceptions (rare) now show in the top right task bar instead of as a popup dialog

### Fixed

- Fixed lookups, supporting documents etc not appearing in the extractable artifacts tree view of the extraction window when non global.

## [4.1.4] - 2020-07-02

### Added

- Custom Metadata Report now supports looping items in a Catalogue (use `$foreach CatalogueItem` to start and `$end` to end)
- Added help to 'New Project' user interface
- Forward/Backward now includes selection changes in tree collections
- Added support for newline replacement in custom metadata doc templates

### Changed

- Improved usability of selecting multiple datasets in the 'New Project' user interface
- When in multiple selection mode, double clicking a row in the object selection dialog will add it to the selection (previously would close the dialog with the double clicked item as the sole selected item)

### Fixed

- Extractable columns Order field defaults to Max + 1 (previously 1).  This results in new columns appearing last in extracted datasets and prevents Order collisions.
- 'Select Core' columns UI button now works correctly with ProjectSpecific Catalogues (previously the highlighted rows would not change)
- Fixed popup error message showing when deleting an ExtractionConfiguration where one or more datasets are currently being edited (in tabs) 
- Fixed context menu opening error that could occur in cohort builder when datasets are not configured properly (e.g. have too many [IsExtractionIdentifier] columns).
- Fixed alias changes not showing up as 'Differences' in edit dataeset extraction user interface
- Fixed bugs in using GoTo menu of document tabs after a Refresh
- Fixed ALTER context sub menu of TableInfo when Server property is null (or other fundamental connection details cannot be resolved).
- Fixed whitespace only literal strings (e.g. `" "`) on command line causing error while parsing arguments
- Fixed bug with YesNoToAll popups launched from ChecksUI when running as a modal dialogue.
- Fixed bug with user setting 'Show Object Collection On Tab Change' when selecting tabs for objects in CohortBuilder configurations.

## [4.1.3] - 2020-06-15

### Added

- Added `-f` option to CLI (`rdmp.exe -f somefile.yaml`) to run all commands in a file
- Added "Go To" to tab right click context menu (previously only available in collections).
- Private key encryption file location can now be customized per user by setting an environment variable `RDMP_KEY_LOCATION`.  This will override any key file location specified in the RDMP platform database.

### Changed

- Frozen Extraction Configurations folder always appears at the bottom of the branch under Projects
- Improved layout of query building errors in QueryBuilder SQL viewing user interfaces

### Fixed

- Fixed bug in tree ordering when comparing a fixed order node to a non fixed order node.

## [4.1.2] - 2020-06-03

### Added

- Ability to create (Project Specific) Catalogues using the Project collection tree view top menu
- Ability to Enable/Disable many objects at once
- Catalogue icons under a load now show full range of status icons (e.g. internal / project specific)

### Changed

- When a load has only one LoadProgress dropdown no longer shows "All available"
- Double clicking a crashed configuration in cohort builder now shows the error message (previously would edit/expand the object).  Error message still accessible via context menu (as previously).
 
### Fixed

- Fixed Order not being considered 'OutOfSync' on ExtractableColumn
- Fixed changes to Catalogue visibility checkboxes not being persisted
- Fixed object caching system when RDMP user has insufficient permissions to view Change Tracking tables. 
- Fixed UserSettings last column sort order multithreading issue (causing File IO permissions error in rare cases)

## [4.1.1] - 2020-05-11


### Added

- Added ability to pick a folder in Metadata Report UI

### Fixed

- Opening 'Recent' items that have been deleted now prompts to remove from list
- Fixed race conditions updating UI during refresh / dispose of activators

## [4.1.0] - 2020-05-05

### Added

- Added tool strip to tree collection user interfaces
- Added new [PipelineComponent] `SetNull` which detects bad data in a specific column of pipeline data and sets cells matching the `Regex` to null
- Added support for template based metadata extractions ([Catalogue] descriptions etc) 
- Added new property RemoteServerReference to RemoteTableAttacher which centralises server name/database/credentials when creating many attachers that all pull data from the same place
- Added double click to expand tree option for RDMP
- When searching (Ctrl+F), exact matches now appear first
- Added RDMP platform database name (and server) to the window title
- Added Export Plugins command (which saves the currently loaded RDMP plugins to the selected folder)
- Double clicking a dataset in the Extraction user interface opens it for editing (previously you had to right click and select Edit)

### Changed

- CohortBuilder interface has been revamped
- Home screen now follows more consistent user experience and includes recently used items
- Catalogue collection no longer expands when CatalogueFolder changes

### Fixed

- LoadProgress with RemoteTableAttacher now works correctly with DBMS that do not support Sql parameter declarations (Oracle / Postgres)

## [4.0.3] - 2020-02-28

### Added

- Added timestamps to Word Metadata Reports (e.g. when document was created)
- Added icon for HashOnDataRelease
- Added Order column to [Catalogue] Collection tree view
- Added ability to disable the TicketingSystem that controls whether datasets can be released (only applies where one has been configured)
- Added ability to customize extraction directory subfolder names
- Added check for stale extraction records when generating a one off Release Document (i.e. not part of a Release workflow)
- Added clarifiaction on what to do if a table is not found during synchronization
- Refresh now shows 'waiting' cursor while updates take effect
- Creating a [Catalogue] from a CatalogueFolder right click context menu now creates the resulting [Catalogue] in that directory
- Added ability to right click a dataset in an [ExtractionConfiguration] and open the directory into which it was extracted (if it was extracted to disk)
- Added Extraction Category column for columns included in the project extractions
- Added command Import [Catalogue] Item Descriptions accessible from the [CatalogueItem] node menu that imports all descriptions (and other fields) from one [Catalogue] into another.
- Added 'Execute' button on [Catalogue] and Extraction dataset SQL viewing windows.
- 'Show' on collection based tab windows now prompts you to pick which you want to navigate to (previously did nothing)
- Datagrid UI now shows server/database names and DatabaseType
- Running Checks or CheckAll now shows the Checks column (if it isn't already visible)
- Added 'Clear Cache' option for clearing the cache on a single [Catalogue] in a cohort builder configuration (without affecting the cache state of the others)
- Added `FOR UPDATE` to the end of the DLE migration query for MySql server (prevents edge case deadlocks when live table changes during migration)

### Changed

- Datagrid/query syntax errors are now more visible and consistent with other SQL IDEs
- Open / New [Catalogue] no longer closes all toolboxes prior to setting up editing layout
- Bulk Process CatalogueItems now defaults to exact matching (ignoring case)
- Changed MySql adapter from `MySql.Data` to `MySqlConnector` (see [FAnsiSql] version 0.11.1 change notes)

### Fixed

- Fixed bug where broken Lookup configurations could result in DQE not passing checks
- Fixed top menu missing some options on extraction/cohort building graphs (e.g. timeout / retry query)
- Fixed DLE backup trigger creation for old versions of MySql (5.5 and earlier)
- Fixed some forms not getting launched when new objects are created (e.g. Supporting Documents)
- Fixed null reference when cancelling adding a SupportingDocument
- Fixed bug in axis section of graph editor where changing value would result in text box loosing focus
- Fixed ticketing system Reason [for not being able to release a configuration] not being displayed on the ReleaseUI

## [4.0.2] - 2020-01-23

### Fixed

- Fixed stack overflow when trying to edit 'unknown pipelines' in Tables tree view
- Undo/Redo button now changes label as well as icon during use
- Fixed null reference when using command `Reports->Generate...->Metadata Report...`
- Fixed bug in console gui where cancelling a property change (e.g. Description) would result in setting the value to null.

## [4.0.1] - 2019-12-03

### Added

- Ability to generate metadata reports for subset of catalogues (e.g. all catalogues in a folder).
- Cohort Builder build log now lists the [IsExtractionIdentifier] column for each cohort set

### Changed

- Cohort Builder now shows "No Cache" when there is no query cache server configured for a configuration instead of "0/1" (or "0/2" etc)

### Fixed

- Fixed issue using the 'context menu' button on compatible keyboards to access the GoTo menu (sometimes menu would not be expandable)
- Fixed issue where ProjectNumber and Version appeared editable in some tree controls (changes were ignored).  These cells are now correctly readonly.
- Fixed bug in log viewer right click (introduced in 4.0.1 command refactoring)
- TestConnection now shows obfuscated connection string when a connection cannot be established (affects RDMP API users only - not core software)
- Fixed changing join direciton in patient index tables not triggering refresh
- Fixed Data Load Engine RAW server credentials when running RDMP installer with sql user authentication (RAW server entry would be created with Integrated Security)

## [4.0.1-rc3] - 2019-11-25

### Added

- Console gui supports short code searches (e.g. "c", "ti" etc)

### Changed

- Updated to [FAnsiSql] 0.10.13

### Fixed

- Fixed various issues with new CLI gui

## [4.0.1-rc2] - 2019-11-20

### Added

- Added interactive terminal user interface `./rdmp gui`

### Changed

- Cloning an Extraction Configuration no longer expands clone and names the new copy "Clone of [..]" (previously name was a guid)
- Select object dialog now display a maximum of 1000 objects (prioritising your search text)
- Logging tasks are now case insensitive

### Fixed

- Fixed Console input in CLI when running under Linux
- Fixed issue where parallel checks could fail due to UI cross thread access
- Fixed bugs in DLE when loading tables with dodgy column names (e.g. `[My Group by lolz]`)
- 
...

## [4.0.1-rc1] - 2019-11-11

### Added

- Support for PostgreSql databases

### Changed

- Sql Server `..` syntax is no longer used (now uses `.dbo.` - or whatever the table schema is).  Since references can be shared by users the default schema notation is not good idea.
- Cohort Query Bulder will now connect to the database containing the data rather than the users default database when querying data on a single database
- Flat file Attachers now process files in alphabetical order (case insensitive) when Pattern matches multiple files (previously order was arbitrary / OS defined)
- Extraction source now specifies database to connect to when a dataset exists in a single database (previously connected to users default server e.g. master)
- Updated to latest version of [FAnsiSql] (0.10.12) for Postgres support
- 
### Fixed

- Fixed handling of credentials where password is blank (allowed)
- Fixed race condition when there are multiple cohort databases that host cohorts for the same project
- Extracting a dataset using Cross Server extraction source now shows the correct SQL in error message when no records are returned by the linkage

## [3.2.1] - 2019-10-30

### Added

- SET containers ([UNION] / [INTERSECT] / [EXCEPT]) now highlight (as a `Problem`) when they will be ignored (empty) or not applied (when they contain only 1 child)

## Fixed

- Fixed bug generating metadata reports that include Catalogues with orphan [ExtractionInformation] (not mapped to an underlying ColumnInfo)
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
- Fixed bug where adding a [Catalogue] to a cohort set container would create an extra duplicate copy (which would appear under orphans)
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
- Extraction Category can now be changed directly from a CatalogueItem, [ExtractionInformation] 
- Extraction Category can be changed for all columns in a [Catalogue] at once by right clicking the or the CatalogueItemsNode (folder under a Catalogue)
- Right clicking a column allows you to Alter it's type e.g. increase the size of a varchar field

### Changed

- Help documentation for objects no longer uses NuDoq library (now faster and more maintainable)
- Extraction source component `ExecuteCrossServerDatasetExtractionSource` now never drops the temporary cohort database (previously it would drop it if it created it and CreateTemporaryDatabaseIfNotExists was true)
- Updated to latest version of [FAnsiSql] (0.10.4) for better Oracle, localization and type estimation
- Dashboards now appear in tree view instead of application tool strip and are searchable
- [CatalogueItem] descriptions pie chart has flags for including internal/project specific etc in it's counts
- [CatalogueItem] descriptions pie chart now lets you navigate directly to problem objects rather than showing a data table

### Fixed 
- Deleting an object now clears the selection in tree views (previously selection would become an arbitrary object).
- Fixed bug where adding/moving cohort sets between containers ([INTERSECT]/[UNION]/[EXCEPT]) could result in 2 objects with the same Order in the same container (resulting in ambiguous order of execution).
- Fixed UI bug where selecting an extractable [Catalogue] would hide it's extractable (small green e) icon overlay
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
- Fixed bug in generating FROM SQL when there are circular [JoinInfo] configured between tables used in the query
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
- Added menu option to set [IsExtractionIdentifier] on a [Catalogue] without having to open ExtractionInformations directly
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
- Fixed bug in [Catalogue] validation setup window (DQE Validation Rules) which resulted in changes not being saved if it had been refreshed after initially loading
- Fixed scrollbars not appearing in [Catalogue] validation setup window when lots of validation rules are applied to a single column
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
- System no longer auto selects objects when there is only 1 option (e.g. when user starts a Release when there is only one [Project] in the system).  This previously created an inconsistent user experience.
- Dita extraction checks no longer propose deleting non dita files in the output directory
- Improved Find (Ctrl+F) dialog layout and added shortcut codes (e.g. typing "c Bob" will return all Catalogues containing the word "Bob")
- Message boxes now display a limit of 20,000 characters (full text can still be accessed by the copy to clipboard button).
- DLE Debug options (e.g. Skip migrating RAW=>STAGING) now appear as a drop down with more descriptive titles (e.g. StopAfterRAW)
 
### Fixed 

- Fixed bug when cloning a Pipeline called "Bob" when there was already an existing Pipeline called "Bob (Clone)"
- Fixed validation issue in some user interfaces of INamed classes (e.g. Catalogue) where all properties were checked for illegal characters instead of just the Name
- Fixed image scaling in Metadata reports to 100% (previously 133%)
- Governance report now properly escapes newlines and quotes in [Catalogue] descriptions when outputting as CSV
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
- New right click context menu GoTo shows related objects e.g. which ExtractionConfiguration(s) a [Catalogue] has been used in
- Heatmap hover tool tip now shows more information about the cell value
- 'Other Pipelines' (unknown use case) can now be edited by double clicking.  This prompts user to pick a use case to edit them under
- Creating a Catalogue/TableInfo by importing a file now lets you rename the table after it has been created
- Added new DLE module ExecuteSqlFileRuntimeTask which runs the SQL stored in the RDMP platform database (rather than relying on an sql file on disk like ExecuteSqlFileRuntimeTask)
- RDMP platform database schemas no longer require 100% matching to models.  This allows limited backwards compatibility between minor versions of RDMP in which new fields are added to the database.

### Changed

- Updated to latest version of [BadMedicine] (0.0.1.2)
- Updated to latest version of [FAnsiSql] (0.9.2)
- File=>New now launches modal dialog instead of dropdown menu
- [Project] objects can now be sorted (previously they always appeared alphabetically)
- [Project] creation UI now shows duplicate ProjectNumbers as a Warning instead of an Error allowing users to create 2+ Projects with shared cohorts
- Disabled objects in tree views now appear greyed out instead of red
- Improved message shown when cohorts with null descriptions are preventing cohort importing
- Attempting to deleting an Extractable [Catalogue] no longer shows an error and instead asks if you want to make it non extractable (then delete)
- xmldoc are now shipped inside SourceCodeForSelfAwareness.zip (instead of side by side with the binary).  This avoids an issue where [Squirrel drops xmldoc files](https://github.com/Squirrel/Squirrel.Windows/issues/1323)

### Fixed 

- Fixed bug in CLI (rdmp.exe) where yaml settings would override command line values for connection strings to platform databases
- Disabled smiley controls now render in greyscale
- Fixed bug in Aggregate graphs which included a PIVOT on columns containing values with leading whitespace
- Fixed crash bug in UI responsible for picking the DLE load folder that could occur when when xmldocs are missing
- Fixed bug resolving Plugin dll dependencies where dependencies would only be resolved correctly the first time they were loaded into the AppDomain
- Fixed Culture (e.g. en-us) not being passed correctly in DelimitedFlatFileAttacher
- Fixed bug where Updater would show older versions of RDMP as installable 'updates'

[Unreleased]: https://github.com/HicServices/RDMP/compare/v4.1.9...develop
[4.1.9]: https://github.com/HicServices/RDMP/compare/v4.1.8...v4.1.9
[4.1.8]: https://github.com/HicServices/RDMP/compare/v4.1.7...v4.1.8
[4.1.7]: https://github.com/HicServices/RDMP/compare/v4.1.6...v4.1.7
[4.1.6]: https://github.com/HicServices/RDMP/compare/v4.1.5...v4.1.6
[4.1.5]: https://github.com/HicServices/RDMP/compare/v4.1.4...v4.1.5
[4.1.4]: https://github.com/HicServices/RDMP/compare/v4.1.3...v4.1.4
[4.1.3]: https://github.com/HicServices/RDMP/compare/v4.1.2...v4.1.3
[4.1.2]: https://github.com/HicServices/RDMP/compare/v4.1.1...v4.1.2
[4.1.1]: https://github.com/HicServices/RDMP/compare/v4.1.0...v4.1.1
[4.1.0]: https://github.com/HicServices/RDMP/compare/v4.0.3...v4.1.0
[4.0.3]: https://github.com/HicServices/RDMP/compare/v4.0.2...v4.0.3
[4.0.2]: https://github.com/HicServices/RDMP/compare/v4.0.1...v4.0.2
[4.0.1]: https://github.com/HicServices/RDMP/compare/v4.0.1-rc3...v4.0.1
[4.0.1-rc3]: https://github.com/HicServices/RDMP/compare/v4.0.1-rc2...v4.0.1-rc3
[4.0.1-rc2]: https://github.com/HicServices/RDMP/compare/v4.0.1-rc1...v4.0.1-rc2
[4.0.1-rc1]: https://github.com/HicServices/RDMP/compare/v3.2.1...v4.0.1-rc1
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
[IsExtractionIdentifier]: ./Documentation/CodeTutorials/Glossary.md#IsExtractionIdentifier

[Catalogue]: ./Documentation/CodeTutorials/Glossary.md#Catalogue
[SupportingDocument]: ./Documentation/CodeTutorials/Glossary.md#SupportingDocument
[TableInfo]: ./Documentation/CodeTutorials/Glossary.md#TableInfo

[ExtractionConfiguration]: ./Documentation/CodeTutorials/Glossary.md#ExtractionConfiguration
[Project]: ./Documentation/CodeTutorials/Glossary.md#Project

[CatalogueItem]: ./Documentation/CodeTutorials/Glossary.md#CatalogueItem
[ExtractionInformation]: ./Documentation/CodeTutorials/Glossary.md#ExtractionInformation
[ColumnInfo]: ./Documentation/CodeTutorials/Glossary.md#ColumnInfo

[JoinInfo]: ./Documentation/CodeTutorials/Glossary.md#JoinInfo

[PipelineComponent]: ./Documentation/CodeTutorials/Glossary.md#PipelineComponent
[Pipeline]: ./Documentation/CodeTutorials/Glossary.md#Pipeline

[Lookup]: ./Documentation/CodeTutorials/Glossary.md#Lookup
[CohortIdentificationConfiguration]: ./Documentation/CodeTutorials/Glossary.md#CohortIdentificationConfiguration
