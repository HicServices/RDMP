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

public class ImportFileTests : DatabaseTests
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

            source.PreInitialize(new FlatFileToLoad(new FileInfo(file)),
                ThrowImmediatelyDataLoadEventListener.Quiet); //this is the file we want to load
            source.Check(ThrowImmediatelyCheckNotifier.Quiet);

            var server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn;
            var database = server.ExpectDatabase(databaseName);

            //recreate it
            database.Create(true);

            server.ChangeDatabase(databaseName);

            var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

            var tbl = database.CreateTable(dt.TableName, dt);
            var tableName = tbl.GetRuntimeName();

            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);

            var tablesInDatabase = server.ExpectDatabase(databaseName).DiscoverTables(false);

            //there should be 1 table in this database
            Assert.That(tablesInDatabase, Has.Length.EqualTo(1));

            Assert.Multiple(() =>
            {
                //it should be called the same as the file loaded
                Assert.That(tablesInDatabase[0].GetRuntimeName(), Is.EqualTo(Path.GetFileNameWithoutExtension(file)));

                Assert.That(GetColumnType(database, tableName, "Name"), Is.EqualTo("varchar(7)"));
                Assert.That(GetColumnType(database, tableName, "Surname"), Is.EqualTo("varchar(13)"));
                Assert.That(GetColumnType(database, tableName, "Age"), Is.EqualTo("int"));
                Assert.That(GetColumnType(database, tableName, "Healthiness"), Is.EqualTo("decimal(3,2)"));
                Assert.That(GetColumnType(database, tableName, "DateOfImagining"), Is.EqualTo("datetime2"));
            });

            using (var con = (SqlConnection)server.GetConnection())
            {
                con.Open();

                var cmdReadData =
                    new SqlCommand(
                        $"Select * from {tablesInDatabase[0].GetRuntimeName()} WHERE Name='Frank'", con);
                var r = cmdReadData.ExecuteReader();

                Assert.Multiple(() =>
                {
                    //expected 1 record only
                    Assert.That(r.Read());

                    Assert.That(r["Name"], Is.EqualTo("Frank"));
                    Assert.That(r["Surname"], Is.EqualTo("Mortus,M"));
                    Assert.That(r["Age"], Is.EqualTo(41));
                    Assert.That(r["Healthiness"], Is.EqualTo(0.0f));
                    Assert.That(r["DateOfImagining"], Is.EqualTo(new DateTime(2005, 12, 1)));
                });

                //and no more records
                Assert.That(r.Read(), Is.False);

                con.Close();
            }

            server.ExpectDatabase(databaseName).Drop();
            Assert.That(server.ExpectDatabase(databaseName).Exists(), Is.False);
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

    private static string GetColumnType(DiscoveredDatabase database, string tableName, string colName) =>
        database.ExpectTable(tableName).DiscoverColumn(colName).DataType.SQLType;
}