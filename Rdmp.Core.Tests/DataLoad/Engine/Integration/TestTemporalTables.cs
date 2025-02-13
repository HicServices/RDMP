// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.IO;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Checks;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

internal class TestTemporalTables : DataLoadEngineTestsBase
{
    private string sql = @"CREATE TABLE dbo.Employee
(
  [EmployeeID] int NOT NULL PRIMARY KEY CLUSTERED
  , [Name] nvarchar(100) NOT NULL
  , [Position] varchar(100) NOT NULL
  , [Department] varchar(100) NOT NULL
  , [Address] nvarchar(1024) NOT NULL
  , [AnnualSalary] decimal (10,2) NOT NULL
  , [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START
  , [ValidTo] datetime2 GENERATED ALWAYS AS ROW END
  , PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
 )
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.EmployeeHistory));

INSERT INTO Employee(EmployeeID,Name,Position,Department,Address,AnnualSalary) VALUES(1,'Frank','Security Guard','Arkham', '22 Innsmouth Way', 55000.5)
";

    [TestCase(true)]
    [TestCase(false)]
    public void TestTemporalTable(bool ignoreWithGlobalPattern)
    {
        const DatabaseType dbtype = FAnsi.DatabaseType.MicrosoftSQLServer;
        var db = GetCleanedServer(dbtype);

        using (var con = db.Server.GetConnection())
        {
            con.Open();
            db.Server.GetCommand(sql, con).ExecuteNonQuery();
        }

        var tbl = db.ExpectTable("Employee");

        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad")
        {
            IgnoreTrigger = true
        };
        lmd.SaveToDatabase();

        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");

        //create a text file to load where we update Frank's favourite colour (it's a pk field) and we insert a new record (MrMurder)
        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @"EmployeeID,Name,Position,Department,Address,AnnualSalary
1,Frank,Boss,Department of F'Tang, 22 Innsmouth Way, 55000.5
2,Herbert,Super Boss,Department of F'Tang, 22 Innsmouth Way, 155000.5");


        //the checks will probably need to be run as ddl admin because it involves creating _Archive table and trigger the first time

        //clean SetUp RAW / STAGING etc and generally accept proposed cleanup operations
        var checker =
            new CheckEntireDataLoadProcess(null,lmd, new HICDatabaseConfiguration(lmd), new HICLoadConfigurationFlags());
        checker.Check(new AcceptAllCheckNotifier());

        if (ignoreWithGlobalPattern)
        {
            var regex = new StandardRegex(RepositoryLocator.CatalogueRepository)
            {
                ConceptName = StandardRegex.DataLoadEngineGlobalIgnorePattern,
                Regex = "^Valid((From)|(To))$"
            };

            regex.SaveToDatabase();
        }
        else
        {
            var col = ti.ColumnInfos.Single(c => c.GetRuntimeName().Equals("ValidFrom"));
            col.IgnoreInLoads = true;
            col.SaveToDatabase();

            col = ti.ColumnInfos.Single(c => c.GetRuntimeName().Equals("ValidTo"));
            col.IgnoreInLoads = true;
            col.SaveToDatabase();
        }

        var dbConfig = new HICDatabaseConfiguration(lmd, null);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

        var exitCode = exe.Run(
            new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig),
            new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));

            //frank should be updated to his new departement and role
            Assert.That(tbl.GetRowCount(), Is.EqualTo(2));
        });
        var result = tbl.GetDataTable();
        var frank = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "Frank");
        Assert.Multiple(() =>
        {
            Assert.That(frank["Department"], Is.EqualTo("Department of F'Tang"));
            Assert.That(frank["Position"], Is.EqualTo("Boss"));
        });

        //post test cleanup
        foreach (var regex in RepositoryLocator.CatalogueRepository.GetAllObjects<StandardRegex>())
            regex.DeleteInDatabase();
    }
}