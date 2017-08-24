# RDMP

## Build

	You can build directly through Visual Studio by opening HIC.DataManagementPlatform.sln.  Do not worry about the unloadable installer projects, these require WiX to build and are not immediately required.
	
	Or alternatively you can run build.bat to perform a console build
	cd .\Uppercut
	.\build.bat [TEST|LIVE]
	
## Integration Test Database
	If you run unit tests without first creating a test database suite, all database integration tests will fail.  To create the test databases you will need a Microsoft Sql Server (Sql Express is fine and free).  Once you have an instance installed you can run DatabaseCreation.exe with appropriate parameters.  For example if you have a local server you can run ".\DatabaseCreation\bin\Debug\DatabaseCreation.exe localhost\sqlexpress TEST_ -D".  This will create databases for Catalogue/Data Export etc with a prefix of 'TEST_', you can also create another database set with a different prefix if you want to be able to do manual tests or continue development while the tests run e.g. ".\DatabaseCreation\bin\Debug\DatabaseCreation.exe localhost\sqlexpress DEMO_ -D".  __The -D flag drops any existing databases with the target name__.
	
	If you need to change the server name or database prefix from the above example then before running the integration tests you will have to update ".\Tests.Common\DatabaseTests.txt" to have a matching servername and prefix.
	
	__WARNING__:Integration tests will delete all database contents of the TEST_ databases before each test is run so make sure you do not point DatabaseTests.txt to your live database/server.

## Running the software
	In order to run the software you can launch the startup project '.\Application\ResearchDataManagementPlatform'.  This will launch the startup user interface which will guide you through selecting your platform databases (See the 'Integration Test Database' section above for details on how to create these).  Once the main screen appears you can select 'Help=>Show User Manual'.
	
### Verbosity

Can pass verbosity flag to MSBuild task, for example:

	.\build.bat TEST "-D:build.verbosity=diagnostic"

Acceptable values from [MSDN](https://msdn.microsoft.com/en-us/library/ms164311.aspx):

> You can specify the following verbosity levels: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].

### Logging

Build log is at `.\Uppercut\build_output\build.log`
