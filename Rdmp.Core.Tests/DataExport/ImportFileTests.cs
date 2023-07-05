// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.IO;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport;

public class ImportFileTests:DatabaseTests
{
    [Test]
    public void ImportFile()
    {
        var file = Path.GetTempFileName();
        var databaseName = TestDatabaseNames.GetConsistentName(GetType().Name);
            
        try
        {
            using (var sw = new StreamWriter(file))
            {
                sw.WriteLine("Name,Surname,Age,Healthiness,DateOfImagining");
                sw.WriteLine("Frank,\"Mortus,M\",41,0.00,2005-12-01");
                sw.WriteLine("Bob,Balie,12,1,2013-06-11");
                sw.WriteLine("Munchen,'Smith',43,0.3,2002-01-01");
                sw.WriteLine("Carnage,Here there is,29,0.91,2005-01-01");
                sw.WriteLine("Nathan,Crumble,51,0.78,2005-01-01");
                sw.Close();
            }

            var source = new DelimitedFlatFileDataFlowSource
            {
                Separator = ",",
                IgnoreBlankLines = true,
                MakeHeaderNamesSane = true,
                StronglyTypeInputBatchSize = -1,
                StronglyTypeInput = true
            };

            source.PreInitialize(new FlatFileToLoad(new FileInfo(file)), new ThrowImmediatelyDataLoadEventListener());//this is the file we want to load
            source.Check(new ThrowImmediatelyCheckNotifier());
                
            var server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn;
            var database = server.ExpectDatabase(databaseName);

            //recreate it
            database.Create(true);
                
            server.ChangeDatabase(databaseName);

            var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
                
            var tbl = database.CreateTable(dt.TableName, dt);
            var tableName = tbl.GetRuntimeName();

            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);

            var tablesInDatabase = server.ExpectDatabase(databaseName).DiscoverTables(false);

            //there should be 1 table in this database
            Assert.AreEqual(1, tablesInDatabase.Length);

            //it should be called the same as the file loaded
            Assert.AreEqual(Path.GetFileNameWithoutExtension(file), tablesInDatabase[0].GetRuntimeName());

            Assert.AreEqual("varchar(7)", GetColumnType(database, tableName, "Name"));
            Assert.AreEqual("varchar(13)", GetColumnType(database, tableName, "Surname"));
            Assert.AreEqual("int", GetColumnType(database, tableName, "Age"));
            Assert.AreEqual("decimal(3,2)", GetColumnType(database, tableName, "Healthiness"));
            Assert.AreEqual("datetime2", GetColumnType(database, tableName, "DateOfImagining"));

            using (var con = (SqlConnection) server.GetConnection())
            {
                con.Open();

                var cmdReadData =
                    new SqlCommand(
                        $"Select * from {tablesInDatabase[0].GetRuntimeName()} WHERE Name='Frank'", con);
                var r = cmdReadData.ExecuteReader();

                //expected 1 record only
                Assert.IsTrue(r.Read());

                Assert.AreEqual("Frank", r["Name"]);
                Assert.AreEqual("Mortus,M", r["Surname"]);
                Assert.AreEqual(41, r["Age"]);
                Assert.AreEqual(0.0f, r["Healthiness"]);
                Assert.AreEqual(new DateTime(2005, 12, 1), r["DateOfImagining"]);

                //and no more records
                Assert.IsFalse(r.Read());

                con.Close();
            }

            server.ExpectDatabase(databaseName).Drop();
            Assert.IsFalse(server.ExpectDatabase(databaseName).Exists());
        }
        finally 
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                //Couldn't delete temporary file... oh well
            }
                
        }

    }

    private static string GetColumnType(DiscoveredDatabase database, string tableName, string colName)
    {
        return
            database.ExpectTable(tableName).DiscoverColumn(colName).DataType.SQLType;
    }
}