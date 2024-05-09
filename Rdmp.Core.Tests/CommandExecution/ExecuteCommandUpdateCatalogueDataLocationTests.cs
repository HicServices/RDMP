// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandUpdateCatalogueDataLocationTests : DatabaseTests
{
    protected DiscoveredDatabase RemoteDatabase { get; set; }
    protected DiscoveredTable RemoteTable { get; set; }
    protected string RemoteDatabaseName = TestDatabaseNames.GetConsistentName("rdb");

    private DiscoveredDatabase db;

    private int goodCatalogueID;
    private int originalTableInfoID;

    private void CreateDatabase()
    {
        RemoteDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(RemoteDatabaseName);
        RemoteDatabase.Create(true);
        var dt = new DataTable();
        dt.Columns.Add("Column1", typeof(string));
        dt.Columns.Add("Column2", typeof(int));

        var dr = dt.NewRow();
        dr["Column1"] = "d";
        dr["Column2"] = 100;
        dt.Rows.Add(dr);
        RemoteTable = RemoteDatabase.CreateTable("SomeTable", dt);
        dt.Dispose();
    }

    private void CreateCatalogue()
    {
        var fileName = Path.GetTempPath() + Guid.NewGuid() + ".csv";
        using (var outputFile = new StreamWriter(fileName, true))
        {
            outputFile.WriteLine("Column1,Column2");
            outputFile.WriteLine("b,1");
            outputFile.WriteLine("c,2");
        }

        var info = new FileInfo(fileName);
        db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var creator = new CataloguePipelinesAndReferencesCreation(
            RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);
        if (CatalogueRepository.GetAllObjects<Pipeline>().Length == 0)
            creator.CreatePipelines(new PlatformDatabaseCreationOptions());

        var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
            .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
        var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(new ThrowImmediatelyActivator(RepositoryLocator),
            info, "Column1", db, pipe, null);
        cmd.Execute();
        originalTableInfoID = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>().First().ColumnInfo
            .TableInfo_ID;
        var column1 = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column1").First();
        column1.ExtractionInformation.SelectSQL = column1.ExtractionInformation.SelectSQL + " as SOME_ALIAS";
        column1.ExtractionInformation.SaveToDatabase();
    }

    private void CreateSecondaryCatalogue()
    {
        var fileName = Path.GetTempPath() + Guid.NewGuid() + ".csv";
        using (var outputFile = new StreamWriter(fileName, true))
        {
            outputFile.WriteLine("Column,Other");
            outputFile.WriteLine("b,1");
        }

        var info = new FileInfo(fileName);

        // find the excel loading pipeline
        var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
            .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
        var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(new ThrowImmediatelyActivator(RepositoryLocator),
            info, "Other", db, pipe, null);
        cmd.Execute();
    }

    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();
        CreateDatabase();
        CreateCatalogue();
        CreateSecondaryCatalogue();
    }


    [Test]
    public void UpdateLocationCheckNoItems()
    {
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            new List<CatalogueItem>().ToArray(), RemoteTable, null);
        Assert.That(cmd.Check(), Is.EqualTo("Must select at least one catalogue item to modify"));
    }

    [Test]
    public void UpdateLocationCheckBadMapping()
    {
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>(), RemoteTable, "badMaping");
        Assert.That(cmd.Check(), Is.EqualTo("Column Mapping must contain the string '$column'"));
    }

    [Test]
    public void UpdateLocationColumnNotExist()
    {
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>().Where(c => c.Name == "Column")
                .ToArray(), RemoteTable, "$column");
        Assert.That(cmd.Check(), Is.EqualTo("Unable to find column '[Column]' in selected table"));
    }

    [Test]
    public void UpdateLocationColumnBadType()
    {
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>().Where(c => c.Name == "Column")
                .ToArray(), RemoteTable, "$column2");
        Assert.That(cmd.Check(),
            Is.EqualTo(
                "The data type of column '[Column2]' is of type 'int'. This does not match the current type of 'varchar(1)'"));
    }

    [Test]
    public void UpdateLocationCheckOK()
    {
        goodCatalogueID = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column2").First().Catalogue_ID;
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
                .Where(c => c.Catalogue_ID == goodCatalogueID).ToArray(), RemoteTable, null);
        Assert.That(cmd.Check(), Is.EqualTo(null));
    }

    [Test]
    public void UpdateLocationExecuteNoCheck_Bad()
    {
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>(), RemoteTable, "badMapping");
        var ex = Assert.Throws<Exception>(() => cmd.Execute());
        Assert.That(ex.Message,
            Is.EqualTo(
                "Unable to execute ExecuteCommandUpdateCatalogueDataLocation as the checks returned: Column Mapping must contain the string '$column'"));
    }

    [Test]
    public void UpdateLocationExecuteNoCheck_OK()
    {
        goodCatalogueID = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column2").First().Catalogue_ID;
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
                .Where(c => c.Catalogue_ID == goodCatalogueID).ToArray(), RemoteTable, null);
        Assert.DoesNotThrow(() => cmd.Execute());
        var ci = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column2" && c.Catalogue_ID == goodCatalogueID).First();
        Assert.That(ci.ColumnInfo.Name, Is.EqualTo(RemoteTable.GetFullyQualifiedName() + ".[Column2]"));
        Assert.That(ci.ExtractionInformation.SelectSQL, Is.EqualTo(RemoteTable.GetFullyQualifiedName() + ".[Column2]"));
        Assert.That(ci.ColumnInfo.TableInfo_ID, Is.Not.EqualTo(originalTableInfoID));
    }

    [Test]
    public void UpdateLocationExecuteNoCheck_AliasCheck()
    {
        goodCatalogueID = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column2").First().Catalogue_ID;
        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
                .Where(c => c.Catalogue_ID == goodCatalogueID).ToArray(), RemoteTable, null);
        Assert.DoesNotThrow(() => cmd.Execute());
        var ci = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column1" && c.Catalogue_ID == goodCatalogueID).First();
        Assert.That(ci.ColumnInfo.Name, Is.EqualTo(RemoteTable.GetFullyQualifiedName() + ".[Column1]"));
        var ei = RepositoryLocator.CatalogueRepository.GetAllObjects<ExtractionInformation>()
            .Where(e => e.ID == ci.ExtractionInformation.ID).First();
        Assert.That(ei.SelectSQL, Is.EqualTo(RemoteTable.GetFullyQualifiedName() + ".[Column1] as SOME_ALIAS"));
        Assert.That(ci.ColumnInfo.TableInfo_ID, Is.Not.EqualTo(originalTableInfoID));
    }

    [Test]
    public void UpdateLocationWithMultipleExtractionIdentifiers()
    {
        goodCatalogueID = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column2").First().Catalogue_ID;
        var ci = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column2" && c.Catalogue_ID == goodCatalogueID).First();

        var otherci = RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
            .Where(c => c.Name == "Column").First();
        var cmd1 = new ExecuteCommandAddNewCatalogueItem(new ThrowImmediatelyActivator(RepositoryLocator), ci.Catalogue,
            new List<ColumnInfo> { otherci.ColumnInfo }.ToArray());
        Assert.DoesNotThrow(() => cmd1.Execute());

        var cmd = new ExecuteCommandUpdateCatalogueDataLocation(new ThrowImmediatelyActivator(RepositoryLocator),
            RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>()
                .Where(c => c.Catalogue_ID == goodCatalogueID && c.Name == "Column2").ToArray(), RemoteTable, null);
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(ci.ColumnInfo.Name, Is.EqualTo(RemoteTable.GetFullyQualifiedName() + ".[Column2]"));
        var ei = RepositoryLocator.CatalogueRepository.GetAllObjects<ExtractionInformation>()
            .Where(e => e.ID == ci.ExtractionInformation.ID).First();
        Assert.That(ei.SelectSQL, Is.EqualTo(RemoteTable.GetFullyQualifiedName() + ".[Column2]"));
        Assert.That(ci.ColumnInfo.TableInfo_ID, Is.Not.EqualTo(originalTableInfoID));
    }
}