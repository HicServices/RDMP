# Table of contents
1. [RDMP Binary and Documentation](#binary)
1. [Hello World Plugin](#helloWorldPlugin)
2. [Debugging](#debugging)
5. [A basic anonymisation plugin](#basicAnoPlugin)
  * [Version 1](#anoPluginVersion1)
  * [Version 2](#anoPluginVersion2)
  * [Version 3](#anoPluginVersion3)
5. [Tests](#tests)
  * [Unit Tests](#unitTests)
  * [Setting up Database Tests](#databaseTestsSetup)
  * [Writing a Database Test](#databaseTestsWriting)
6. [Checks](#checks)
  * [Version 4](#anoPluginVersion4)
7. [Progress Logging](#progress)
  * [Version 5](#anoPluginVersion5)
  * [What is wrong with NLog etc?](#NLog)
  * [What other funky things can I do with IDataLoadEventListener?](#funkyIDataLoadEventListener)
8. [Graphical User Interfaces In Plugins](#guis)
9. [Dependencies](#dependencies)
10. [Troubleshooting Plugins](#troubleshooting)

<a name="binary"></a>
# RDMP Binary and Documentation
The Research Data Management Platform Binaries are available on [Releases section of Github](https://github.com/HicServices/RDMP/releases).  In order to use RDMP you will also need access to a Microsoft Sql Server (or Sql Express).  After completing the setup process the main UI will launch.

From here you can access several resources that help understand RDMP classes / patterns etc.  

![Main application Help](Images/Help.png)

Firstly there is the `Help=>Show User Manual` (also available at [UserManual.md](./UserManual.md)).

Secondly there is the `Help=>Generate Class/Table Summary` which describes the DatabaseEntity objects that appear in RDMPCollectionUIs and are core concepts for RDMP.

![Class Documentation](Images/ClassDocumentation.png)

Thirdly `Help=>Show Help` will show a dialog telling you what User Interface control you are in (class name) and any comments the class has (works for content tabs only - not collection trees).

Fourthly there is the Tutorial system `Help=>Tutorials` which cover the basics for setting up RDMP test data, importing files etc.

 <a name="helloWorldPlugin"></a>
 # Hello World Plugin

Rdmp plugins must be packaged as [NuGet packages](https://en.wikipedia.org/wiki/NuGet) (e.g. MyPlugin.0.0.1.nupkg).

Create a new project and add a reference to the nuget package [HIC.RDMP.Plugin](https://www.nuget.org/packages/HIC.RDMP.Plugin/)

```
dotnet new classlib -n MyPlugin -o ./MyPlugin
cd ./MyPlugin
dotnet add package HIC.RDMP.Plugin
```

Add the following classes:

```csharp
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using System.Collections.Generic;

namespace MyPlugin
{
    class MyPluginUI : PluginUserInterface
    {
        public MyPluginUI(IBasicActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override IEnumerable<IAtomicCommand> GetAdditionalRightClickMenuItems(object o)
        {
            if(o is Catalogue)
            {
                yield return new ExecuteCommandHelloWorld(BasicActivator);
            }
        }
    }


    internal class ExecuteCommandHelloWorld : BasicCommandExecution
    {
        public ExecuteCommandHelloWorld(IBasicActivateItems activator) : base(activator)
        {

        }
        public override void Execute()
        {
            BasicActivator.Show("Hello World!");
        }
    }
}
```

Create a nuspec file called `MyPlugin.nuspec` that describes your plugin:

```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
    <metadata>
        <id>MyPlugin</id>
        <version>0.0.1</version>
        <authors>Me</authors>
        <description>My RDMP Plugin</description>
        <dependencies>
            <!-- Make sure this matches your running RDMP major/minor versions-->
            <dependency id="HIC.RDMP.Plugin" version="7.0" />
        </dependencies>
    </metadata>
    <files>
    <file src="bin\Debug\net6.0\*" target="lib\main" />	
    </files>
</package>
```
_Substitute 'net6.0' for whatever version of dotnet you are using_

Build the solution and package it with the [nuget cli app](https://www.nuget.org/downloads):
```
dotnet build
nuget pack MyPlugin.nuspec
```

This should produce a file called `MyPlugin.0.0.1.nupkg`.  Open RDMP and click the `Tables(Advanced)` collection and right click `Plugins`.  Navigate to your plugin package.

 ![Adding a plugin via the RDMP user interface](Images/AddPluginContextMenu.png)

Make sure that you have listed the correct RDMP Major/Minor version in the nuspec file (7.0 in the xml example above).  Otherwise when you add it you will get an error:

```
Plugin version 0.0.1 is incompatible with current running version of RDMP (7.0.0).
```

Restart RDMP and right click a Catalogue

 ![What it should look like](Images/HelloWorldSuccess.png)

If you have the RDMP command line you can also call your command from there:

```
./rdmp cmd HelloWorld
```

For example on windows running in powershell the following output would appear:
```
PS Z:\Repos\RDMP\Tools\rdmp\bin\Debug\net6.0> ./rdmp cmd HelloWorld
2021-10-19 10:42:00.7839 INFO Dotnet Version:5.0.10 .
2021-10-19 10:42:00.8063 INFO RDMP Version:7.0.0.0 .
2021-10-19 10:42:01.6559 INFO Setting yaml config value for CatalogueConnectionString .
2021-10-19 10:42:01.6568 INFO Setting yaml config value for DataExportConnectionString .
Hello World!
Command Completed
2021-10-19 10:42:05.9123 INFO Exiting with code 0 .
```

 <a name="debugging"></a>
 # Debugging
If you want to debug your plugin, first delete it in RDMP.  Then set the output build directory to the location of the RDMP binary e.g.:

```
dotnet build -o Z:\rdmp-client
```

Launch the RDMP binary and then attach the visual studio debugger (Debug=>Attach to Process)

<a name="basicAnoPlugin"></a>
# A (very) basic Anonymisation Plugin

We have seen how UI plugins work, now we will write a plugin which transforms data.

These instructions will expand on the [Hello World Plugin](#helloWorldPlugin) above and will assume the files are already there from that tutorial.

<a name="anoPluginVersion1"></a>
## Version 1

Most of the processes in RDMP use the [Pipeline] system.  This involves a series of components performing operations on a flow of objects of type T (often a `System.Data.DataTable`).  The pipeline is setup/tailored by RDMP users and then reused every time the task needs to be executed.  For example importing a csv file into the database and generating a [Catalogue] from the resulting table (the first thing you do when playing with the RDMP test data) happens through a pipeline called `BULK INSERT: CSV Import File (automated column-type detection)`.

We will write a reusable component which lets the user identify problem strings (names) in data they are importing.

Declare a new class `BasicDataTableAnonymiser1` and implement IPluginDataFlowComponent<DataTable>:


```csharp
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace MyPipelinePlugin
{
    public class BasicDataTableAnonymiser1 : IPluginDataFlowComponent<DataTable>
    {
        public void Abort(IDataLoadEventListener listener)
        {

        }

        public void Check(ICheckNotifier notifier)
        {

        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {

        }

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            //Go through each row in the table
            foreach (DataRow row in toProcess.Rows)
            {
                //for each cell in current row
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    //if it is a string
                    var stringValue = row[i] as string;

                    if (stringValue != null)
                    {
                        //replace any common names with REDACTED
                        foreach (var name in CommonNames)
                            stringValue = Regex.Replace(stringValue, name, "REDACTED", RegexOptions.IgnoreCase);

                        row[i] = stringValue;
                    }
                }
            }

            return toProcess;
        }

        string[] CommonNames = new string[]
        {
            "Dave","Frank","Bob","Pete","Daisy","Marley","Lucy","Tabitha","Thomas","Wallace"
        };

    }
}
```

Rebuild the plugin into the RDMP bin folder and run it:

```
dotnet build -o Z:\rdmp-client
cd Z:\rdmp-client\
.\ResearchDataManagementPlatform.exe
```

In the RDMP Client edit the file import Pipeline called `BULK INSERT: CSV Import File (automated column-type detection)` by adding your plugin class:

![Editting a pipeline - Version 1](Images/EditPipelineComponentVersion1.png)

If your plugin doesn't appear see [Troubleshooting Plugins](#troubleshooting).

Create a new demography csv file using `Diagnostics->Generate Test Data...`.  Import this file into RDMP using your modified pipeline

![What it should look like](Images/ImportCatalogue.png)

Execute the import and do a select out of the final table to confirm that it has worked:

```sql
select * from test..demography where forename like '%REDACTED%'
```

<a name="anoPluginVersion2"></a>
## Version 2 - Adding arguments
You can add user configured properties by declaring public properties decorated with `[DemandsInitialization]`.  This attribute is supported on a wide range of common Types (see `Rdmp.Core.Curation.Data.DataLoad.Argument`.`PermissableTypes` for a complete list) and some RDMP object Types (e.g. [Catalogue]).  Let's add a file list of common names and a regular expression that lets you skip columns you know won't have any names in.

Add a new component BasicDataTableAnonymiser2 (or adjust your previous component).  Add two public properties as shown below.

```csharp
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace MyPipelinePlugin
{
    public class BasicDataTableAnonymiser2: IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("List of names to redact from columns", mandatory:true)]
        public FileInfo NameList { get; set; }

        [DemandsInitialization("Columns matching this regex pattern will be skipped")]
        public Regex ColumnsNotToEvaluate { get; set; }

        private string[] _commonNames;
        
        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,GracefulCancellationToken cancellationToken)
        {
            if (_commonNames == null)
                _commonNames = File.ReadAllLines(NameList.FullName);

            //Go through each row in the table
            foreach (DataRow row in toProcess.Rows)
            {
                //for each cell in current row
                foreach (DataColumn col in toProcess.Columns)
                {
                    //if it's not a column we are skipping
                    if(ColumnsNotToEvaluate != null && ColumnsNotToEvaluate.IsMatch(col.ColumnName))
                        continue;
                    
                    //if it is a string
                    var stringValue = row[col] as string;

                    if(stringValue != null)
                    {
                        //replace any common names with REDACTED
                        foreach (var name in _commonNames)
                            stringValue =  Regex.Replace(stringValue, name, "REDACTED",RegexOptions.IgnoreCase);

                        row[col] = stringValue;
                    }
                }
            }

            return toProcess;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            
        }
    }
}

```

Rebuild the new version (into the RDMP bin directory) and restart RDMP.

Drop the demography table from your database (and delete any associated Catalogues / TableInfos in RDMP).  Import demography.csv again but edit the pipeline to include the new component BasicDataTableAnonymiser2.  Now when you select it you should be able to type in some values.

For NameList create a file with a few basic names (don't put any blank lines in the file or your likely to end up redacting spaces!)

```
Dave
Frank
Peter
Angela
Laura
Emma
```

![Editting a pipeline - Version 2](Images/EditPipelineComponentVersion2.png)

<a name="anoPluginVersion3"></a>
## Version 3 - Referencing a database table
Having a text file isn't that great, it would be much better to power it with a database table.  

Create a new plugin component BasicDataTableAnonymiser3 (or modify your existing one).  Get rid of the property NameList and add a [TableInfo] one instead:

```csharp
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

namespace MyPipelinePlugin
{
    public class BasicDataTableAnonymiser3: IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("Table containing a single column which must have a list of names to redact from columns", mandatory:true)]
        public TableInfo NamesTable { get; set; }

        [DemandsInitialization("Columns matching this regex pattern will be skipped")]
        public Regex ColumnsNotToEvaluate { get; set; }

        private string[] _commonNames;
        
        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,GracefulCancellationToken cancellationToken)
        {
            if (_commonNames == null)
            {
                //discover the table
                var tableDiscovered = NamesTable.Discover(DataAccessContext.DataLoad);

                //make sure it exists
                if(!tableDiscovered.Exists())
                    throw new NotSupportedException("TableInfo '" + tableDiscovered + "' does not exist!");
                
                //Download all the data
                var dataTable = tableDiscovered.GetDataTable();

                //Make sure it has the correct expected schema (i.e. 1 column)
                if(dataTable.Columns.Count != 1)
                    throw new NotSupportedException("Expected a single column in DataTable '" + tableDiscovered +"'");

                //turn it into an array (throwing out any nulls)
                _commonNames = dataTable.Rows.Cast<DataRow>().Select(r => r[0] as string).Where(s=>!string.IsNullOrWhiteSpace(s)).ToArray();
            }

            //Go through each row in the table
            foreach (DataRow row in toProcess.Rows)
            {
                //for each cell in current row
                foreach (DataColumn col in toProcess.Columns)
                {
                    //if it's not a column we are skipping
                    if(ColumnsNotToEvaluate != null && ColumnsNotToEvaluate.IsMatch(col.ColumnName))
                        continue;
                    
                    //if it is a string
                    var stringValue = row[col] as string;

                    if(stringValue != null)
                    {
                        //replace any common names with REDACTED
                        foreach (var name in _commonNames)
                            stringValue =  Regex.Replace(stringValue, name, "REDACTED",RegexOptions.IgnoreCase);

                        row[col] = stringValue;
                    }
                }
            }

            return toProcess;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            
        }
    }
}

```

You will need to create the names table:

```sql

use test

create table NamesListTable 
(
Name varchar(500) primary key,
)
go

insert into NamesListTable values ('Thomas')
insert into NamesListTable values ('Mitchell')
insert into NamesListTable values ('Davis')
insert into NamesListTable values ('Walker')
insert into NamesListTable values ('Saunders')

```

And import it into RDMP as a [TableInfo] (you don't need to create a Catalogue if you don't want to, just the [TableInfo] part)

![Import TableInfo - Version 3](Images/ImportExistingTableInfo.png)

Test the plugin by importing demography.csv again through the pipeline with the new component implmentation

<a name="tests"></a>
# Tests

<a name="unitTests"></a>
## Unit Tests 
We definetly want to write some unit/integration tests for this component.  Create a new project called MyPipelinePluginTests.  

```
dotnet new classlib -n MyPipelinePluginTests -o MyPipelinePluginTests
cd MyPipelinePluginTests
dotnet add reference ../MyPlugin/MyPlugin.csproj
```


References to the following NuGet packages:

```
dotnet add package HIC.RDMP.Plugin.Test
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package NunitXml.TestLogger
```

Add the following test:

```csharp
using MyPipelinePlugin;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Progress;
using System.Data;

namespace MyPipelinePluginTests
{
    public class TestAnonymisationPlugins
    {
        [Test]
        public void TestBasicDataTableAnonymiser1()
        {
            var dt = new DataTable();
            dt.Columns.Add("Story");
            dt.Rows.Add(new[] {"Thomas went to school regularly"});
            dt.Rows.Add(new[] {"It seems like Wallace went less regularly"});
            dt.Rows.Add(new[] {"Mr Smitty was the teacher"});

            var a = new BasicDataTableAnonymiser1();
            var resultTable = a.ProcessPipelineData(dt,new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());

            Assert.AreEqual(resultTable.Rows.Count,3);
            Assert.AreEqual("REDACTED went to school regularly",resultTable.Rows[0][0]);
            Assert.AreEqual("It seems like REDACTED went less regularly",resultTable.Rows[1][0]);
            Assert.AreEqual("Mr Smitty was the teacher",resultTable.Rows[2][0]);
        }
    }
}

```

This is a very basic test.  We create a data table that would be flowing through our pipeline (e.g. as it was read from a csv file) and look for the REDACTED names appearing.  The only new bits we need to worry about are `ThrowImmediatelyDataLoadEventListener` and `GracefulCancellationToken`.  

`GracefulCancellationToken` is a wrapper for two `CancellationToken` (Abort and Cancel).  Since our component doesn't support aborting/cancelling anyway we don't need to worry about it.

`IDataLoadEventListener` is the interface that handles messages generated by data flow components, this includes progress messages (done 1000 of x records) and notifications (Information, Warning, Error).  There are many implementations of `IDataLoadEventListener` including user interface components (e.g. `ProgressUI`) but we will use `ThrowImmediatelyDataLoadEventListener` this is a data class that treats Error messages as Exceptions (hence the throw) but otherwise writes progress messages to the Console.

<a name="databaseTestsSetup"></a>

## Setting up Database Tests

Lets look at testing `BasicDataTableAnonymiser3`, this is harder since it involves having a user specified [TableInfo] that references a table of names.  We can do this though.

Start by making a new class `TestAnonymisationPluginsDatabaseTests` and inherit from `Tests.Common.DatabaseTests`:

```
using NUnit.Framework;
using Tests.Common;

namespace MyPipelinePluginTests
{
    class TestAnonymisationPluginsDatabaseTests: DatabaseTests
    {
        [Test]
        public void Test()
        {
            Assert.Pass();
        }
    }
}
```

Run the unit test again.  It should fail at test fixture setup with something like

```
OneTimeSetUp: System.TypeInitializationException : The type initializer for 'Tests.Common.DatabaseTests' threw an exception.
      ----> System.IO.FileNotFoundException : Could not find file 'Z:\Repos\SmiServices\MyPipelinePluginTests\bin\x64\Debug\net6.0\TestDatabases.txt'

```

Add a new file to your project called TestDatabases.txt and set it to `Copy if newer`

```
ServerName: localhost\sqlexpress
Prefix: TEST_
```

Note if you do not have a test instance of SqlServer you can set this to `(localdb)\MSSQLLocalDB` which is [Visual Studios internal automatic test instance](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver15).

Now running the test should result in:

```
Message: OneTimeSetUp:   Catalogue database does not exist, run 'rdmp.exe install' to create it (Ensure that servername and prefix in TestDatabases.txt match those you provide to 'rdmp.exe install' e.g. 'rdmp.exe install localhost\sqlexpress TEST_')
```

Create these databases you can use the main RDMP UI:

![Create platform database in rdmp main ui](Images/CreatePlatformDatabases.png)

Clean and Rebuild your project and run the unit test again. It should pass this time.

<a name="databaseTestsWriting"></a>
## Writing a Database Test
Add a new test 

```csharp
using FAnsi;
using FAnsi.Discovery;
using MyPipelinePlugin;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Progress;
using System.Data;
using Tests.Common;

namespace MyPipelinePluginTests
{
    class TestAnonymisationPluginsDatabaseTests: DatabaseTests
    {
        [Test]
        public void Test()
        {
            Assert.Pass();
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void TestBasicDataTableAnonymiser3(DatabaseType type)
        {
            DiscoveredDatabase database = GetCleanedServer(type);

            //Create a names table that will go into the database
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Rows.Add(new[] {"Thomas"});
            dt.Rows.Add(new[] {"Wallace"});
            dt.Rows.Add(new[] {"Frank"});

            DiscoveredTable table = database.CreateTable("ForbiddenNames",dt);
            
            Import(table,out ITableInfo tableInfo,out _);

            //Create the test dataset chunk that will be anonymised
            var dtStories = new DataTable();
            dtStories.Columns.Add("Story");
            dtStories.Rows.Add(new[] { "Thomas went to school regularly" });
            dtStories.Rows.Add(new[] { "It seems like Wallace went less regularly" });
            dtStories.Rows.Add(new[] { "Mr Smitty was the teacher" });

            //Create the anonymiser
            var a = new BasicDataTableAnonymiser3();

            //Tell it about the database table
            a.NamesTable = (TableInfo)tableInfo;

            //run the anonymisation
            var resultTable = a.ProcessPipelineData(dtStories, new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());

            //check the results
            Assert.AreEqual(resultTable.Rows.Count, 3);
            Assert.AreEqual("REDACTED went to school regularly", resultTable.Rows[0][0]);
            Assert.AreEqual("It seems like REDACTED went less regularly", resultTable.Rows[1][0]);
            Assert.AreEqual("Mr Smitty was the teacher", resultTable.Rows[2][0]);

            //finally drop the database table
            table.Drop();
        }
    }
}
```

This has a few intersting lines in it.  Firstly we create a DataTable containing a Names column with some values then we use the base class property `GetCleanedServer` this is a database for creating test tables in.  The database is nuked before each test set is run.  `DiscoveredDatabase.CreateTable` will upload the `DataTable` to the destination and return a `DiscoveredTable`.

`Discovered[...]` is how we reference Servers / Databases / Tables / Columns as we find them at runtime.  These classes exist to provide simplified access to common tasks in a cross platform way.

Once we have a `DiscoveredTable` we can create a persistent reference to it in the TEST_Catalogue database ([TableInfo]) via base method `Import` (this is a helper method that wraps `TableInfoImporter`).  The [TableInfo] pointer is given to the `BasicDataTableAnonymiser3` and used to anonymise the 'pipeline chunk' `dtStories` (like in the first unit test).

If you have access to an oracle / mysql testing database you can add the other test cases by adding the connection strings to TestDatabases.txt:

```
ServerName: localhost\sqlexpress
Prefix: TEST_
MySql: Server=localhost;Uid=root;Pwd=zombie;SSLMode=None
Oracle: Data Source=localhost:1521/orclpdb.dundee.uni;User Id=ora;Password=zombie;
``` 

Add the following 2 test cases to the test
```
[TestCase(DatabaseType.Oracle)]
[TestCase(DatabaseType.MySql)]
```

This will result in the names table being created/read on the other DMBS provider databases.

<a name="checks"></a>
# Checks
RDMP tries to make sure all components are configured correctly before executing, this is done through the `ICheckNotifier` / `ICheckable` system.

<a name="anoPluginVersion4"></a>
## Version 4
Create an exact copy of `BasicDataTableAnonymiser3` called `BasicDataTableAnonymiser4`.  Move the initialization code for `_commonNames` into a method GetCommonNamesTable.

Next go into the empty `Check` method in your class (`BasicDataTableAnonymiser4`) and call the new method `GetCommonNamesTable`

```csharp
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

namespace MyPipelinePlugin
{
    public class BasicDataTableAnonymiser4: IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("Table containing a single column which must have a list of names to redact from columns", mandatory:true)]
        public TableInfo NamesTable { get; set; }

        [DemandsInitialization("Columns matching this regex pattern will be skipped")]
        public Regex ColumnsNotToEvaluate { get; set; }

        private string[] _commonNames;
        
        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,GracefulCancellationToken cancellationToken)
        {
            GetCommonNamesTable();

            //Go through each row in the table
            foreach (DataRow row in toProcess.Rows)
            {
                //for each cell in current row
                foreach (DataColumn col in toProcess.Columns)
                {
                    //if it's not a column we are skipping
                    if(ColumnsNotToEvaluate != null && ColumnsNotToEvaluate.IsMatch(col.ColumnName))
                        continue;
                    
                    //if it is a string
                    var stringValue = row[col] as string;

                    if(stringValue != null)
                    {
                        //replace any common names with REDACTED
                        foreach (var name in _commonNames)
                            stringValue =  Regex.Replace(stringValue, name, "REDACTED",RegexOptions.IgnoreCase);

                        row[col] = stringValue;
                    }
                }
            }

            return toProcess;
        }
        private void GetCommonNamesTable()
        {
            if (_commonNames == null)
            {
                //get access to the database under DataLoad context
                var databaseDiscovered = DataAccessPortal.GetInstance().ExpectDatabase(NamesTable, DataAccessContext.DataLoad);

                //expect a table matching the TableInfo
                var tableDiscovered = databaseDiscovered.ExpectTable(NamesTable.GetRuntimeName());

                //make sure it exists
                if (!tableDiscovered.Exists())
                    throw new NotSupportedException("TableInfo '" + tableDiscovered + "' does not exist!");

                //Download all the data
                var dataTable = tableDiscovered.GetDataTable();

                //Make sure it has the correct expected schema (i.e. 1 column)
                if (dataTable.Columns.Count != 1)
                    throw new NotSupportedException("Expected a single column in DataTable '" + tableDiscovered + "'");

                //turn it into an array
                _commonNames = dataTable.Rows.Cast<DataRow>().Select(r => r[0] as string).Where(s=>!string.IsNullOrWhiteSpace(s)).ToArray();
            }
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            GetCommonNamesTable();
        }
    }
}
```

Go to your unit tests and write a test for it passing it a `ThrowImmediatelyCheckNotifier` (this is just like `ThrowImmediatelyDataLoadEventListener` in that it will treat all Fail messages - and optionally Warnings too as Exceptions and throw them).

```csharp
[Test]
public void TestBasicDataTableAnonymiser4_FailConditions()
{
    var a = new BasicDataTableAnonymiser4();
    a.Check(ThrowImmediatelyCheckNotifier.Quiet());
}
```

Running this test should give an error like 

```
Message: System.NullReferenceException : Object reference not set to an instance of an object.
```

This is not very helpful.  We can use the `ReusableLibraryCode.Checks.ICheckNotifier` argument of `Check` to record the checking process.  This will look something like:
```csharp
notifier.OnCheckPerformed(new CheckEventArgs("Ready to start checking", CheckResult.Success, null, null));
```

The two null arguments are for Exception (if any) and the 'proposed fix' which is a string that describes how you can immediately fix the problem in a way where you want to delegate the descision (i.e. you don't want to automatically always fix it').  The return value of OnCheckPerformed is `bool` this indicates whether the fix should be attempted.  Most `ICheckNotifier` implementations provide static answers to fixes e.g. `ReusableLibraryCode.Checks.AcceptAllCheckNotifier` will return true but throw an Exception any time there is a check that fails without a ProposedFix.  Some `ICheckNotifier` will consult the user about whether to apply a ProposedFix e.g. `ChecksUI`
  
For now we can ignore ProposedFix because nothing that goes wrong with our component can be easily fixed.

Start by passing the `notifier` argument into `GetCommonNamesTable` and use it to document the setup process (and any failures).  You can pass a `ThrowImmediatelyCheckNotifier` when calling it from `ProcessPipelineData`

```csharp
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

namespace MyPipelinePlugin
{
    public class BasicDataTableAnonymiser4: IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("Table containing a single column which must have a list of names to redact from columns", mandatory:true)]
        public TableInfo NamesTable { get; set; }

        [DemandsInitialization("Columns matching this regex pattern will be skipped")]
        public Regex ColumnsNotToEvaluate { get; set; }

        private string[] _commonNames;
        
        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,GracefulCancellationToken cancellationToken)
        {
            GetCommonNamesTable(ThrowImmediatelyCheckNotifier.Quiet());

            //Go through each row in the table
            foreach (DataRow row in toProcess.Rows)
            {
                //for each cell in current row
                foreach (DataColumn col in toProcess.Columns)
                {
                    //if it's not a column we are skipping
                    if(ColumnsNotToEvaluate != null && ColumnsNotToEvaluate.IsMatch(col.ColumnName))
                        continue;
                    
                    //if it is a string
                    var stringValue = row[col] as string;

                    if(stringValue != null)
                    {
                        //replace any common names with REDACTED
                        foreach (var name in _commonNames)
                            stringValue =  Regex.Replace(stringValue, name, "REDACTED",RegexOptions.IgnoreCase);

                        row[col] = stringValue;
                    }
                }
            }

            return toProcess;
        }
        private void GetCommonNamesTable(ICheckNotifier notifier)
        {
            if (_commonNames == null)
            {
                if (NamesTable == null)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "No NamesTable has been set, this must be a Table containing a list of names to REDACT from the pipeline data being processed",
                            CheckResult.Fail));

                    return;
                }

                //get access to the database under DataLoad context
                var databaseDiscovered = DataAccessPortal.GetInstance().ExpectDatabase(NamesTable, DataAccessContext.DataLoad);


                
                //expect a table matching the TableInfo
                var tableDiscovered = databaseDiscovered.ExpectTable(NamesTable.GetRuntimeName());

                //make sure it exists
                if (!tableDiscovered.Exists())
                    throw new NotSupportedException("TableInfo '" + tableDiscovered + "' does not exist!");

                //Download all the data
                var dataTable = tableDiscovered.GetDataTable();

                //Make sure it has the correct expected schema (i.e. 1 column)
                if (dataTable.Columns.Count != 1)
                    throw new NotSupportedException("Expected a single column in DataTable '" + tableDiscovered + "'");

                //turn it into an array
                _commonNames = dataTable.Rows.Cast<DataRow>().Select(r => r[0] as string).Where(s=>!string.IsNullOrWhiteSpace(s)).ToArray();
            }
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Ready to start checking", CheckResult.Success, null, null));

            GetCommonNamesTable(notifier);
        }
    }
}
```

Now we can run our test and see an error that makes sense

```
[Test]
public void TestBasicDataTableAnonymiser4_FailConditions()
{
    var a = new BasicDataTableAnonymiser4();
    var ex = Assert.Throws<Exception>(()=>a.Check(ThrowImmediatelyCheckNotifier.Quiet()));
    Assert.IsTrue(ex.Message.Contains("No NamesTable has been set"));
}
```

Now when you run RDMP and add this component to the `BULK INSERT:CSV Import File` without specifying a [TableInfo] it should look something like:

![Add empty Catalogue](Images/ChecksFailure.png)

Finally we can add in some other sensible checks

```csharp
private void GetCommonNamesTable(ICheckNotifier notifier)
{
    if (_commonNames == null)
    {
        if (NamesTable == null)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "No NamesTable has been set, this must be a Table containing a list of names to REDACT from the pipeline data being processed",
                    CheckResult.Fail));

            return;
        }

        //get access to the database under DataLoad context
        var databaseDiscovered = DataAccessPortal.GetInstance().ExpectDatabase(NamesTable, DataAccessContext.DataLoad);

        if (databaseDiscovered.Exists())
            notifier.OnCheckPerformed(new CheckEventArgs("Found Database '" + databaseDiscovered + "' ",CheckResult.Success));
        else
            notifier.OnCheckPerformed(new CheckEventArgs("Database '" + databaseDiscovered + "' does not exist ", CheckResult.Fail));

        //expect a table matching the TableInfo
        var tableDiscovered = databaseDiscovered.ExpectTable(NamesTable.GetRuntimeName());

        if (tableDiscovered.Exists())
            notifier.OnCheckPerformed(new CheckEventArgs("Found table '" + tableDiscovered + "' ", CheckResult.Success));
        else
            notifier.OnCheckPerformed(new CheckEventArgs("Table '" + tableDiscovered + "' does not exist ", CheckResult.Fail));

        //make sure it exists
        if (!tableDiscovered.Exists())
            throw new NotSupportedException("TableInfo '" + tableDiscovered + "' does not exist!");

        //Download all the data
        var dataTable = tableDiscovered.GetDataTable();

        //Make sure it has the correct expected schema (i.e. 1 column)
        if (dataTable.Columns.Count != 1)
            throw new NotSupportedException("Expected a single column in DataTable '" + tableDiscovered + "'");

        //turn it into an array
        _commonNames = dataTable.Rows.Cast<DataRow>().Select(r => r[0] as string).Where(s=>!string.IsNullOrWhiteSpace(s)).ToArray();

        if (_commonNames.Length == 0)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Table '" + tableDiscovered + "' did not have any rows in it!", CheckResult.Fail));
                    
            //reset it just in case
            _commonNames = null;
        }
        else
            notifier.OnCheckPerformed(new CheckEventArgs("Read " + _commonNames.Length + " names from name table", CheckResult.Success));
    }
}
```

<a name="progress"></a>
# Progress Logging
Now that we are familiar with `ReusableLibraryCode.Checks.ICheckNotifier` it is time to get to grips with the other event system in RDMP `ReusableLibraryCode.Progress.IDataLoadEventListener`.  While `ICheckNotifier` is intended to run primarily at Design time (when the user is configuring his pipelines) and can propose fixes, `IDataLoadEventListener` is the opposite.  

`IDataLoadEventListener` is intended for use at execution time and supports both `ReusableLibraryCode.Progress.ProgressEventArgs` (incremental messages about how many records have been processed in what time period) as well as one off messages (`ReusableLibraryCode.Progress.NotifyEventArgs`).

<a name="anoPluginVersion5"></a>
## Version 5
Create a copy of `BasicDataTableAnonymiser4` called `BasicDataTableAnonymiser5`.  Add an Information message into `ProcessPipelineData` recording the fact that you are processing a new batch:

```csharp
listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Ready to process batch with row count " + toProcess.Rows.Count));
```

Declare a StopWatch and a counter variable at class level
```csharp
private int _redactionsMade = 0;
private Stopwatch _timeProcessing = new Stopwatch();
```

This will let us record how long is specifically spent on the anonymisation of the DataTable (bearing in mind we are only one component in a long pipeline which might be slow).  Start the StopWatch before the `foreach` statement and stop it afterwards.  Send a Progress message at the end of the `foreach` statement too.  The new code for `ProcessPipelineData` should look like:

```csharp
public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
{
    GetCommonNamesTable(ThrowImmediatelyCheckNotifier.Quiet());

    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Ready to process batch with row count " + toProcess.Rows.Count));

    _timeProcessing.Start();

    //Go through each row in the table
    foreach (DataRow row in toProcess.Rows)
    {
        //for each cell in current row
        foreach (DataColumn col in toProcess.Columns)
        {
            //if it's not a column we are skipping
            if (ColumnsNotToEvaluate != null && ColumnsNotToEvaluate.IsMatch(col.ColumnName))
                continue;

            //if it is a string
            var stringValue = row[col] as string;

            if (stringValue != null)
            {
                //replace any common names with REDACTED
                foreach (var name in _commonNames)
                    stringValue = Regex.Replace(stringValue, name, "REDACTED", RegexOptions.IgnoreCase);

                //if string value changed
                if (!row[col].Equals(stringValue))
                {
                    //increment the counter of redactions made
                    _redactionsMade++;

                    //update the cell to the new value
                    row[col] = stringValue;
                }
            }
        }
    }

    _timeProcessing.Stop();
    listener.OnProgress(this, new ProgressEventArgs("REDACTING Names",new ProgressMeasurement(_redactionsMade,ProgressType.Records),_timeProcessing.Elapsed));

    return toProcess;
}
```

Notice that we are only counting the number of redactions since that is the most interesting bit.  Now it is time to test it and see the power of the `IDataLoadEventListener` system.  We are going to write a test which explores three different `IDataLoadEventListener` implementations.

Add the following to `TestAnonymisationPluginsDatabaseTests`

```csharp
public enum LoggerTestCase
{
    ToConsole,
    ToMemory,
    ToDatabase
}

[Test]
[TestCase(LoggerTestCase.ToConsole)]
[TestCase(LoggerTestCase.ToMemory)]
[TestCase(LoggerTestCase.ToDatabase)]
public void TestBasicDataTableAnonymiser5(LoggerTestCase testCase)
{
    //Create a names table that will go into the database
    var dt = new DataTable();
    dt.Columns.Add("Name");
    dt.Rows.Add(new[] { "Thomas" });
    dt.Rows.Add(new[] { "Wallace" });
    dt.Rows.Add(new[] { "Frank" });

    //upload the DataTable from memory into the database
    var discoveredTable = GetCleanedServer(DatabaseType.MicrosoftSQLServer).CreateTable("ForbiddenNames", dt);
    try
    {
        TableInfo tableInfo;

        //import the persistent TableInfo reference
        var importer = Import(discoveredTable,out tableInfo ,out _);
                
        //Create the test dataset chunks that will be anonymised
        var dtStories1 = new DataTable();
        dtStories1.Columns.Add("Story");
        dtStories1.Rows.Add(new[] { "Thomas went to school regularly" }); //1st redact
        dtStories1.Rows.Add(new[] { "It seems like Wallace went less regularly" }); //2nd redact
        dtStories1.Rows.Add(new[] { "Mr Smitty was the teacher" });

        var dtStories2 = new DataTable();
        dtStories2.Columns.Add("Story");
        dtStories2.Rows.Add(new[] { "Things were going so well" });
        dtStories2.Rows.Add(new[] { "And then it all turned bad for Wallace" }); //3rd redact
    
        var dtStories3 = new DataTable();
        dtStories3.Columns.Add("Story");
        dtStories3.Rows.Add(new[] { "There were things creeping in the dark" });
        dtStories3.Rows.Add(new[] { "Surely Frank would know what to do.  Frank was a genius" }); //4th redact
        dtStories3.Rows.Add(new[] { "Mr Smitty was the teacher" });
    
        //Create the anonymiser
        var a = new BasicDataTableAnonymiser5();

        //Tell it about the database table
        a.NamesTable = tableInfo;

        //Create a listener according to the test case
        IDataLoadEventListener listener = null;

        switch (testCase)
        {
            case LoggerTestCase.ToConsole:
                listener = new ThrowImmediatelyDataLoadEventListener();
                break;
            case LoggerTestCase.ToMemory:
                listener = new ToMemoryDataLoadEventListener(true);
                break;
            case LoggerTestCase.ToDatabase:
            
                //get the default logging server
                var logManager = CatalogueRepository.GetDefaultLogManager();

                //create a new super task Anonymising Data Tables
                logManager.CreateNewLoggingTaskIfNotExists("Anonymising Data Tables");

                //setup a listener that goes to this logging database 
                listener = new ToLoggingDatabaseDataLoadEventListener(this,logManager ,"Anonymising Data Tables","Run on " + DateTime.Now);
                break;
            default:
                throw new ArgumentOutOfRangeException("testCase");
        }

        //run the anonymisation
        //process all 3 batches
        a.ProcessPipelineData(dtStories1, listener, new GracefulCancellationToken());
        a.ProcessPipelineData(dtStories2, listener, new GracefulCancellationToken());
        a.ProcessPipelineData(dtStories3, listener, new GracefulCancellationToken());

        //check the results
        switch (testCase)
        {
            case LoggerTestCase.ToMemory:
                Assert.AreEqual(4, ((ToMemoryDataLoadEventListener)listener).LastProgressRecieivedByTaskName["REDACTING Names"].Progress.Value);
                break;
            case LoggerTestCase.ToDatabase:
                ((ToLoggingDatabaseDataLoadEventListener)listener).FinalizeTableLoadInfos();
                break;
        }
    }
    finally
    {
        //finally drop the database table
        discoveredTable.Drop();
    }
}
```

This test has the same setup of the ForbiddenNames table, this time we create 3 batches which will go through our pipeline component in sequence (as would happen in normal execution where you could be processing millions of records in sub batches).  It then creates one of three `IDataLoadEventListener` and passes the 3 batches in.

The first test case `LoggerTestCase.ToConsole` creates a `ThrowImmediatelyDataLoadEventListener`.  This ignores `OnProgress` messages, writes out `OnNotify` to the console and throws an Exception if there are any Error messages received.

![To Console Output](Images/Version5ToConsoleOutput.png)

The second test case `LoggerTestCase.ToMemory` creates a `ReusableLibraryCode.Progress.ToMemoryDataLoadEventListener`.  `ToMemoryDataLoadEventListener` records `OnProgress` and `OnNotify` messages in Dictionaries indexed by component (that sent the message).  We need a Dictionary because in practice there will usually be multiple components executing and all logging to the same `IDataLoadEventListener`.  This class is particularly useful for testing where you want to confirm that a certain message was sent or that a certain number of records was processed.  `ToMemoryDataLoadEventListener` can also be used when you want to run an entire Pipeline and make descisions based on the logging messages generated (`ProgressEventType GetWorst()` method can be helpful here).

We use the `ToMemoryDataLoadEventListener` to confirm that the final progress count of redactions as logged by the component are 4.

```csharp
Assert.AreEqual(4,
 ((ToMemoryDataLoadEventListener)listener).LastProgressRecieivedByTaskName["REDACTING Names"].Progress.Value);
```

Finally we have the test case `LoggerTestCase.ToDatabase` which creates a `ToLoggingDatabaseDataLoadEventListener`.  This is class writes to the RDMP relational logging database which RDMP uses to record all the ongoing activities executed by users.  A test instance of this database is automatically setup by `rdmp.exe install` and is therefore available any class inheriting from `DatabaseTests`.  If you look at your test server in Sql Management Studio you should see a database called `TEST_Logging`.  This database has a hierarchy 

TableName|Purpose
--------|---------
DataLoadTask | The overarching task which occurs regularly e.g. DataExtraction
DataLoadRun | An instance of the overarching task being attempted/executed e.g. `Extracting 'Cases' for 'Project 32'
ProgressLog | All the messages generated during a given `DataLoadRun`
FatalError | All Error messages generated during a given `DataLoadRun` with a flag for whether they have been resolved or not
TableLoadRun | A count of the number of records that ended up at a given destination (this might be a database table but could equally be a flat file etc)
DataSource | A description of all the contributors of data to the `TableLoadRun` (this could be flat files or a block of SQL run on a server or even just a class name!)

After running this test case you can open the TEST_Logging database in Sql Management Studio.  Unlike TEST_Catalogue, The TEST_Logging database is not automatically cleared cleared after each test so you might have some additional runs (if you ran the test multiple times or had some bugs implementing it) but it should look something like:

![Logging database should look like this](Images/Version5LoggingDatabase.png)

One final thing to note is the call to `FinalizeTableLoadInfos`.  Since we might pass the `ToLoggingDatabaseDataLoadEventListener` to multiple components and even possibly multiple pipeline executions (or pipelines within pipelines!) it is not easy to automatically define an end point after which the `DataLoadRun` / `TableLoadRun` should be closed off and marked complete.  Therefore `ToLoggingDatabaseDataLoadEventListener` requires you to call this at some point once you are sure all the things you wanted to log in the run are complete and all relevant components have Disposed etc.

<a name="NLog"></a>
## What is wrong with NLog etc?
Nothing is stopping you creating your own class logger (e.g. NLog, Log4Net etc).  If you want to send events reported by a `IDataLoadEventListener` to your existing log you can use `NLogIDataLoadEventListener` or `NLogICheckNotifier`.  If you are using Log4Net or another logging package you can follow the pattern and create your own implementation of `IDataLoadEventListener`.

<a name="funkyIDataLoadEventListener"></a>
## What other funky things can I do with IDataLoadEventListener?
Well you can route messages to two different locations at once:

```csharp
IDataLoadEventListener toUserInterface = new ProgressUI();
IDataLoadEventListener toDatabase = new ToLoggingDatabaseDataLoadEventListener(this, logManager, "Anonymising Data Tables", "Run on " + DateTime.Now);
IDataLoadEventListener forkListener = new ForkDataLoadEventListener(toUserInterface, toDatabase);
```

You can also convert between an `IDataLoadEventListener` and an `ICheckNotifier` 
```csharp
IDataLoadEventListener listener = new ThrowImmediatelyDataLoadEventListener();
ICheckNotifier checker = new FromDataLoadEventListenerToCheckNotifier(listener);
```		

And even back again
```csharp
IDataLoadEventListener listener = new ThrowImmediatelyDataLoadEventListener();
ICheckNotifier checker = new FromDataLoadEventListenerToCheckNotifier(listener);
IDataLoadEventListener listener2 = new FromCheckNotifierToDataLoadEventListener(checker);	
```

Keep in mind the differences though: 
Going from `IDataLoadEventListener` to `ICheckNotifier` will result in rejecting any ProposedFix automatically
Going from `ICheckNotifier` to `IDataLoadEventListener` will result in a listener which basically ignores OnProgress counts

<a name="guis"></a>
# Graphical User Interfaces In Plugins

All the plugins described in this tutorial have been written to work without any explicit gui elements (e.g. forms).  This enables them to work in automated workflows (i.e. from the RDMP command line).

RDMP does support graphical plugins for the RDMP windows client application.  These must be packaged into the `libs\windows` subdirectory of the plugins `.nupkg`.

```
dotnet new winformslib -n MyPluginUI -o ./MyPluginUI
cd ./MyPluginUI
dotnet add package HIC.RDMP.Plugin.UI
```

Add a new user control using the Visual Studio Forms Designer

![A simple user control in Visual Studio Forms Designer](Images/UserControl1.png)

Add an implementation of `IPluginUserInterface` that detects when the windows client is being used and shows this alternative UI for editing `Catalogue` objects.

```csharp
using MapsDirectlyToDatabaseTable;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;

namespace MyPluginUI
{
    public class MyPluginUIExample: PluginUserInterface
    {
        public MyPluginUIExample(IBasicActivateItems activator):base(activator)
        {

        }

        public override bool CustomActivate(IMapsDirectlyToDatabaseTable o)
        {
            // if this is the windows client
            if(BasicActivator is IActivateItems a)
            {
                if(o is Catalogue c)
                {
                    var control = new UserControl1(c);
                    a.ShowWindow(control,true);
                    return true;
                }
            }

            return base.CustomActivate(o);
        }
    }
}
```

Update the `UserControl1` constructor to take a `Catalogue`

```csharp
public UserControl1(Rdmp.Core.Curation.Data.Catalogue c)
{
    InitializeComponent();
    textBox1.Text = c.Description;
}
```

Build to the RDMP bin directory

```
dotnet build -o Z:\rdmp-client\
cd Z:\rdmp-client\
./ResearchDataManagementPlatform.exe
```

Now double clicking a Catalogue should launch your custom user interface.

![UserControl1 running in RDMP showing data from the Catalogue](Images/ExamplePluginGui.png)

If you want the control to use the same look and feel as the rest of RDMP then change `UserControl` to inherit from `RDMPSingleDatabaseObjectControl<T>`:

```csharp
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System.Windows.Forms;

namespace MyPluginUI
{
    public partial class UserControl1 : RDMPSingleDatabaseObjectControl<Catalogue>
    {
        public UserControl1()
        {
            InitializeComponent();
        }
        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            // make sure to do this first
            base.SetDatabaseObject(activator, databaseObject);

            // now bind the text box to Catalogue Description field
            Bind(textBox1, "Text", "Description", c => c.Description);

            // add Save button
            var s = GetObjectSaverButton();
            s.SetupFor(this, databaseObject, activator);
        }
    }
}
```

Update how you call your control:

```csharp
// if this is the windows client
if(BasicActivator is IActivateItems a)
{
    if(o is Catalogue c)
    {
        var control = new UserControl1();
        a.ShowWindow(control,true);
        control.SetDatabaseObject(a, c);
        return true;
    }
}
```

![UserControl1 running in RDMP showing data from the Catalogue with consistent look and feel](Images/SameLookAndFeel.png)

To package your plugin as a nupkg that is usable by others.  Update `MyPlugin.nuspec` include both `MyPluginUI` and `MyPlugin` binaries in the appropriate subdirectory:

```
    <files>
    <file src="MyPlugin\bin\net6.0\*" target="lib\main" />
    <file src="MyPluginUI\bin\net6.0-windows\*" target="lib\windows" />
    </files>
```

Delete all `MyPlugin...` files that you built into the RDMP directory

Package the plugin with nuget:

```
nuget pack ./MyPlugin.nuspec
```

<a name="troubleshooting"></a>
# Troubleshooting Plugins

If you do not see code changes taking effect or are unable to see an expected plugin module etc that you have written then the following may help:

## If building into the RDMP bin directory

If you are building your plugin directly into the RDMP bin directory.  First make sure that you have not packaged and uploaded it into the RDMP database.  

Next check that the Modified timestamp on `MyPlugin.dll` in the RDMP directory matches when you built your project.

## If packaging and uploading your plugin

Make sure that you have either bumped the version of your `nupkg` or deleted stale versions of your plugin.

This is done by deleting the plugin from the RDMP client and then deleting the contents of `%appdata%/MEF` e.g. `C:\Users\thomas\AppData\Roaming\MEF`.  **This directory will be locked when RDMP is running so you will need to first close RDMP**


<a name="dependencies"></a>
# Dependencies

If your plugin has many dependencies that are not already included in RDMP then you will need to ensure the dlls also appear in the relevant lib directory of the plugin.

For example instead of using `dotnet build` and packaging `bin\net6.0\*` in your `MyPlugin.nuspec` you should use `dotnet publish`

```
dotnet publish --runtime win-x64 -c Release --self-contained false
```

When packaging `dotnet publish` results you can exclude dlls that already come with RDMP by using the `exclude` keyword in `MyPlugin.nuspec`:

```
<files>
    <file src="Plugin\windows\bin\$configuration$\net6.0-windows\win-x64\publish\*"
          exclude="**\SynthEHR.Core.dll;**\FAnsi.*;**\MapsDirectlyToDatabaseTable.dll;**\MySql.Data.dll;**\Oracle.ManagedDataAccess.dll;**\Rdmp.Core.dll;**\NPOI.*;**\Renci.*;**\MathNet.Numerics.dll*;**\Rdmp.UI.dll;**\ScintillaNET.dll;**\ReusableUIComponents.dll;**\ObjectListView.dll;**\WeifenLuo.WinFormsUI.Docking*"
          target="lib\windows" />
</files>
```

## Other Steps you can take

You can see all the currently loaded class Types by selecting `Diagnostics->Plugins->List All Types` to view all the loaded Types.

During startup you can click the status icon to see what plugins are loaded and if any had issues (bear in mind not all errors/warnings will be about your plugin or relevant):

![Messages logged by RDMP during startup](Images/StartupLog.png)

[Catalogue]: ./Glossary.md#Catalogue
[TableInfo]: ./Glossary.md#TableInfo

[Project]: ./Glossary.md#Project

[Pipeline]: ./Glossary.md#Pipeline
