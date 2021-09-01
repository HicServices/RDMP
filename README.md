# ![logo](./Application/ResearchDataManagementPlatform/Icon/mainsmall.png)Research Data Management Platform

[![Build status](https://github.com/HicServices/RDMP/workflows/Build/badge.svg)](https://github.com/HicServices/RDMP/actions?query=workflow%3ABuild) [![Total alerts](https://img.shields.io/lgtm/alerts/g/HicServices/RDMP.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/HicServices/RDMP/alerts/) [![NuGet Badge](https://buildstats.info/nuget/HIC.RDMP.Plugin)](https://buildstats.info/nuget/HIC.RDMP.Plugin) [![Coverage Status](https://coveralls.io/repos/github/HicServices/RDMP/badge.svg?branch=develop)](https://coveralls.io/github/HicServices/RDMP?branch=develop)

- [Demo Video](https://www.youtube.com/watch?v=Fgi9-Sdup-Y)
- [Releases](https://github.com/HicServices/RDMP/releases)
- [FAQ](Documentation/CodeTutorials/FAQ.md)
- [User Manual](https://github.com/HicServices/RDMP/raw/master/Documentation/UserManual.docx)
- [Glossary](./Documentation/CodeTutorials/Glossary.md)
- [Changelog](./CHANGELOG.md)
- [Libraries](./Documentation/CodeTutorials/Packages.md)

RDMP is a free, open source software application for cohort building, loading, linking, anonymisation and extraction of datasets stored in relational databases (Sql Server, MySql, Postgres and Oracle). It was designed from the bottom up to support with data provenance, preserving domain knowledge and configuration management workflows.

RDMP does not require your data be moved or transformed prior to processing and is integrates into existing SQL based extraction practices.

![image](./Application/ResearchDataManagementPlatform/Icon/RdmpFlow.svg?sanitize=true)

## Install

Signed release binaries for the RDMP client and Command Line Interface (CLI) are in the available in the [GitHub releases section](https://github.com/HicServices/RDMP/releases).

## Build

You can build directly through Visual Studio (**2017 or later**) by opening HIC.DataManagementPlatform.sln.  You will also need to install the dotnet5 SDK.  The startup project for the main RDMP user interface is ResearchDataManagementPlatform.csproj.

You can build from the command line with:

```
dotnet build
```

Run tests with:

```
dotnet test ./scripts/run-all-tests.proj -c Release -p:BuildInParallel=false
```

The first time you run tests you will likely see many inconclusive tests e.g.

```
Test Run Successful.
Total tests: 1455
     Passed: 571
    Skipped: 2
 Total time: 1.5237 Minutes
```

This is because many tests require RDMP platform databases and/or specific [DBMS] engines to run.  [Read how to set up your test environment in Tests.md](Documentation/CodeTutorials/Tests.md).

## Contributing

We welcome all contributions including:

- [Issues and bug reports](https://github.com/HicServices/RDMP/issues)
- Code Reviews
- [Translations](./Documentation/CodeTutorials/Localization.md)
- Documentation
- [Code Contributions](./Documentation/CodeTutorials/Coding.md)

[DBMS]: ./Documentation/CodeTutorials/Glossary.md#DBMS
