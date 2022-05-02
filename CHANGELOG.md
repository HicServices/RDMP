# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

...

### Added

- Added new command 'RefreshBrokenCohorts' for clearing the 'forbid list' of unreachable cohort sources - [#1094](https://github.com/HicServices/RDMP/issues/1094)
- Added abilty to skip CIC validation checks when opening the commit cohort dialogue - [#1118](https://github.com/HicServices/RDMP/issues/1118)

### Changed

- Dll load warnings must now be enabled otherwise the information is reported as Success (see user settings error codes R008 and R009)
- The Choose Cohort command no longer lets you pick deprecated cohorts - [#/1109](https://github.com/HicServices/RDMP/issues/1109)

### Fixed

- Fixed resizing issue on License UI when using very low resolution
- Fixed connection strings dialog 'Save as yaml...' producing invalid entry for 'DataExportConnectionString' - [#1086](https://github.com/HicServices/RDMP/issues/1086)
- Fixed various startup errors when Databases.yaml strings are invalid.
- Fixed bug with the 'unreachable' picturebox icon not being clickable
- Fixed unreachable catalogue database resulting in the Startup form immediately closing
- Fixed being able to drag filters/containers onto API calls in Cohort Builder -[#1101](https://github.com/HicServices/RDMP/issues/1101)
- Fixed regression in 7.0.10 where calling `public void ClearDefault(PermissableDefaults toDelete)` multiple times caused an Exception
- Fixed `ExecuteCrossServerDatasetExtractionSource` to work properly with identifiable extractions - [#1097](https://github.com/HicServices/RDMP/issues/1097)

## [7.0.10] - 2022-04-25

### Added

- "parameter description" and "property name" have been added to the "set value" option for filters - https://github.com/HicServices/RDMP/issues/1034
- Filter parameter values are now prompted for the user when adding existing filter without known good value sets - https://github.com/HicServices/RDMP/issues/1030
- "Set Parameter Value(s)" option added to filter menus so you can more easily change the parameter values - https://github.com/HicServices/RDMP/issues/1035
- Added 'SelectiveRefresh' user setting
- Add options to create an extraction from a Cohorts right click menu and main userinterface - https://github.com/HicServices/RDMP/issues/1039
- Warnings are now shown if "non core" column are used for an extraction/release - https://github.com/HicServices/RDMP/issues/1024
- Added AlwaysJoinEverything user setting for always forcing joins in CohortBuilder - https://github.com/HicServices/RDMP/issues/1032
- Added UsefulProperty columns back into Find/Select dialog - https://github.com/HicServices/RDMP/issues/1033
- Added Extraction/Release warnings for extractions that contain Internal/Deprecated/SpecialApproval fields - https://github.com/HicServices/RDMP/issues/1024
- Added right click context menu support for console gui
- Cohorts now have right click option "Go To -> Project(s)"

### Fixed

- Fixed bug preventing example datasets being created from the RDMP UI client because checkbox was disabled
- "Exisiting" filter typo corrected - https://github.com/HicServices/RDMP/issues/1029
- Fixed refreshes sometimes changing selection in Data Export tree - https://github.com/HicServices/RDMP/issues/1008


### Changed

- New filters are now highlighted correctly when added to a CIC - https://github.com/HicServices/RDMP/issues/1031
- Creating a new Extracion Configuration will now ask the user for Name, Cohort and Datasets to be included for the extraction - https://github.com/HicServices/RDMP/issues/983
- AllowIdentifiableExtractions is now an ErrorCode so can be set to Success instead of always being Fail or Warning (i.e. to completley ignore it).
- The extractability of columns are no longer saved if a Dataset is removed from an Extraction Configuration - https://github.com/HicServices/RDMP/issues/1023
- "Show Pipeline Completed Popup" now enabled by default - https://github.com/HicServices/RDMP/issues/1069
- Cohorts are now "emphasise" after being commited. If part of one project it will highlight under that project.


## [7.0.9] - 2022-03-29

### Added

- Added command CreateNewCohortFromTable which creates a cohort from a table directly without having to first import it as a [Catalogue]
- Import Catalogue filter now allows selecting multiple filters at once.
- Improved performance of Select objects dialog when there are many objects available to pick from
- Made Select objects dialog filter in the same way as the Find dialog (i.e. support short codes and Type names)
- Ability to select multiple objects at once when adding to a Session
- Ability to find multiple objects at once (ctrl+shift+f)
- Added new pipeline component CohortSampler


### Fixed

- Fixed newlines in CatalogueItem descriptions not being output correctly in docx metadata report
- Fixed iterative data loads run on the CLI throwing and returning non zero when caught up to date with load progress (when running in iterative mode)
- Pipeline component order is now "correct" and will list more important variables at the top rather than at the bottom - https://github.com/HicServices/RDMP/issues/996
- Fixed bug where Pipeline objects could not be deleted from the `Tables (Advanced)` tree
- Removing a datset from an [ExtractionConfiguration] now deletes any extraction specific column changes (i.e. changes are not persisted if the dataset is added back in again)
- Fixed Release button prompting to pick [Project] when clicked in the ExecuteExtractionUI [#963](https://github.com/HicServices/RDMP/issues/963)

### Changed

- Processes wanting to run a Pipeline using the current user interface abstraction layer `IPipelineRunner GetPipelineRunner` must now provide a task description and UI look and feel as a `DialogArgs` argument.

## [7.0.8] - 2022-03-08

### Fixed

- Fixed Startup skipping some plugin dlls during load and enabled multithreading
- Fixed CLI not showing underlying exception when unable to reach platform databases

### Removed

- CSV files with unclosed leading quotes are no longer preserved when using IgnoreQuotes (side effect of updating CsvHelper)

## [7.0.7] - 2022-03-01

*Database Patches Included (enables ExtractionProgress retry)*

### Added
- Added ArchiveTriggerTimeout user setting [#623](https://github.com/HicServices/RDMP/issues/623)
- Support for referencing plugin objects from command line e.g. `./rdmp.exe cmd delete MyPluginClass:2`
- The word 'now' is a valid date when supplied on the command line
- Ability to sort based on Favourite status [#925](https://github.com/HicServices/RDMP/issues/925)
- Added Frozen column to Cohort Builder tree for easier sorting
- Added ability to query an [ExternalDatabaseServer] from the right click context menu [#910](https://github.com/HicServices/RDMP/issues/910)
- Added an overlay @ symbol for filters that have known parameter values configured [#914](https://github.com/HicServices/RDMP/issues/914)
- Added Retry support to [ExtractionProgress]
- Added new CLI options for RDMP installer `--createdatabasetimeout` and `--otherkeywords` for custom auth setups e.g. Azure/Active Directory Authentication etc.

### Fixed
- Fixed closing and changing instance not consulting tabs before closing
- Fixed bug where setting `SuggestedCategory` on a plugin command resulted in it vanishing from context menu
- Fixed bug with AllowEmptyExtractions not working under some situations
- Fixed [Lookup] creation UI creating CatalogueItem with the suffix _Desc even when you ask it not to in prompt
- Fixed layout bug in rule validation configuration UI where rationale tip was cut off [#909](https://github.com/HicServices/RDMP/issues/909)
- Fixed ViewLogs tab not remembering sort order between usages [#902](https://github.com/HicServices/RDMP/issues/902)

### Changed

- Find sorts ties firstly by favourite status (favourite items appear above others)
- Find sorts ties lastly alphabetically (previously by order of ID)
- Default sort order of ViewLogs on first time use is now date order descending [#902](https://github.com/HicServices/RDMP/issues/902)

## [7.0.6] - 2022-01-25

*Database Patch Included (enables ExtractionProgress batching)*

### Added

- Added [ExtractionProgress] for robustly extracting large datasets in multiple smaller executions
- Added ability to export [ExtractableCohort] to CSV file
- Added 'Created From' column to cohort detail page (parses cohorts AuditLog)

### Fixed

- Fixed a bug where ProjectUI would not show cohorts when some cohort sources are unreachable
- Fixed ProgressUI filter hiding global errors on extraction where the whole operation failed and a dataset filter was selected ([888](https://github.com/HicServices/RDMP/issues/888))
- Fixed a rare dll resolving issue that could occur during startup when running the RDMP windows client from outside the current directory (https://github.com/HicServices/RDMP/issues/877)

### Changed

- Changed right click context menu item 'Delete' to say 'Remove' when deleting a chain or relationship object (e.g. cohort usage by a project) ([#887](https://github.com/HicServices/RDMP/issues/887))
- Restricted [Pipelines] shown to only those where all components are compatible with the input objects (previously on context was checked) (https://github.com/HicServices/RDMP/issues/885)
- "Show All/Incompatible Pipelines" option added to Pipelines dropdown to make a simpler user interface
- When committing a cohort through the Cohort Builder the Project will automatically be selected if it already belongs to a single one (https://github.com/HicServices/RDMP/issues/868)
- Removed requirement for filter parameters to have comments to be published (https://github.com/HicServices/RDMP/issues/582)

## [7.0.5] - 2022-01-10

### Added

- Added ability to open extraction directory for an [ExtractionConfiguration]
- Added diagnostic screen logging last executed command (https://github.com/HicServices/RDMP/issues/815)
- Added tooltips for objects in tree views (https://github.com/HicServices/RDMP/issues/819).
- Added custom icon for [CatalogueItem] that represent transforms on the underlying column (https://github.com/HicServices/RDMP/issues/818)
- Added Extraction Primary Keys to Catalogue tooltip
- Added ability to 'View TOP 100' etc samples on [ExtractionInformation] (previously only available on [ColumnInfo] objects)
- Added icon overlays for 'Is Extraction Identifier' and 'Is Extraction Primary Key' (https://github.com/HicServices/RDMP/issues/830)
- Extraction Information for a Catalogue Item now includes "Transforms Data" property (which shows yes/no based on whether it transform the column data)
- Added 'open load directory' command to [Catalogue] context menu
- Added ability to switch between instances of RDMP using the Locations menu
- Added CLI command `ClearQueryCache`
- Added Description capability to prompts. More descriptions to be added (https://github.com/HicServices/RDMP/issues/814)
- Added description to Publish Filter "Select One" dialog (https://github.com/HicServices/RDMP/issues/813)
### Fixed
- Changed to SHIFT+Enter for closing multiline dialogs (https://github.com/HicServices/RDMP/issues/817)
- Fixed bug where configuring dataset didn't show all available tables when listing optional joinable tables (https://github.com/HicServices/RDMP/issues/804)

### Changed
- Updated CatalogueItemUI (https://github.com/HicServices/RDMP/issues/820)
- Fixed bug where cached aggregates were not considered stale even though changes had been made to their patient index table (https://github.com/HicServices/RDMP/issues/849)
- "You only have one object Yes/No" box has been removed in favour of being more consistent for the user (https://github.com/HicServices/RDMP/issues/811)

## [7.0.4] - 2021-12-08

### Added

- Added `RoundFloatsTo` to ExecuteDatasetExtractionFlatFileDestination
- Added new menu item Diagnostics->Restart Application
- Trying to extract an [ExtractionConfiguration] with a cohort that is marked IsDeprecated now fails checks
- Added [MigrateUsages] setting to cohort creation destination pipeline components.  When enabled and creating a new version of an existing cohort then all unreleased [ExtractionConfiguration] using the old (replaced) cohort switch to the new version
- Added an 'All Tasks', 'All Runs' etc commands to View Logs tab menu
- Added ability to filter [Catalogue] in the Find dialog by Internal/Deprecated etc
- Added search and filter compatible controls to [Pipeline] editing dialog
- Added ability to ignore/elevate specific errors in UserSettings
- Enabled Expand/Collapse all when right clicking whitespace in a tree collection
- Added title to graph charts
- Added a user setting for hiding Series in which all cells are 0/null
- Added `IPipelineOptionalRequirement` interface for Plugin Pipeline Components that can optionally make use of Pipeline initialization objects but do not require them to function.
- Support for templating in `ColumnSwapper` when used in an extraction pipeline (e.g. $n for project number)
- Support for specifying `--ConnectionStringsFile somefile.yaml` when starting RDMP (gui client or CLI)
- Added 'Hash On Release' column to initial new Catalogue extractability configuration dialog (https://github.com/HicServices/RDMP/issues/394)

### Fixed

- Fixed [Pipeline] objects showing an ID of 0 in tree collections
- Fixed the 'filters' count column in [Catalogue] tree collection showing edit control when clicked
- Fixed Find not working when searching by ID for [Pipeline] objects
- Prevented showing out dated cohorts when changing Project half way through defining a cohort
- When plugins contain dlls with differing version numbers then the latest dll version is loaded (previously the first encountered was used)
- Fixed bug in Console Gui where edit window showed value set directly instead of passing through Property Setters
- Fixed bug in Console Gui where password properties showed (encrypted) HEX binary value instead of ****
- Fixed Command Line UI showing abstract and interfaces when prompting user to pick a Type
- Fixed `OverrideCommandName` not working for `ExecuteCommandViewLogs` command
- Fixed `View Logs` commands appearing twice in right click context menu for logging servers objects (once on root and once under 'View Logs' submenu)
- Generate Release Document now shows as impossible when Cohort is not defined or unreachable (e.g. if user does not have access to cohort database)
- Fixed bug where selecting a [PipelineComponent] for which help is unavailable would leave the previously selected component's help visible
- Fixed bug with 'Commit Cohort' storing the target cohort database for future clicks
- Fixed a bug where editing a field like `Description` would fire validation on other properties e.g. `Name` which could slow controls down when validation is slow and change events are fired in rapid succession.
- Edit Catalogue window layout updated to allow errors to be seen on the right hand side of inputs (https://github.com/HicServices/RDMP/issues/758)
- Cohort Identification Configuration descriptions box is now easy to read and edit (https://github.com/HicServices/RDMP/issues/755)
- Fixed bug where RDMP would lose focus when "checks" were being run in background resulting in RDMP appearing unresponsive (https://github.com/HicServices/RDMP/issues/747)
- Fixed bug where some words in RDMP would have spaces in the wrong place (e.g. "W HERE") (https://github.com/HicServices/RDMP/issues/752)

### Changed

- Bump System.Drawing.Common from 5.0.2 to 5.0.3
- Bump System.Security.Permissions from 5.0.0 to 6.0.0
- Bump NLog from 4.7.12 to 4.7.13
- Changed to Dock layout for Pipeline editing control (may improve performance on older machines)
- Removed dependency on `System.Drawing.Common` by updating usages to `System.Drawing`
- Increased size of all text fields in [Catalogue] and [CatalogueItem] to `nvarchar(max)` to support long urls etc
- Updated icons to a more modern look. Catalogue Item image no longer has black corner. Green yellow and red smiley faces have been replaced. Cloud API icon replaced (https://github.com/HicServices/RDMP/issues/712)
- Extract to database now checks for explicit table names amongst pre-existing tables on the destination
- Startup no longer reports non dotnet dlls as 'unable to load' (warnings)
- Added Project number to Title Bar (and full project name to tooltip) for Extraction Configurations (https://github.com/HicServices/RDMP/issues/621)
- Root Cohort Identification Configuration will now highlight SET container issues with red highlight (https://github.com/HicServices/RDMP/issues/681)
- "Data Export" has been renamed to "Projects" to be more consistent (https://github.com/HicServices/RDMP/issues/720)
- Corrected layout of "Master Ticket" in New Project dialog (https://github.com/HicServices/RDMP/issues/735)
- Corrected layout of "Create New Lookup" (https://github.com/HicServices/RDMP/issues/730)
- Aligned buttons for Pipeline options (https://github.com/HicServices/RDMP/issues/721)
- Add "clause" (e.g. WHERE) to SQL attribute input to make it clearer what SQL you need to enter (https://github.com/HicServices/RDMP/issues/751)
- User Settings dialog now has a nicer layout (https://github.com/HicServices/RDMP/issues/760)


## [7.0.3] - 2021-11-04

### Fixed

- Fixed bug with ConfirmLogs when running with multiple [CacheProgress]

## [7.0.2] - 2021-11-03

### Fixed

- Fixed 'package downgrade' dependencies issue with `HIC.RDMP.Plugin.UI`
- Fixed log viewer total time display in logs view when task ran for > 24 hours.
- Fixed not implemented Exception when using username/password authentication and viewing [CohortIdentificationConfiguration] SQL
- Fixed missing 'add sql file process task' in DLE load stage right click context menus


### Added

- Console gui context menu now shows compatible commands from plugins
- Added the 'ConfirmLogs' command for verifying if a task is failing (e.g. a DLE run)

### Changed

- When syncing table columns with the database, the full column (including table name) is displayed in the proposed fix (previously only the column name was displayed).
- Bump Terminal.Gui from 1.2.1 to 1.3.1

## [7.0.1] - 2021-10-27

### Changed

- Bump NLog from 4.7.11 to 4.7.12
- Bump Microsoft.NET.Test.Sdk from 16.11.0 to 17.0.0
- [Catalogue] and [CatalogueItem] edit tab now expands to fill free space and allows resizing

### Fixed

- Fixed Null Reference exception when collection tabs are opened twice
- Fixed CohortBuilder 'Execute' showing ExceptionViewer on the wrong Thread

### Added

- Column visibility and size are now persisted in UserSettings

### Removed

- Removed FillsFreeSpace on columns.  User must now manually resize columns as desired

## [7.0.0] - 2021-10-18

### Changed

- IPluginUserInterface is now in `Rdmp.Core` and therefore you can write console gui or dual mode (console and winforms) plugin UIs
- IPluginUserInterface CustomActivate now takes IMapsDirectlyToDatabaseTable allowing custom plugin behaviour for activating any object
- DatasetRaceway chart (depicts multiple datasets along a shared timeline) now ignores outlier values (months with count less than 1000th as many records as the average month)
- Renamed `SelectIMapsDirectlyToDatabaseTableDialog` to `SelectDialog<T>` (now supports any object Type)
- Selected datasets icon now includes all symbols of the Catalogue they represent (e.g. ProjectSpecific, Internal)
- Changed how RDMP treats cohorts where the data has been deleted from the cohort table.  'Broken Cohort' renamed 'Orphan Cohort' and made more stable
- [CohortAggregateContainer] now show up in the find dialog (you can disable this in UserSettings)
- Bump Microsoft.Data.SqlClient from 3.0.0 to 3.0.1
- Checks buttons on the toolbars are now hidden instead of disabled when inapplicable
- Shortened tool tips in top menu bar

### Removed

- IPluginUserInterface can no longer add items to tab menu bars (only context menus)
- Removed some Catalogue context menu items when the Catalogue is an API call
- Adding a Filter from Catalogue no longer opens it up in edit mode after adding
- Command line execution (e.g. `rdmp cmd ...`) no longer supports user interactive calls (e.g. YesNo questions)
- Removed PickOneOrCancelDialog
- Removed RAG smiley from server connection UI.  Now errors are reported 'Connection Failed' text label

### Added
- Added CatalogueFolder column to Select Catalogue dialog
- Added custom metadata report tokens:
  - $Comma (for use with formats that require seperation e.g. JSON when using the `$foreach` operation)
  - $TimeCoverage_ExtractionInformation (the column that provides the time element of a dataset to the DQE e.g. StudyDate)
- Added support for default values in constructors invoked from the command line (previously command line had to specify all arguments.  Now you can skip default ones at the end of the line)
- Added support for deleting multiple objects at once with the delete command (e.g. `rdmp cmd Delete Plugin true` to delete all plugins)
  - Boolean flag at the end is optional and defaults to false (expect to delete only 1 object)
  - Use `rdmp cmd DescribeCommand Delete` for more information
- Added ability to directly query Catalogue/DataExport to Console Gui
- Added extraction check that datasets are not marked `IsInternalDataset`
- Added ability to script multiple tables at once via right click context menu in windows client
- Support for shortcodes in arguments to commands on CLI e.g. `rdmp cmd describe c:11`
- Added new command 'AddPipelineComponent' for use with RDMP command line
- Added ability to filter datasets and selected datasets by Catalogue criteria (e.g. Deprecated, Internal)
- Added Clone, Freeze, Unfreeze and add dataset(s) ExtractionConfiguration commands to command line
- Added support for identifying items by properties on CLI (e.g. list all Catalogues with Folder name containing 'edris')
- Cloning a [CohortIdentificationConfiguration] now opens the clone
- Added ability to remove objects from a UI session
- Added new command ViewCohortSample for viewing a sample or extracting all cohort identifiers (and anonymous mapping) to console/file
- Added the ability to pick which tables to import during Bulk Import TableInfos
- Added CLI command to create DLE load directory hierarchy ('CreateNewLoadDirectory')

### Fixed
- Fixed deleting a parameter value set failing due to a database constraint
- Fixed a bug where changing the server/database name could disable the Create button when selecting a database
- Added the ability to drop onto the Core/Project folders in the 'execute extraction' window
- Fixed a big where Yes/No close popup after running a pipeline in console gui could crash on 'No'
- Fixed deleting source/destination pipeline components directly from tree UI
- Fixed various issues when viewing the DQE results of a run on an empty table
- DatasetRaceway in dashboards now shows 'Table(s) were empty for...' instead of `No DQE Evaluation for...` when the DQE was run but there was no result set
- Added better error message when trying to create a new RDMP platform database into an existing database that already has one set up
- Fixed [CohortAggregateContainer] and filter containers not showing up in Find when explicitly requested
- Fixed deleting an [ExtractionFilter] with many parameter values configured.  Now confirmation message is shown and all objects are deleted together
- Fixed bug saving an [ExtractionInformation] when it is an extraction transform without an alias
- Fixed bug refreshing Data Export tree collection when deleting multiple Projects/Packages at once (deleted objects were still shown)
- Fixed bug dragging filters into Cohort Builder

## [6.0.2] - 2021-08-26

### Changed

- Bump Microsoft.NET.Test.Sdk from 16.10.0 to 16.11.0
- Bump NLog from 4.7.10 to 4.7.11

### Added

- Support for plugin Catalogues in cohort builder.  These allow you to write plugins that call out to arbitrary APIs (e.g. REST etc) from the RDMP cohort builder

### Fixed

- Fixed ExecuteCommandCloneCohortIdentificationConfiguration asking for confirmation when activation layer is non interactive

## [6.0.1] - 2021-08-12

### Added

- Added new command 'Similar' for finding columns that have the same name in other datasets
- Added the ability to Query Catalogue/DataExport databases directly through RDMP
- Support for custom column names in ColumnSwapper that do not match the names of the lookup columns
- Added ScriptTables command for scripting multiple [TableInfo] at once (optionally porting schema to alternate DBMS types).
- Support for nullable value/Enum types in command constructors

### Fixed

- AlterColumnType command now shows as IsImpossible when column is part of a view or table valued function
- Describe command no longer shows relationship properties
- Fixed layout of Bulk Process Catalogue Items in dotnet 5
- Fixed missing dependency in new installations when rendering Charts

## [6.0.0] - 2021-07-28

### Changed

- Upgraded Sql Server library from `System.Data.SqlClient` to `Microsoft.Data.SqlClient`
- `ExecuteCommandAlterColumnType` now automatically alters \_Archive table too without asking for confirmation
- When foreign key values are missing from lookups, the 'Missing' status is now attributed to the `_Desc` field (previously to the foreign key field)
- Changed Console gui DLE / DQE (etc) execution to use ListView instead of TextView
- Referencing an object by name in a script file now returns the latest when there are collisions e.g. "[ExtractableCohort]" would return the latest one (created during the script execution session)
- Bump YamlDotNet from 11.2.0 to 11.2.1
- Bump SecurityCodeScan.VS2019 from 5.1.0 to 5.2.1
- Command 'Set' now shows as Impossible for property 'ID'
- RDMP no longer complains about mixed capitalisation in server names and will connect using the capitalisation of the first encountered.

## Fixed

- Fixed release engine not respecting `-g false` (do not release Globals)
- Fixed column order in DQE results graph sometimes resulting in shifted colors (e.g. Correct appearing in red instead of green)
- Fixed Prediction rules never being run when value being considered is null (DQE).
- Fixed a bug creating a cohort without specifying a Project from the console
- Fixed bug where searching in console gui could be slow or miss keystrokes
- Fixed bug in console gui where GoTo Project or Cohort would not highlight the correct item
- Fixed bug in console gui where delete key was not handled resulting in a loop if errors occurred trying to delete the object
- Removed limit of 500 characters on extraction SQL of columns

### Added

- Added user setting for filtering table load logs where there are 0 inserts,updates and deletes
- Added support for specifying datatype when calling `ExecuteCommandAlterColumnType`
- Pipeline and DLE components with object list arguments now show the previously selected items in the 'Select Object(s)' popup
- Pressing 'delete' key in console gui edit window now offers to set value of property to null
- Editing a foreign key property (e.g. `PivotCategory_ExtractionInformation_ID`) now shows objects rather than asking for an `int` value directly
- Fatal errrors in console gui now get logged by NLog (e.g. to console/file)
- Added user setting `CreateDatabaseTimeout`

### Removed

- Removed check for DataLoadProgress being before OriginDate of a `LoadProgress`

## [5.0.3] - 2021-06-17

- Hotfix extraction/DLE progress UI layout on some Windows configurations

## [5.0.2] - 2021-06-16

### Changed

- Bump YamlDotNet from 11.1.1 to 11.2.0


### Fixed

- Fixed layout of windows client engine progress controls not filling all available screen space

## [5.0.1] - 2021-06-08

### Added

- Added CLI console gui context menu for [LoadMetadata]
- Commit cohort from CohortIdentificationConfiguration now shows crash message Exception on failure
- Added `--usc` flag to `rdmp gui`.  This allows you to specify using the `NetDriver` for Terminal.Gui (an alternative display driver)
- Added optional file argument to `ExecuteAggregateGraph` command (outputs graph data table to the file specified)
- Added ability to select a [DataAccessCredentials] in table/database selector control
- Added TopX and Filter (text) to console view logs
- Added alternative colour scheme to console gui

### Changed

- Changed `ExtractMetadata` template syntax to require `DQE_` and added year/month/day sub components:
  - `$StartDate`, `$EndDate` and `$DateRange` are now `$DQE_StartDate`, $DQE_EndDate and $DQE_DateRange.
  - Added `$DQE_StartYear`,`$DQE_EndYear`,`$DQE_StartMonth`,`$DQE_EndMonth`,`$DQE_StartDay`,`$DQE_EndDay`
  - Added `$DQE_PercentNull` (must be used with a `$foreach CatalogueItem` block)
  - Added TableInfo and ColumnInfo properties (e.g. `$Server`)
  - Added $DQE_CountTotal
- Improved performance of checks user interface (especially when there are a large number of check messages)

### Fixed

- Fixed arguments not showing up under Pipeline components of 'Other' (unknown) pipelines node
- Fixed refresh speed of console gui causing problems with Guacamole
- Fixed Keyboard shortcuts of pipeline engine execution window sharing the same letters
- Fixed bug running rdmp gui (console) with a remote current directory
- Fixed 'View Catalogue Data' command when run on ProjectSpecific Catalogues
- Fixed 'Import ProjectSpecific Catalogue' command not preserving Project choice in configure extractability dialog
- When importing an existing data table into RDMP and cancelling [Catalogue] creation RDMP will prompt you to optionally also delete the [TableInfo]

### Dependencies

- Bump Terminal.Gui from 1.0.0 to 1.1.1
- Bump HIC.FAnsiSql from 1.0.6 to 1.0.7
- Bump Microsoft.NET.Test.Sdk from 16.9.4 to 16.10.0


## [5.0.0] - 2021-05-05

### Changed

- .Net 5.0 for all, instead of Framework 4.6.1+Core 2.2+Standard 2.0 mix
- Query editor autocomplete now uses integrated autocomplete (no icons, better matching)
- Throttled how often spelling is checked in Scintilla controls.
- Changed message about inaccessible cohorts to a warning instead of an error. 
- Collation is now explicitly specified when creating a new cohort source using the wizard (as long as there is a single collation amongst existing ColumnInfo of that type)

### Added

- Added `$foreach Catalogue` option for custom metadata report templates (to allow prefix, suffixes, table of contents etc)
- Added ability to search for objects by ID in console gui
- More detailed logging of Type decisions when extracting to database
- Added ability to cancel ongoing queries in CLI Sql Editor
- Added 'Reset Sql' and 'Clear Sql' buttons to CLI Sql Editor
- Added ability to set custom timeout for queries in CLI Sql Editor
- Added ability to save results of CLI Sql Editor (table) to CSV
- Added view data/aggregate etc on ColumnInfo objects to list of commands accessible from the CLI gui
- Added 'Go To' commands to CLI gui
- Exposed 'Add New Process Task...' to load stages in CLI menu
- Added 'ViewCatalogueData' command for CLI and CLI GUI use
- Better error reporting when item validators crash during validation execution (now includes constraint type, column name and value being validated).
- Added 'Go To' commands to CLI gui
- Exposed 'Add New Process Task...' to load stages in CLI menu
- Exposed 'View Logs' commands on CLI and CLI gui
- Added minimum timeout of 5 seconds for `CohortIdentificationConfigurationSource`
- 'View Logs' tree view now accessible for CacheProgress objects
- Added query/result tabs to CLI GUI Sql editor
- Console GUI now shows important information (e.g. 'Disabled') in brackets next to items where state is highly important
- Added new command RunSupportingSql
- Console GUI root nodes now offer sensible commands (e.g. create new Catalogue)
- Added Value column to tree views (allows user to quickly see current arguments' values)
- Added 'other' checkbox to 'Create Catalogue by importing a file' (for selecting custom piplelines)
- Command SetExtractionIdentifier now supports changing the linkage identifier for specific ExtractionConfigurations only
- Added new command `AlterTableMakeDistinct`
- Added CLI GUI window for running Pipelines that displays progress
- Added RDMP.Core version number to logs at startup of rdmp cli
- Added graph commands to CLI:
  - ExecuteCommandSetPivot
  - ExecuteCommandSetAxis
  - ExecuteCommandAddDimension


### Fixed

- Fixed CLI database selection UI not using password mask symbol (`*`)
- Fixed CLI GUI message boxes bug with very long messages
- Fixed Custom Metadata template stripping preceeding whitespace in templated lines e.g. `"  - $Name"` (like you might find in a table of contents section of a template)
- Fixed 'Set Global Dle Ignore Pattern' failing the first time it is used by creating a StandardRegex with no/null Pattern
- Fixed order of branches in CLI gui tree
- Fixed importing filter containers not saving Operation (AND/OR)
- Fixed right click menu not showing when right clicking after selecting multiple objects
- Fixed some delete commands not updating the UI until refreshed (e.g. disassociating a [Catalogue] from a [LoadMetadata])
- Fixed text on disassociating a [Catalogue] from a [LoadMetadata]
- Fixed sort order not being respected in cohort summary screen
- Fixed DQE graph when data has dates before the year 1,000
- Fixed `ExecuteCommandCreateNewCatalogueByImportingFile` when using blank constructor and from CLI GUI
- Fixed extraction UI showing "WaitingForSQLServer" when DBMS might not be (now says "WaitingForDatabase").
- Fixed bug where some UI tabs would not update when changes were made to child objects (e.g. deleting a dataset from an extraction using another window in the client)
- Fixed support for UNC paths in SupportingDocument extraction (e.g. \\myserver\somedir\myfile.txt)
- Fixed not being able to add `Pipeline` objects to Sessions

### Dependencies

- Bump System.Drawing.Common from 5.0.0 to 5.0.2
- Bump Moq from 4.16.0 to 4.16.1
- Bump Microsoft.NET.Test.Sdk from 16.8.3 to 16.9.4
- Bump NLog from 4.7.7 to 4.7.10
- Bump SecurityCodeScan.VS2019 from 5.0.0 to 5.1.0
- Bump Newtonsoft.Json from 12.0.3 to 13.0.1
- Bump YamlDotNet from 9.1.4 to 11.1.1
- Bump NUnit from 3.13.1 to 3.13.2

## [4.2.4] - 2021-02-05

- Added CLI commands for viewing/changing `UserSettings` e.g. AllowIdentifiableExtractions
- Added user setting `ShowPipelineCompletedPopup` for always popping a modal dialog on completion of a pipeline execution in the GUI client (e.g. committing a cohort)
- Added new flexible file/directory extraction component `SimpleFileExtractor`

### Changed

- Globals tickbox can now be checked even when there are no explicit files (this allows implicit files e.g. `SimpleFileExtractor` to still run)

### Fixed 

- Fixed MySql backup trigger implementation not updating validTo on the new row entering the table on UPDATE operations

## [4.2.3] - 2021-02-01

### Fixed 

- Fixed rare threading issue with tree representations of Lookups
- Fixed proxy objects context menus not functioning correctly since 4.2.0 (e.g. Catalogues associated with a load) for some commands

### Dependencies

- Bump NUnit from 3.13.0 to 3.13.1

## [4.2.2] - 2021-01-28

### Added

- Added `patch` command to rdmp CLI e.g. `./rdmp patch -b`
- Added ProjectName to ExtractionConfiguration objects visualisation in Find / Select popups

### Fixed

- Fixed erroneous warning where some characters were wrongly reported as illegal e.g. '#' in Filter names 
- Fixed RemoteDatabaseAttacher not logging table name (only database)

### Changed

- Metadata report now lists Catalogues in alphabetical order
- Changed hierarchy multiple parents state to be a Warning instead of an Error

### Dependencies

- Bump Moq from 4.15.2 to 4.16.0
- Bump YamlDotNet from 9.1.1 to 9.1.4
- Bump NLog from 4.7.6 to 4.7.7
- Bump SSH.NET from 2020.0.0 to 2020.0.1

## [4.2.1] - 2021-01-13

### Added

- Choose Load Directory on DLE now shows old value during editing
- Added property suggestions when using ExecuteCommandSet with an incorrect property name
- Added the ability to drag and drop aggregates into other CohortIdentificationConfigurations to import
- Added ColumnDropper that allows a user to specify the columns that should not be extracted in the pipeline.
- Added Favourite/UnFavourite to right click context menus
- CachingHost now logs the state of the CacheProgress being executed first thing on start
- Home screen now supports right click context menu, drag and drop etc
- Added 'Sessions'.  These are tree collection windows similar to Favourites but with a user defined name and limited duration (until closed)

### Fixed

- Fixed startup error when user enters a corrupt connection string for platform database locations.  This bug affected syntactically invalid (malformed) connection strings (i.e. not simply connection strings that point to non existant databases)
- Fixed various issues in ColumnSwapper
  - If input table contains nulls these are now passed through unchanged
  - If mapping table contains nulls these are ignored (and not used to map input nulls)
  - If input table column is of a different Type than the database table a suitable Type conversion is applied
- Data load engine logging checks are better able to repair issues with missing logging server IDs / logging tasks
- Better support for abort/cancel in
  - RemoteTableAttacher
  - ExcelAttacher
  - KVPAttacher
  - RemoteDatabaseAttacher
- Fixed View Inserts/Updates dialog when using non SqlServer DBMS (e.g. MySql)
- Fixed various layout and performance issues with RDMP console GUI.
- Fixed `rdmp cmd` loop exiting when commands entered result in error.
- Fixed autocomplete in `rdmp cmd` mode and enabled for Linux
- Fixed right click context menu being built twice on right click a new node (once for selection and once for right click)

### Changed

- Added timeout of 10 minutes (previously 30 seconds) for counting unique patient identifiers while writing metadata for extractions
- Choose Load Directory now lets you specify invalid directories e.g. when building a load on one computer designed to run on separate computer with an isolated file system.
- Reinvented Console Gui to more closely resemble the windows client

### Dependencies

- Bump SSH.NET from 2016.1.0 to 2020.0.0

## [4.2.0] - 2020-10-19

### Fixed

- Reduced memory overhead during refreshes
- Fixed various graphical/performance issues when running in VDI environments with limited CPU
- Fixed missing scrollbars in Explicit Column Typing user interface
- Fixed various errors that could occur when a [Catalogue] referenced by an extraction is deleted outside of RDMP (e.g. by truncating the database table(s))

### Added

- Support for importing WHERE logic into extraction datasets from other configurations or cohort builder configurations
- Pipeline ID and Name now recorded in logs for Data Extractions
- Added support for viewing extraction logs in tree form (for a given ExtractionConfiguration)
- Added `AllowIdentifiableExtractions` user setting.  Enabling this prevents RDMP reporting an error state when cohorts are created that have the same private and release ID fields.
- Added GoTo from extraction/cohort building filters to the parent Catalogue level filter and vice versa
- Added ability to suppress [LoadMetadata] triggers
- Added ability for Plugins to store custom information about objects in the RDMP Catalogue platform database
- Added IgnoreColumns setting for DLE to ignore specific columns in the final table completely (not created in RAW/STAGING and not migrated)

### Changed

- CLI tools now built for .Net Core 3.1 since 2.2 has reached EOL

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

[Unreleased]: https://github.com/HicServices/RDMP/compare/v7.0.10...develop
[7.0.10]: https://github.com/HicServices/RDMP/compare/v7.0.9...v7.0.10
[7.0.9]: https://github.com/HicServices/RDMP/compare/v7.0.8...v7.0.9
[7.0.8]: https://github.com/HicServices/RDMP/compare/v7.0.7...v7.0.8
[7.0.7]: https://github.com/HicServices/RDMP/compare/v7.0.6...v7.0.7
[7.0.6]: https://github.com/HicServices/RDMP/compare/v7.0.5...v7.0.6
[7.0.5]: https://github.com/HicServices/RDMP/compare/v7.0.4...v7.0.5
[7.0.4]: https://github.com/HicServices/RDMP/compare/v7.0.3...v7.0.4
[7.0.3]: https://github.com/HicServices/RDMP/compare/v7.0.2...v7.0.3
[7.0.2]: https://github.com/HicServices/RDMP/compare/v7.0.1...v7.0.2
[7.0.1]: https://github.com/HicServices/RDMP/compare/v7.0.0...v7.0.1
[7.0.0]: https://github.com/HicServices/RDMP/compare/v6.0.2...v7.0.0
[6.0.2]: https://github.com/HicServices/RDMP/compare/v6.0.1...v6.0.2
[6.0.1]: https://github.com/HicServices/RDMP/compare/v6.0.0...v6.0.1
[6.0.0]: https://github.com/HicServices/RDMP/compare/v5.0.3...v6.0.0
[5.0.3]: https://github.com/HicServices/RDMP/compare/v5.0.2...v5.0.3
[5.0.2]: https://github.com/HicServices/RDMP/compare/v5.0.1...v5.0.2
[5.0.1]: https://github.com/HicServices/RDMP/compare/v5.0.0...v5.0.1
[5.0.0]: https://github.com/HicServices/RDMP/compare/v4.2.4...v5.0.0
[4.2.4]: https://github.com/HicServices/RDMP/compare/v4.2.3...v4.2.4
[4.2.3]: https://github.com/HicServices/RDMP/compare/v4.2.2...v4.2.3
[4.2.2]: https://github.com/HicServices/RDMP/compare/v4.2.1...v4.2.2
[4.2.1]: https://github.com/HicServices/RDMP/compare/v4.2.0...v4.2.1
[4.2.0]: https://github.com/HicServices/RDMP/compare/v4.1.9...v4.2.0
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

[ExtractionProgress]: ./Documentation/CodeTutorials/Glossary.md#ExtractionProgress
[DBMS]: ./Documentation/CodeTutorials/Glossary.md#DBMS
[UNION]: ./Documentation/CodeTutorials/Glossary.md#UNION
[INTERSECT]: ./Documentation/CodeTutorials/Glossary.md#INTERSECT
[EXCEPT]: ./Documentation/CodeTutorials/Glossary.md#EXCEPT
[IsExtractionIdentifier]: ./Documentation/CodeTutorials/Glossary.md#IsExtractionIdentifier
[DataAccessCredentials]: ./Documentation/CodeTutorials/Glossary.md#DataAccessCredentials
[Catalogue]: ./Documentation/CodeTutorials/Glossary.md#Catalogue
[SupportingDocument]: ./Documentation/CodeTutorials/Glossary.md#SupportingDocument
[TableInfo]: ./Documentation/CodeTutorials/Glossary.md#TableInfo

[ExtractionConfiguration]: ./Documentation/CodeTutorials/Glossary.md#ExtractionConfiguration
[Project]: ./Documentation/CodeTutorials/Glossary.md#Project

[CatalogueItem]: ./Documentation/CodeTutorials/Glossary.md#CatalogueItem
[ExtractionInformation]: ./Documentation/CodeTutorials/Glossary.md#ExtractionInformation
[ColumnInfo]: ./Documentation/CodeTutorials/Glossary.md#ColumnInfo
[CacheProgress]: ./Documentation/CodeTutorials/Glossary.md#CacheProgress

[JoinInfo]: ./Documentation/CodeTutorials/Glossary.md#JoinInfo

[PipelineComponent]: ./Documentation/CodeTutorials/Glossary.md#PipelineComponent
[Pipeline]: ./Documentation/CodeTutorials/Glossary.md#Pipeline
[Pipelines]: ./Documentation/CodeTutorials/Glossary.md#Pipeline

[Lookup]: ./Documentation/CodeTutorials/Glossary.md#Lookup
[CohortIdentificationConfiguration]: ./Documentation/CodeTutorials/Glossary.md#CohortIdentificationConfiguration
[LoadMetadata]: ./Documentation/CodeTutorials/Glossary.md#LoadMetadata
[ExtractableCohort]: ./Documentation/CodeTutorials/Glossary.md#ExtractableCohort
[CohortAggregateContainer]: ./Documentation/CodeTutorials/Glossary.md#CohortAggregateContainer
[ExtractionFilter]: ./Documentation/CodeTutorials/Glossary.md#ExtractionFilter
[MigrateUsages]: https://github.com/HicServices/RDMP/pull/666
[ExternalDatabaseServer]: ./Documentation/CodeTutorials/Glossary.md#ExternalDatabaseServer
