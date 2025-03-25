// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

internal class DatabaseOperationTests : DatabaseTests
{
    private readonly Stack<IDeleteable> toCleanUp = new();

    [Test]
    // This no longer copies between servers, but the original test didn't guarantee that would happen anyway
    public void CloneDatabaseAndTable()
    {
        var testLiveDatabaseName = TestDatabaseNames.GetConsistentName("TEST");

        var testDb = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testLiveDatabaseName);
        var raw = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase($"{testLiveDatabaseName}_RAW");

        foreach (var db in new[] { raw, testDb })
            if (db.Exists())
            {
                foreach (var table in db.DiscoverTables(true))
                    table.Drop();

                db.Drop();
            }

        DiscoveredServerICanCreateRandomDatabasesAndTablesOn.CreateDatabase(testLiveDatabaseName);
        Assert.That(testDb.Exists());

        testDb.CreateTable("Table_1", new[] { new DatabaseColumnRequest("Id", "int") });


        //clone the builder
        var builder =
            new SqlConnectionStringBuilder(
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString)
            {
                InitialCatalog = testLiveDatabaseName
            };

        var dbConfiguration = new HICDatabaseConfiguration(new DiscoveredServer(builder), null, CatalogueRepository);

        var cloner = new DatabaseCloner(dbConfiguration);
        try
        {
            cloner.CreateDatabaseForStage(LoadBubble.Raw);

            //confirm database appeared
            Assert.That(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(
                $"{testLiveDatabaseName}_RAW").Exists());

            //now create a catalogue and wire it SetUp to the table TEST on the test database server
            var cata = SetupATestCatalogue(builder, testLiveDatabaseName, "Table_1");

            //now clone the catalogue data structures to MachineName
            foreach (TableInfo tableInfo in cata.GetTableInfoList(false))
                cloner.CreateTablesInDatabaseFromCatalogueInfo(ThrowImmediatelyDataLoadEventListener.Quiet, tableInfo,
                    LoadBubble.Raw, false);

            Assert.Multiple(() =>
            {
                Assert.That(raw.Exists());
                Assert.That(raw.ExpectTable("Table_1").Exists());
            });
        }
        finally
        {
            cloner.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

            while (toCleanUp.Count > 0)
                try
                {
                    toCleanUp.Pop().DeleteInDatabase();
                }
                catch (Exception e)
                {
                    //always clean SetUp everything
                    Console.WriteLine(e);
                }
        }
    }

    private Catalogue SetupATestCatalogue(SqlConnectionStringBuilder builder, string database, string table)
    {
        //create a new catalogue for test data (in the test data catalogue)
        var cat = new Catalogue(CatalogueRepository, "DeleteMe");
        var importer = new TableInfoImporter(CatalogueRepository, builder.DataSource, database, table,
            DatabaseType.MicrosoftSQLServer, builder.UserID, builder.Password);
        importer.DoImport(out var tableInfo, out var columnInfos);

        toCleanUp.Push(cat);

        //push the credentials if there are any
        var creds = (DataAccessCredentials)tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
        if (creds != null)
            toCleanUp.Push(creds);

        //and the TableInfo
        toCleanUp.Push(tableInfo);

        //for each column we will add a new one to the
        foreach (var col in columnInfos)
        {
            //create it with the same name
            var cataItem = new CatalogueItem(CatalogueRepository, cat,
                col.Name[(col.Name.LastIndexOf(".", StringComparison.Ordinal) + 1)..].Trim('[', ']', '`'));
            toCleanUp.Push(cataItem);

            cataItem.SetColumnInfo(col);

            toCleanUp.Push(col);
        }


        return cat;
    }
}