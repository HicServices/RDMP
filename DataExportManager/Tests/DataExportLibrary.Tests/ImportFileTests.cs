using System;
using System.Data.SqlClient;
using System.IO;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.Tests.DataExtraction;
using DataExportLibrary;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataExportLibrary.Tests
{
    public class ImportFileTests:DatabaseTests
    {
        [Test]
        public void ImportFile()
         {
             Exception ex = null;
            string file = Path.GetTempFileName();
            string databaseName = TestDatabaseNames.GetConsistentName(GetType().Name);
            CsvDataTableHelper csvDataTableHelper = null;

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


                csvDataTableHelper = new CsvDataTableHelper(file);

                csvDataTableHelper.LoadDataTableFromFile();

                csvDataTableHelper.Check(new ThrowImmediatelyCheckNotifier());

                var server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn;
                var database = server.ExpectDatabase(databaseName);

                if(!database.Exists())
                    server.CreateDatabase(databaseName);
                
                server.ChangeDatabase(databaseName);

                var dt = csvDataTableHelper.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
                
                var tbl = database.CreateTable(dt.TableName, dt);
                string tableName = tbl.GetRuntimeName();

                csvDataTableHelper.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);

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

                    SqlCommand cmdReadData =
                        new SqlCommand(
                            "Select * from " + tablesInDatabase[0].GetRuntimeName() + " WHERE Name='Frank'", con);
                    SqlDataReader r = cmdReadData.ExecuteReader();

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

                server.ExpectDatabase(databaseName).ForceDrop();
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

        private string GetColumnType(DiscoveredDatabase database, string tableName, string colName)
        {
            return
                database.ExpectTable(tableName).DiscoverColumn(colName).DataType.SQLType;
        }
    }
}
