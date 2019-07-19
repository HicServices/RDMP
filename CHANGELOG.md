# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- Attempting to generate a graph from a query that returns more than 1,000,000 cells now asks for confirmation.
- Updated to latest version of [FAnsiSql] (0.9.3) for better Oracle support
- Oracle extraction commands no longer generate parameters (e.g. @projectNumber).  Previously invalid SQL was generated.

### Fixed 
- Fixed bug in Catalogue validation setup window (DQE Validation Rules) which resulted in changes not being saved if it had been refreshed after initially loading
- Fixed scrollbars not appearing in Catalogue validation setup window when lots of validation rules are applied to a single column
- Type text dialog prompt now resizes correctly and has a display limit of 20,000 characters for messages
- Fixed bug that prevented exiting if the RDMP directory (in user's application data folder) was deleted while the program was running
- Fixed bug where CatalogueItems created when importing Oracle tables had database qualifiers in the name e.g. "CHI" (including the double quotes)

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

[Unreleased]: https://github.com/HicServices/RDMP/compare/v3.0.16-rc2...develop
[3.0.16-rc2]: https://github.com/HicServices/RDMP/compare/v3.0.16-rc...v3.0.16-rc2
[3.0.16-rc]: https://github.com/HicServices/RDMP/compare/v3.0.15...v3.0.16-rc
[FAnsiSql]: https://github.com/HicServices/FAnsiSql/
[BadMedicine]: https://github.com/HicServices/BadMedicine/