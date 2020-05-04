# ![logo](./Application/ResearchDataManagementPlatform/Icon/mainsmall.png)Research Data Management Platform

[![Build Status](https://travis-ci.org/HicServices/RDMP.svg?branch=master)](https://travis-ci.org/HicServices/RDMP) [![Total alerts](https://img.shields.io/lgtm/alerts/g/HicServices/RDMP.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/HicServices/RDMP/alerts/) [![NuGet Badge](https://buildstats.info/nuget/HIC.RDMP.Plugin)](https://buildstats.info/nuget/HIC.RDMP.Plugin) [![Coverage Status](https://coveralls.io/repos/github/HicServices/RDMP/badge.svg?branch=develop)](https://coveralls.io/github/HicServices/RDMP?branch=develop)

- [Releases](https://github.com/HicServices/RDMP/releases)
- [FAQ](Documentation/CodeTutorials/FAQ.md)
- [User Manual](https://github.com/HicServices/RDMP/raw/master/Documentation/UserManual.docx)
- [Glossary](./Documentation/CodeTutorials/Glossary.md)
- [Changelog](./CHANGELOG.md)
- [Libraries](./Documentation/CodeTutorials/Packages.md)

RDMP is a free, open source software application for the loading,linking,anonymisation and extraction of datasets stored in SQL databases (Sql Server, MySql and Oracle).  It is designed to assist with data provenance, preserving domain knowledge and configuration management of linkage/cohort generation workflows.

RDMP does not require your data be moved or transformed prior to processing and is integrates into existing SQL based extraction practices.

![image](./Application/ResearchDataManagementPlatform/Icon/RdmpFlow.svg?sanitize=true)

## Install

Signed release binaries for the RDMP client and Command Line Interface (CLI) are in the available in the [GitHub releases section](https://github.com/HicServices/RDMP/releases).

## Build

You can build directly through Visual Studio (**2017 or later**) by opening HIC.DataManagementPlatform.sln.  You will also need to install the DotNetCore 2.2 SDK.  The startup project for the main RDMP user interface is ResearchDataManagementPlatform.csproj.

Alternatively you can run `msbuild` or `rake build` (set path to MSBuild15CMD in `rakeconfig.rb` first) to perform a console build.

## Integration Test Database
In addition to unit tests, the RDMP test suite includes many Integration tests which require writing to a database.  You can [read how to set up your test environment in Tests.md](Documentation/CodeTutorials/Tests.md).
