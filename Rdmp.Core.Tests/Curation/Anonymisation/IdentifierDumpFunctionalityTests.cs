// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.Curation.Anonymisation;

public class IdentifierDumpFunctionalityTests:TestsRequiringFullAnonymisationSuite
{
    private ITableInfo tableInfoCreated;
    private ColumnInfo[] columnInfosCreated;
    private BulkTestsData _bulkData;

    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        Console.WriteLine("Setting SetUp bulk test data");
        _bulkData = new BulkTestsData(RepositoryLocator.CatalogueRepository, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer));
        _bulkData.SetupTestData();
            
        Console.WriteLine("Importing to Catalogue");
        var tbl = _bulkData.Table;
        var importer = new TableInfoImporter(CatalogueRepository, tbl);

        importer.DoImport(out tableInfoCreated,out columnInfosCreated);
            
        Console.WriteLine($"Imported TableInfo {tableInfoCreated}");
        Console.WriteLine(
            $"Imported ColumnInfos {string.Join(",", columnInfosCreated.Select(c => c.GetRuntimeName()))}");

        Assert.NotNull(tableInfoCreated);

        var chi = columnInfosCreated.Single(c => c.GetRuntimeName().Equals("chi"));

        Console.WriteLine($"CHI is primary key? (expecting true):{chi.IsPrimaryKey}");
        Assert.IsTrue(chi.IsPrimaryKey);


        tableInfoCreated.ColumnInfos.Single(c => c.GetRuntimeName().Equals("surname")).DeleteInDatabase();
        tableInfoCreated.ColumnInfos.Single(c => c.GetRuntimeName().Equals("forename")).DeleteInDatabase();
        tableInfoCreated.ClearAllInjections();
    }


    #region tests that pass
    [Test]
    public void DumpAllIdentifiersInTable_Passes()
    {
        var preDiscardedColumn1 = new PreLoadDiscardedColumn(CatalogueRepository, tableInfoCreated, "surname")
        {
            Destination = DiscardedColumnDestination.StoreInIdentifiersDump,
            SqlDataType = "varchar(20)"
        };
        preDiscardedColumn1.SaveToDatabase();

        //give it the correct server
        tableInfoCreated.IdentifierDumpServer_ID = IdentifierDump_ExternalDatabaseServer.ID;
        tableInfoCreated.SaveToDatabase();

        var dumper = new IdentifierDumper(tableInfoCreated);

        var chiToSurnameDictionary = new Dictionary<string, HashSet<string>>();
        try
        {
            dumper.Check(new AcceptAllCheckNotifier());

            var dt = _bulkData.GetDataTable(1000);

            Assert.AreEqual(1000,dt.Rows.Count);
            Assert.IsTrue(dt.Columns.Contains("surname"));

            //for checking the final ID table has the correct values in
            foreach (DataRow row in dt.Rows)
            {
                var chi = row["chi"].ToString();

                if(!chiToSurnameDictionary.ContainsKey(chi))
                    chiToSurnameDictionary.Add(chi,new HashSet<string>());

                chiToSurnameDictionary[chi].Add(row["surname"] as string);
            }

            dumper.CreateSTAGINGTable();
            dumper.DumpAllIdentifiersInTable(dt);
            dumper.DropStaging();

            //confirm that the surname column is no longer in the pipeline
            Assert.IsFalse(dt.Columns.Contains("surname"));

            //now look at the ids in the identifier dump and make sure they match what was in the pipeline before we sent it
            var server = IdentifierDump_Database.Server;
            using (var con = server.GetConnection())
            {
                con.Open();

                var cmd = server.GetCommand($"Select * from ID_{BulkTestsData.BulkDataTable}", con);
                var r = cmd.ExecuteReader();
                    
                //make sure the values in the ID table match the ones we originally had in the pipeline
                while (r.Read())
                    if (!chiToSurnameDictionary[r["chi"].ToString()].Any())
                        Assert.IsTrue(r["surname"] == DBNull.Value);
                    else
                        Assert.IsTrue(chiToSurnameDictionary[r["chi"].ToString()].Contains(r["surname"] as string),"Dictionary did not contain expected surname:" + r["surname"]);
                r.Close();

                //leave the identifier dump in the way we found it (empty)
                var tbl = IdentifierDump_Database.ExpectTable($"ID_{BulkTestsData.BulkDataTable}");

                if(tbl.Exists())
                    tbl.Drop();

                tbl = IdentifierDump_Database.ExpectTable($"ID_{BulkTestsData.BulkDataTable}_Archive");

                if (tbl.Exists())
                    tbl.Drop();
            }
        }
        finally
        {
            preDiscardedColumn1.DeleteInDatabase();
            tableInfoCreated.IdentifierDumpServer_ID = null;//reset it back to how it was when we found it
            tableInfoCreated.SaveToDatabase();
        }

    }


    #endregion


    #region tests that throw
    [Test]
    public void DumpAllIdentifiersInTable_UnexpectedColumnFoundInIdentifierDumpTable()
    {
        var preDiscardedColumn1 = new PreLoadDiscardedColumn(CatalogueRepository, tableInfoCreated, "surname")
            {
                Destination = DiscardedColumnDestination.StoreInIdentifiersDump,
                SqlDataType = "varchar(20)"
            };
        preDiscardedColumn1.SaveToDatabase();

        var preDiscardedColumn2 = new PreLoadDiscardedColumn(CatalogueRepository, tableInfoCreated, "forename")
            {
                Destination = DiscardedColumnDestination.StoreInIdentifiersDump,
                SqlDataType = "varchar(50)"
            };
        preDiscardedColumn2.SaveToDatabase();
            
        //give it the correct server
        tableInfoCreated.IdentifierDumpServer_ID = IdentifierDump_ExternalDatabaseServer.ID;
        tableInfoCreated.SaveToDatabase();

        var dumper = new IdentifierDumper(tableInfoCreated);
        dumper.Check(new AcceptAllCheckNotifier());

        var tableInDump = IdentifierDump_Database.ExpectTable($"ID_{BulkTestsData.BulkDataTable}");
        Assert.IsTrue(tableInDump.Exists(), "ID table did not exist");


        var columnsInDump = tableInDump.DiscoverColumns().Select(c=>c.GetRuntimeName()).ToArray();
        //works and creates table on server
        Assert.Contains("hic_validFrom",columnsInDump);
        Assert.Contains("forename", columnsInDump);
        Assert.Contains("chi", columnsInDump);
        Assert.Contains("surname", columnsInDump);

        //now delete it!
        preDiscardedColumn2.DeleteInDatabase();

        //now create a new dumper and watch it go crazy 
        var dumper2 = new IdentifierDumper(tableInfoCreated);

        var thrower = new ThrowImmediatelyCheckNotifier
        {
            ThrowOnWarning = true
        };

        try
        {
            var ex = Assert.Throws<Exception>(()=>dumper2.Check(thrower));
            Assert.AreEqual("Column forename was found in the IdentifierDump table ID_BulkData but was not one of the primary keys or a PreLoadDiscardedColumn",ex.Message);
        }
        finally
        {
            //Drop all this stuff
            var server = IdentifierDump_Database.Server;
            using (var con = server.GetConnection())
            {
                con.Open();

                //leave the identifier dump in the way we found it (empty)
                var cmdDrop = server.GetCommand($"DROP TABLE ID_{BulkTestsData.BulkDataTable}", con);
                cmdDrop.ExecuteNonQuery();

                var cmdDropArchive = server.GetCommand($"DROP TABLE ID_{BulkTestsData.BulkDataTable}_Archive", con);
                cmdDropArchive.ExecuteNonQuery();
            }

            preDiscardedColumn1.DeleteInDatabase();
            tableInfoCreated.IdentifierDumpServer_ID = null;//reset it back to how it was when we found it
            tableInfoCreated.SaveToDatabase();

        }

    }

    [Test]
    public void IdentifierDumperCheckFails_StagingNotCalled()
    {
        var preDiscardedColumn1 = new PreLoadDiscardedColumn(CatalogueRepository, tableInfoCreated, "forename")
        {
            Destination = DiscardedColumnDestination.StoreInIdentifiersDump,
            SqlDataType = "varchar(50)"
        };
        preDiscardedColumn1.SaveToDatabase();

        //give it the correct server
        tableInfoCreated.IdentifierDumpServer_ID = IdentifierDump_ExternalDatabaseServer.ID;
        tableInfoCreated.SaveToDatabase();

        var dumper = new IdentifierDumper(tableInfoCreated);
        try
        {
            dumper.Check(new AcceptAllCheckNotifier());
            var ex = Assert.Throws<Exception>(()=>dumper.DumpAllIdentifiersInTable(_bulkData.GetDataTable(10)));
            Assert.AreEqual("IdentifierDumper STAGING insert (ID_BulkData_STAGING) failed, make sure you have called CreateSTAGINGTable() before trying to Dump identifiers (also you should call DropStagging() when you are done)",ex.Message);
        }
        finally
        {
            preDiscardedColumn1.DeleteInDatabase();
            tableInfoCreated.IdentifierDumpServer_ID = null;//reset it back to how it was when we found it
            tableInfoCreated.SaveToDatabase();
        }
    }

    [Test]
    public void IdentifierDumperCheckFails_NoTableExists()
    {
        var preDiscardedColumn1 = new PreLoadDiscardedColumn(CatalogueRepository, tableInfoCreated, "forename")
            {
                Destination = DiscardedColumnDestination.StoreInIdentifiersDump,
                SqlDataType = "varchar(50)"
            };
        preDiscardedColumn1.SaveToDatabase();

        //give it the correct server
        tableInfoCreated.IdentifierDumpServer_ID = IdentifierDump_ExternalDatabaseServer.ID;
        tableInfoCreated.SaveToDatabase();

        var existingTable = DataAccessPortal
            .ExpectDatabase(IdentifierDump_ExternalDatabaseServer, DataAccessContext.InternalDataProcessing)
            .ExpectTable("ID_BulkData");

        if(existingTable.Exists())
            existingTable.Drop();

        var dumper = new IdentifierDumper(tableInfoCreated);

        try
        {
            var notifier = new ToMemoryCheckNotifier(new AcceptAllCheckNotifier());
            dumper.Check(notifier);

            Assert.IsTrue(notifier.Messages.Any(m=>
                m.Result == CheckResult.Warning 
                &&
                m.Message.Contains("Table ID_BulkData was not found")));
        }
        finally
        {
            preDiscardedColumn1.DeleteInDatabase();
            tableInfoCreated.IdentifierDumpServer_ID = null;//reset it back to how it was when we found it
            tableInfoCreated.SaveToDatabase();
        }
    }

    [Test]
    public void IdentifierDumperCheckFails_ServerIsNotADumpServer()
    {
        var preDiscardedColumn1 = new PreLoadDiscardedColumn(CatalogueRepository, tableInfoCreated, "NationalSecurityNumber")
            {
                Destination = DiscardedColumnDestination.StoreInIdentifiersDump,
                SqlDataType = "varchar(10)"
            };
        preDiscardedColumn1.SaveToDatabase();
            
        //give it the WRONG server
        tableInfoCreated.IdentifierDumpServer_ID = ANOStore_ExternalDatabaseServer.ID;
        tableInfoCreated.SaveToDatabase();

        var dumper = new IdentifierDumper(tableInfoCreated);
        try
        {
            dumper.Check(new ThrowImmediatelyCheckNotifier());
            Assert.Fail("Expected it to crash before now");
        }
        catch (Exception ex)
        {
            Assert.IsTrue(ex.Message.StartsWith("Exception occurred when trying to find stored procedure sp_createIdentifierDump"));
            Assert.IsTrue(ex.InnerException.Message.StartsWith("Connected successfully to server"));
            Assert.IsTrue(ex.InnerException.Message.EndsWith(" but did not find the stored procedure sp_createIdentifierDump in the database (Possibly the ExternalDatabaseServer is not an IdentifierDump database?)"));
        }
        finally
        {
            preDiscardedColumn1.DeleteInDatabase();
            tableInfoCreated.IdentifierDumpServer_ID = null;//reset it back to how it was when we found it
            tableInfoCreated.SaveToDatabase();
        }

    }

    [Test]
    public void IdentifierDumperCheckFails_NoTableOnServerRejectChange()
    {
        var preDiscardedColumn1 = new PreLoadDiscardedColumn(CatalogueRepository, tableInfoCreated, "NationalSecurityNumber");
        try
        {
            preDiscardedColumn1.Destination = DiscardedColumnDestination.StoreInIdentifiersDump;
            preDiscardedColumn1.SqlDataType = "varchar(10)";
            preDiscardedColumn1.SaveToDatabase();

            var ex = Assert.Throws<ArgumentException>(()=> new IdentifierDumper(tableInfoCreated));
            StringAssert.Contains("does not have a listed IdentifierDump ExternalDatabaseServer",ex.Message);
        }
        finally
        {
            preDiscardedColumn1.DeleteInDatabase();
        }
    }

    [Test]
    public void IdentifierDumperCheckFails_LieAboutDatatype()
    {
        var preDiscardedColumn1 = new PreLoadDiscardedColumn(CatalogueRepository, tableInfoCreated, "forename")
            {
                Destination = DiscardedColumnDestination.StoreInIdentifiersDump,
                SqlDataType = "varchar(50)"
            };
        preDiscardedColumn1.SaveToDatabase();
        try
        {
            //give it the correct server
            tableInfoCreated.IdentifierDumpServer_ID = IdentifierDump_ExternalDatabaseServer.ID;
            tableInfoCreated.SaveToDatabase();

            var dumper = new IdentifierDumper(tableInfoCreated);
            
            //table doesnt exist yet it should work
            dumper.Check(new AcceptAllCheckNotifier());
            
            //now it is varbinary
            preDiscardedColumn1.SqlDataType = "varbinary(200)";
            preDiscardedColumn1.SaveToDatabase();
             
            //get a new dumper because we have changed the pre load discarded column
            dumper = new IdentifierDumper(tableInfoCreated);
            //table doesnt exist yet it should work
            var ex = Assert.Throws<Exception>(()=>dumper.Check(new ThrowImmediatelyCheckNotifier()));

            Assert.IsTrue(ex.Message.Contains("has data type varbinary(200) in the Catalogue but appears as varchar(50) in the actual IdentifierDump"));
        }
        finally
        {
            preDiscardedColumn1.DeleteInDatabase();
            tableInfoCreated.IdentifierDumpServer_ID = null;//reset it back to how it was when we found it
            tableInfoCreated.SaveToDatabase();
        }
            
    }

    #endregion
}