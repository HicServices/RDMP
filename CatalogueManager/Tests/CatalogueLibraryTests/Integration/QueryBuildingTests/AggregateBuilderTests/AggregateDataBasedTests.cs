using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.MainFormUITabs.SubComponents;
using DataLoadEngine.DataFlowPipeline.Destinations;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.QueryBuildingTests.AggregateBuilderTests
{
    public class AggregateDataBasedTests:DatabaseTests
    {
        private DataTable GetTestDataTable()
        {
            DataTable dt = new DataTable();
            dt.TableName = "AggregateDataBasedTests";

            dt.Columns.Add("EventDate");
            dt.Columns.Add("Category");
            dt.Columns.Add("NumberInTrouble");

            dt.Rows.Add("2001-01-01", "T", "7");
            dt.Rows.Add("2001-01-02", "T", "11");
            dt.Rows.Add("2002-02-01", "T", "13");
            dt.Rows.Add("2002-03-02", "T", "17");
            dt.Rows.Add("2003-01-01", "T", "19");
            dt.Rows.Add("2003-04-02", "T", "23");
            dt.Rows.Add("2001-01-01", "T", "49");

            dt.Rows.Add("2002-01-01", "F", "29");
            dt.Rows.Add("2002-01-01", "F", "31");

            dt.Rows.Add("2001-01-01", "E", "37");
            dt.Rows.Add("2002-01-01", "E", "41");
            dt.Rows.Add("2003-01-01", "E", "59");

            dt.Rows.Add(null, "G", "47");
            dt.Rows.Add("2001-01-01", "G", "53");

            return dt;
        }

        #region Helper methods
        private DiscoveredServer GetServer(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return DiscoveredServerICanCreateRandomDatabasesAndTablesOn;
                case DatabaseType.MYSQLServer:
                    return DiscoveredMySqlServer;
                case DatabaseType.Oracle:
                    return DiscoveredOracleServer;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private DiscoveredTable UploadTestDataAsTableToServer(DatabaseType type, out Catalogue catalogue, out ExtractionInformation[] extractionInformations, out TableInfo tableinfo)
        {
            DiscoveredServer server = GetServer(type);
            var listener = new ToConsoleDataLoadEventReceiver();

            if (server == null)
                Assert.Inconclusive();

            var db = server.ExpectDatabase(TestDatabaseNames.GetConsistentName("AggregateDataBasedTests"));
            db.Create(true);

            var data = GetTestDataTable();

            var uploader = new DataTableUploadDestination();
            uploader.PreInitialize(db, listener);
            uploader.ProcessPipelineData(data, listener, new GracefulCancellationToken());
            uploader.Dispose(listener, null);
            var tbl = db.ExpectTable(uploader.TargetTableName);

            Assert.IsTrue(tbl.Exists());

            ColumnInfo[] cis;
            new TableInfoImporter(CatalogueRepository, tbl).DoImport(out tableinfo, out cis);


            CatalogueItem[] cataitems;
            new ForwardEngineerCatalogue(tableinfo, cis, true).ExecuteForwardEngineering(out catalogue, out cataitems,out extractionInformations);

            return tbl;
        }
        private void Destroy(DiscoveredTable tbl, params IDeleteable[] deletablesInOrderOfDeletion)
        {
            tbl.Drop();
            foreach (IDeleteable deleteable in deletablesInOrderOfDeletion)
                deleteable.DeleteInDatabase();
        }

        private DataTable GetResultForBuilder(AggregateBuilder builder, DiscoveredTable tbl)
        {
            string sql = builder.SQL;

            using (var con = tbl.Database.Server.GetConnection())
            {
                con.Open();
                var da = tbl.Database.Server.GetDataAdapter(sql, con);
                var toReturn = new DataTable();
                da.Fill(toReturn);

                return toReturn;
            }
        }
        private void VerifyRowExist(DataTable resultTable, params object[] rowObjects)
        {
            if (resultTable.Columns.Count != rowObjects.Length)
                Assert.Fail("VerifyRowExist failed, resultTable had " + resultTable.Columns.Count + " while you expected " + rowObjects.Length + " columns");

            foreach (DataRow r in resultTable.Rows)
            {
                bool matchAll = true;
                for (int i = 0; i < rowObjects.Length; i++)
                {
                    if (!AreBasicallyEquals(rowObjects[i], r[i]))
                        matchAll = false;
                }

                //found a row that matches on all params
                if (matchAll)
                    return;
            }

            Assert.Fail("VerifyRowExist failed, did not find expected rowObjects in the resultTable");
        }

        private bool AreBasicallyEquals(object o, object o2)
        {
            //if they are legit equals
            if (Equals(o, o2))
                return true;

            //if they are null but basically the same
            var oIsNull = o == null || o == DBNull.Value;
            var o2IsNull = o2 == null || o2 == DBNull.Value;

            if (oIsNull || o2IsNull)
                return oIsNull == o2IsNull;

            //they are not null so tostring them deals with int vs long etc that DbDataAdapters can be a bit flaky on
            return string.Equals(o.ToString(), o2.ToString());
        }

        #endregion

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void Count_CorrectNumberOfRowsCalculated(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type,out catalogue, out extractionInformations, out tableInfo);

            var builder = new AggregateBuilder(null, "count(*)", null,new []{tableInfo});
            var resultTable = GetResultForBuilder(builder, tbl);

            //table has 14 rows
            VerifyRowExist(resultTable, 14);

            Destroy(tbl, catalogue, tableInfo);
        }

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_CategoryWithCount_Correct(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate
            var categoryDimension = extractionInformations.Single(e => e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));
            var configuration  = new AggregateConfiguration(CatalogueRepository,catalogue,"GroupBy_Category");
            var dimension = new AggregateDimension(CatalogueRepository, categoryDimension, configuration);

            //get the result of the aggregate 
            var builder = new AggregateBuilder(null, configuration.CountSQL,configuration);
            builder.AddColumn(dimension);
            var resultTable = GetResultForBuilder(builder, tbl);

            VerifyRowExist(resultTable, "T", 7);
            VerifyRowExist(resultTable, "F", 2);
            VerifyRowExist(resultTable, "E", 3);
            VerifyRowExist(resultTable, "G", 2);

            Destroy(tbl, configuration,catalogue, tableInfo);
        }

    }
}
