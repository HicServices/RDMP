# ![logo](./Application/ResearchDataManagementPlatform/Icon/mainsmall.png)Research Data Management Platform

[![Build status](https://github.com/HicServices/RDMP/workflows/Build/badge.svg)](https://github.com/HicServices/RDMP/actions?query=workflow%3ABuild) [![Total alerts](https://img.shields.io/lgtm/alerts/g/HicServices/RDMP.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/HicServices/RDMP/alerts/) [![NuGet Badge](https://buildstats.info/nuget/HIC.RDMP.Plugin)](https://www.nuget.org/packages/HIC.RDMP.Plugin) [![Coverage Status](https://coveralls.io/repos/github/HicServices/RDMP/badge.svg?branch=develop)](https://coveralls.io/github/HicServices/RDMP?branch=develop)

- [Demo Video](https://www.youtube.com/watch?v=Fgi9-Sdup-Y)
- [Releases](https://github.com/HicServices/RDMP/releases)
- [FAQ](Documentation/CodeTutorials/FAQ.md)
- [User Manual](./Documentation/CodeTutorials/UserManual.md)
- [Glossary](./Documentation/CodeTutorials/Glossary.md)
- [Changelog](./CHANGELOG.md)
- [Libraries](./Documentation/CodeTutorials/Packages.md)
- [New Developers](./NoteForNewDevelopers.md)

RDMP is a free, open source software application for cohort building, loading, linking, anonymisation and extraction of datasets stored in relational databases (SQL Server, MySQL, Postgres and Oracle). It was designed from the bottom up to support with data provenance, preserving domain knowledge and configuration management workflows.

RDMP does not require your data be moved or transformed prior to processing and is integrates into existing SQL based extraction practices.

![image](./Application/ResearchDataManagementPlatform/Icon/RdmpFlow.svg?sanitize=true)

## Install

Signed release binaries for the RDMP client and Command Line Interface (CLI) are in the available in the [GitHub releases section](https://github.com/HicServices/RDMP/releases).

### Windows Install Guide

Download and unzip `rdmp-client.zip` from the [GitHub releases section](https://github.com/HicServices/RDMP/releases) and run `ResearchDataManagementPlatform.exe`.  This will take you to an installation/setup screen which will guide you through the rest of the initial setup process.

### Linux CLI Install Guide

The following steps can be used to install the RDMP CLI and start an SqlServer docker container to install into. __Make sure to set the version (e.g. 7.0.14) to the latest and set a custom password if desired__.

```
wget https://github.com/HicServices/RDMP/releases/download/v7.0.14/rdmp-cli-linux-x64.zip

unzip -d rdmp-cli ./rdmp-cli-linux-x64.zip

sudo docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<YourStrong@Passw0rd>" \
   -p 1433:1433 --name sql1 --hostname sql1 \
   -d mcr.microsoft.com/mssql/server:2019-latest

cd rdmp-cli

cat > Databases.yaml << EOF
CatalogueConnectionString: Server=localhost;Database=RDMP_Catalogue;User ID=SA;Password=<YourStrong@Passw0rd>;Trust Server Certificate=true;
DataExportConnectionString: Server=localhost;Database=RDMP_DataExport;User ID=SA;Password=<YourStrong@Passw0rd>;Trust Server Certificate=true;
EOF

chmod +x ./rdmp

./rdmp install localhost RDMP_ -e -D -u SA -p "<YourStrong@Passw0rd>"
./rdmp gui
```


## Build


### Building on Windows

You can build RDMP from the command line using `dotnet build` or through an IDE e.g. Visual Studio or Visual Studio Code (Requires [latest dotnet SDK](https://dotnet.microsoft.com/download/dotnet/)).

The Windows client:

```
dotnet build
cd Application\ResearchDataManagementPlatform\bin\Debug\net6.0-windows\win-x64
./ResearchDataManagementPlatform.exe
```

The console client:

```
dotnet build
cd Tools\rdmp\bin\Debug\net6.0\
./rdmp.exe --help
```

### Building on Linux

Only the console client can be built/run in Linux

```
cd Tools/rdmp
dotnet build
cd bin/Debug/net6.0
./rdmp --help
```

### Tests

To run tests you will need an instance of SQL Server.  These instructions use localdb which is included in [visual studio](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver15).

If using a docker container or alternate sql server instance then substitute your host name in place of `(localdb)\MSSQLLocalDB`

```
dotnet build
./Tools/rdmp/bin/Debug/net6.0/rdmp.exe install "(localdb)\MSSQLLocalDB" TEST_ -d

echo "ServerName: (localdb)\MSSQLLocalDB" > ./Tests.Common/TestDatabases.txt
echo "Prefix: TEST_" >> ./Tests.Common/TestDatabases.txt

dotnet build

dotnet test ./scripts/run-all-tests.proj -c Release -p:BuildInParallel=false
```

For a more indepth guide to CI testing see [How to set up your test environment in Tests.md](Documentation/CodeTutorials/Tests.md).

## Contributing

We welcome all contributions including:

- [Issues and bug reports](https://github.com/HicServices/RDMP/issues)
- Code Reviews
- [Translations](./Documentation/CodeTutorials/Localization.md)
- Documentation
- [Code Contributions](./Documentation/CodeTutorials/Coding.md)

[DBMS]: ./Documentation/CodeTutorials/Glossary.md#DBMS
