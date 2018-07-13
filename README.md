# ![logo](/Application/ResearchDataManagementPlatform/Icon/mainsmall.png)Research Data Management Platform
RDMP is a free, open source software application for the loading,linking,anonymisation and extraction of datasets stored in relational databases.  It is designed to assist data analysts in their current linkage workflows without requiring underlying data sources to be moved or altered.

## Install
RDMP is available as a ClickOnce package at <https://hic.dundee.ac.uk/Installers/RDMP/Stable/>
## User Manual
The RDMP UserManual is available on GitHub at <https://github.com/HicServices/RDMP/raw/master/Documentation/UserManual.docx>

## Developer FAQ
In addition to the User Manual, RDMP has a comprehensive developer orientated [Frequently Asked Questions List](Documentation/CodeTutorials/FAQ.md)

## Build

You can build directly through Visual Studio by opening HIC.DataManagementPlatform.sln.

Or alternatively you can run build.bat to perform a console build

```
cd .\Uppercut
.\build.bat [TEST|LIVE]
```

## Integration Test Database
Before running tests you should run DatabaseCreation.exe with appropriate parameters

For example if you have a local sql server express instance on your development PC you can run 
```
cd .\Tools\DatabaseCreation\bin\Debug\
DatabaseCreation.exe localhost\sqlexpress TEST_ -D
```

If you need to change the server name or database prefix from the above example then before running the integration tests you will have to update ".\Tests.Common\DatabaseTests.txt" to have a matching servername and prefix.

__WARNING__:DatabaseTests will delete the contents of the TEST_ databases before each test is run and some will create temporary databases/tables during runtime, therefore it is important that you do not use a production server for integration testing

See also [Tests.md](Documentation/CodeTutorials/Tests.md) for a full write up.

## Running the software
The easiest way to run RDMP is via the click once package (See Install) but if you want to run it directly from source then launch the startup project 
	'.\Application\ResearchDataManagementPlatform'

This will launch the startup user interface which will guide you through selecting your platform databases (See the 'Integration Test Database' section above for details on how to create these).  Once the main screen appears you can select 'Help=>Show User Manual'.
### Verbosity

Can pass verbosity flag to MSBuild task, for example:

	.\build.bat TEST "-D:build.verbosity=diagnostic"

Acceptable values from [MSDN](https://msdn.microsoft.com/en-us/library/ms164311.aspx):

> You can specify the following verbosity levels: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].

### Logging

Build log is at `.\Uppercut\build_output\build.log`
