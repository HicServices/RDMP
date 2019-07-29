# ![logo](/Application/ResearchDataManagementPlatform/Icon/mainsmall.png)Research Data Management Platform
RDMP is a free, open source software application for the loading,linking,anonymisation and extraction of datasets stored in SQL databases (Sql Server, MySql and Oracle).  It is designed to assist with data provenance, preserving domain knowledge and configuration management of linkage/cohort generation workflows.

RDMP does not require your data be moved or transformed prior to processing and is integrates into existing SQL based extraction practices.

![image](./Application/ResearchDataManagementPlatform/Icon/RdmpFlow.svg?sanitize=true)

## Install

Signed release binaries for the RDMP client and Command Line Interface (CLI) are in the available in the releases section (https://github.com/HicServices/RDMP/releases).

## Documentation

The UserManual can be downloaded at <https://github.com/HicServices/RDMP/raw/master/Documentation/UserManual.docx>.  Additionally there is a comprehensive [Frequently Asked Questions List](Documentation/CodeTutorials/FAQ.md)

## Changelog

A history of all additions, changes and bugfixes is kept in [CHANGELOG.md](./CHANGELOG.md)

## Build

You can build directly through Visual Studio (**2017 or later**) by opening HIC.DataManagementPlatform.sln.  The startup project for the main RDMP user interface is ResearchDataManagementPlatform.csproj.

Alternatively you can run `msbuild` or `rake build` (set path to MSBuild15CMD in `rakeconfig.rb` first) to perform a console build.

## Integration Test Database
In addition to unit tests, the RDMP test suite includes many Integration tests which require writing to a database.  You can [read how to set up your test environment in Tests.md](Documentation/CodeTutorials/Tests.md).
