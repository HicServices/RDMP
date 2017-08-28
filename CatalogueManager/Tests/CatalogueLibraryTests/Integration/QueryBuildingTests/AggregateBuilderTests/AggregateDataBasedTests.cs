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
using CatalogueLibrary.Spontaneous;
using CatalogueManager.MainFormUITabs.SubComponents;
using DataLoadEngine.DataFlowPipeline.Destinations;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
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
            dt.Rows.Add("2001-01-01", "T", "49");

            dt.Rows.Add("2002-02-01", "T", "13");
            dt.Rows.Add("2002-03-02", "T", "17");
            dt.Rows.Add("2003-01-01", "T", "19");
            dt.Rows.Add("2003-04-02", "T", "23");
            

            dt.Rows.Add("2002-01-01", "F", "29");
            dt.Rows.Add("2002-01-01", "F", "31");

            dt.Rows.Add("2001-01-01", "E", "37");
            dt.Rows.Add("2002-01-01", "E", "41");
            dt.Rows.Add("2005-01-01", "E", "59");  //note there are no records in 2004 it is important for axis tests (axis involves you having to build a calendar table)

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

            Assert.Fail("VerifyRowExist failed, did not find expected rowObjects ("+string.Join(",",rowObjects.Select(o=>"'"+ o + "'"))+") in the resultTable");
        }

        private bool AreBasicallyEquals(object o, object o2)
        {
            //if they are legit equals
            if (Equals(o, o2))
                return true;

            //if they are null but basically the same
            var oIsNull = o == null || o == DBNull.Value || o.ToString().Equals("0");
            var o2IsNull = o2 == null || o2 == DBNull.Value || o2.ToString().Equals("0");

            if (oIsNull || o2IsNull)
                return oIsNull == o2IsNull;

            //they are not null so tostring them deals with int vs long etc that DbDataAdapters can be a bit flaky on
            return string.Equals(o.ToString(), o2.ToString());
        }

        private void AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(AggregateBuilder builder, DatabaseType type)
        {
            var syntaxHelper = new QuerySyntaxHelperFactory().Create(type);
            var declaration = syntaxHelper.GetParameterDeclaration("@category", new DatabaseTypeRequest(typeof(string), 1));

            var ORContainer = new SpontaneouslyInventedFilterContainer(null, null, FilterContainerOperation.OR);
            var constParam = new ConstantParameter(declaration, "'T'", "T Category Only");
            var filter1 = new SpontaneouslyInventedFilter(ORContainer, "Category=@category", "Category Is @category",
                "ensures the records belong to the category @category", new ISqlParameter[] { constParam });
            var filter2 = new SpontaneouslyInventedFilter(ORContainer, "NumberInTrouble > 42",
                "number in trouble greater than 42", "See above", null);

            ORContainer.AddChild(filter1);
            ORContainer.AddChild(filter2);

            builder.RootFilterContainer = ORContainer;
        }

        private AggregateConfiguration SetupAggregateWithAxis(DatabaseType type, ExtractionInformation[] extractionInformations,
            Catalogue catalogue, out AggregateDimension axisDimension)
        {
            var dateDimension =
                extractionInformations.Single(
                    e => e.GetRuntimeName().Equals("EventDate", StringComparison.CurrentCultureIgnoreCase));
            var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
            axisDimension = new AggregateDimension(CatalogueRepository, dateDimension, configuration);

            var syntaxHelper = new QuerySyntaxHelperFactory().Create(type);

            var axis = new AggregateContinuousDateAxis(CatalogueRepository, axisDimension);
            axis.StartDate = "'2000-01-01'";
            axis.EndDate = syntaxHelper.GetScalarFunctionSql(MandatoryScalarFunctions.GetTodaysDate);
            axis.AxisIncrement = AxisIncrement.Year;
            axis.SaveToDatabase();
            return configuration;
        }

        private AggregateConfiguration SetupAggregateWithPivot(DatabaseType type, ExtractionInformation[] extractionInformations,
            Catalogue catalogue, out AggregateDimension axisDimension, out AggregateDimension pivotDimension)
        {
            var axisCol =
                extractionInformations.Single(
                    e => e.GetRuntimeName().Equals("EventDate", StringComparison.CurrentCultureIgnoreCase));
            var categoryCol =
                extractionInformations.Single(
                    e => e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));


            var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
            axisDimension = new AggregateDimension(CatalogueRepository, axisCol, configuration);
            pivotDimension = new AggregateDimension(CatalogueRepository, categoryCol, configuration);

            var syntaxHelper = new QuerySyntaxHelperFactory().Create(type);

            var axis = new AggregateContinuousDateAxis(CatalogueRepository, axisDimension);
            axis.StartDate = "'2000-01-01'";
            axis.EndDate = syntaxHelper.GetScalarFunctionSql(MandatoryScalarFunctions.GetTodaysDate);
            axis.AxisIncrement = AxisIncrement.Year;
            axis.SaveToDatabase();
            return configuration;
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
            try
            {
                //table has 14 rows
                VerifyRowExist(resultTable, 14);
            }
            finally
            {
                Destroy(tbl, catalogue, tableInfo);
            }
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

            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(dimension);
                var resultTable = GetResultForBuilder(builder, tbl);

                VerifyRowExist(resultTable, "T", 7);
                VerifyRowExist(resultTable, "F", 2);
                VerifyRowExist(resultTable, "E", 3);
                VerifyRowExist(resultTable, "G", 2);
            }
            finally
            {
                Destroy(tbl, configuration, catalogue, tableInfo);
            }
        }

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_CategoryWithSum_Correct(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate
            var categoryDimension = extractionInformations.Single(e => e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));
            var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
            var dimension = new AggregateDimension(CatalogueRepository, categoryDimension, configuration);

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.SaveToDatabase();
            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(dimension);
                var resultTable = GetResultForBuilder(builder, tbl);

                VerifyRowExist(resultTable, "T", 139);
                VerifyRowExist(resultTable, "F", 60);
                VerifyRowExist(resultTable, "E", 137);
                VerifyRowExist(resultTable, "G", 100);
                Assert.AreEqual(4,resultTable.Rows.Count);
            }
            finally
            {
                Destroy(tbl, configuration, catalogue, tableInfo);
            }
        }

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_CategoryWithSum_WHEREStatement(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate
            var categoryDimension = extractionInformations.Single(e => e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));
            var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
            var dimension = new AggregateDimension(CatalogueRepository, categoryDimension, configuration);

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.SaveToDatabase();

            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(dimension);

                AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder,type);

                var resultTable = GetResultForBuilder(builder, tbl);

                //T is matched on all records so they are summed
                VerifyRowExist(resultTable, "T", 139);
                //VerifyRowExist(resultTable, "F", 60); //F does not have any records over 42 and isn't T so shouldnt be matched
                VerifyRowExist(resultTable, "E", 59); //E has 1 records over 42
                VerifyRowExist(resultTable, "G", 100); //47 + 53
                Assert.AreEqual(3, resultTable.Rows.Count);
            }
            finally
            {
                Destroy(tbl, configuration, catalogue, tableInfo);
            }
        }
        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_AxisWithSum_Correct(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);
            
            //setup the aggregate with axis
            AggregateDimension dimension;
            var configuration = SetupAggregateWithAxis(type, extractionInformations, catalogue, out dimension);

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.SaveToDatabase();

            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(dimension);

                var resultTable = GetResultForBuilder(builder, tbl);

                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.IsTrue(AreBasicallyEquals( "2000",resultTable.Rows[0][0]));

                VerifyRowExist(resultTable, "2000", null); //because it is a SUM the ANSI return should be null not 0 since it is a sum of no records
                VerifyRowExist(resultTable, "2001", 157);
                VerifyRowExist(resultTable, "2002", 131);
                VerifyRowExist(resultTable, "2003", 42);
                VerifyRowExist(resultTable, "2004", null);
                VerifyRowExist(resultTable, "2005", 59);
                VerifyRowExist(resultTable, "2006", null);
                VerifyRowExist(resultTable, "2007", null);
            }
            finally
            {
                Destroy(tbl, configuration, catalogue, tableInfo);
            }
            
        }
        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_AxisWithCount_HAVING(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate with axis
            AggregateDimension dimension;
            var configuration = SetupAggregateWithAxis(type, extractionInformations, catalogue, out dimension);

            configuration.CountSQL = "count(*)";
            configuration.HavingSQL = "count(*)>3"; //matches only years with more than 3 records
            configuration.SaveToDatabase();

            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(dimension);

                var resultTable = GetResultForBuilder(builder, tbl);

                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.IsTrue(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                VerifyRowExist(resultTable, "2000", null); //records only showing where there are more than 3 records (HAVING refers to the year since theres no pivot)
                VerifyRowExist(resultTable, "2001", 5);
                VerifyRowExist(resultTable, "2002", 5);
                VerifyRowExist(resultTable, "2003", null);
                VerifyRowExist(resultTable, "2004", null);
                VerifyRowExist(resultTable, "2005", null);
                VerifyRowExist(resultTable, "2006", null);
                VerifyRowExist(resultTable, "2007", null);
            }
            finally
            {
                Destroy(tbl, configuration, catalogue, tableInfo);
            }

        }


        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_AxisWithCount_WHERECorrect(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate with axis
            AggregateDimension dimension;
            var configuration = SetupAggregateWithAxis(type, extractionInformations, catalogue, out dimension);

            configuration.CountSQL = "count(NumberInTrouble)";
            configuration.SaveToDatabase();
            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(dimension);

                AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder,type);

                var resultTable = GetResultForBuilder(builder, tbl);

                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.IsTrue(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                VerifyRowExist(resultTable, "2000", null);
                VerifyRowExist(resultTable, "2001", 4); //4 are T or > 42
                VerifyRowExist(resultTable, "2002", 2);
                VerifyRowExist(resultTable, "2003", 2); //only the first date in the test data is <= 2003-01-01
            }
            finally
            {
                Destroy(tbl, configuration, catalogue, tableInfo);
            }
        }

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_PivotWithSum_Correct(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate pivot (and axis)
            AggregateDimension axisDimension;
            AggregateDimension pivotDimension;
            var configuration = SetupAggregateWithPivot(type, extractionInformations, catalogue, out axisDimension, out pivotDimension);

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category

            configuration.SaveToDatabase();
            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(axisDimension);
                builder.AddColumn(pivotDimension);
                builder.SetPivotToDimensionID(pivotDimension);

                var resultTable = GetResultForBuilder(builder, tbl);

                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.IsTrue(AreBasicallyEquals("2000", resultTable.Rows[0][0]));
            
                Assert.AreEqual("T",resultTable.Columns[1].ColumnName);
                Assert.AreEqual("E", resultTable.Columns[2].ColumnName);
                Assert.AreEqual("F", resultTable.Columns[3].ColumnName);
                Assert.AreEqual("G", resultTable.Columns[4].ColumnName);

                //T,E,F,G
                VerifyRowExist(resultTable, "2000", null, null, null, null);//no records in 2000 but it is important it appears still because that is what the axis says
                VerifyRowExist(resultTable, "2001", 67, 37, null, 53);
                VerifyRowExist(resultTable, "2002", 30, 41, 60, null);
                VerifyRowExist(resultTable, "2003", 42, null, null, null);
                VerifyRowExist(resultTable, "2004", null, null, null, null);
                VerifyRowExist(resultTable, "2005", null, 59,null , null);
                VerifyRowExist(resultTable, "2006", null, null, null, null);
                VerifyRowExist(resultTable, "2007", null, null, null, null);
            }
            finally
            {
                Destroy(tbl, configuration, catalogue, tableInfo);
            }
            
        }


        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_PivotWithSum_WHEREStatement(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate pivot (and axis)
            AggregateDimension axisDimension;
            AggregateDimension pivotDimension;
            var configuration = SetupAggregateWithPivot(type, extractionInformations, catalogue, out axisDimension, out pivotDimension);

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category

            configuration.SaveToDatabase();
            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(axisDimension);
                builder.AddColumn(pivotDimension);
                builder.SetPivotToDimensionID(pivotDimension);

                AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder,type);

                var resultTable = GetResultForBuilder(builder, tbl);

                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.IsTrue(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                Assert.AreEqual("T", resultTable.Columns[1].ColumnName);
                Assert.AreEqual("E", resultTable.Columns[2].ColumnName);
                Assert.AreEqual("G", resultTable.Columns[3].ColumnName);

                //T,E,G - F does not appear because WHERE throws it out (both counts are below 42)
                VerifyRowExist(resultTable, "2000", null, null, null); //no records in 2000 but it is important it appears still because that is what the axis says
                VerifyRowExist(resultTable, "2001", 67, null, 53);
                VerifyRowExist(resultTable, "2002", 30, null, null);
                VerifyRowExist(resultTable, "2003", 42, null, null);
                VerifyRowExist(resultTable, "2004", null, null, null);
                VerifyRowExist(resultTable, "2005", null, 59,  null);
                VerifyRowExist(resultTable, "2006", null, null, null);
                VerifyRowExist(resultTable, "2007", null, null,  null);
            }
            finally
            {
                Destroy(tbl, configuration, catalogue, tableInfo);
            }
        }


        /// <summary>
        /// A test which checks the behaviour of Aggregate Building when there is an axis, a pivot and a TopX in which the TopX selection is the 'Top 2 count column'
        /// This translates as 'identify the top 2 pivot values which have the highest counts matching the WHERE condition and pivot those categories only (for all data)'
        /// </summary>
        /// <param name="type"></param>
        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_PivotWithSum_Top2BasedonCountColumnDesc(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate pivot (and axis)
            AggregateDimension axisDimension;
            AggregateDimension pivotDimension;
            var configuration = SetupAggregateWithPivot(type, extractionInformations, catalogue, out axisDimension, out pivotDimension);

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category

            configuration.SaveToDatabase();

            var topx = new AggregateTopX(CatalogueRepository, configuration, 2);
            topx.OrderByDirection = AggregateTopXOrderByDirection.Descending;
            topx.SaveToDatabase();

            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(axisDimension);
                builder.AddColumn(pivotDimension);
                builder.SetPivotToDimensionID(pivotDimension);

                var resultTable = GetResultForBuilder(builder, tbl);

                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.IsTrue(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                Assert.AreEqual("T", resultTable.Columns[1].ColumnName);
                Assert.AreEqual("E", resultTable.Columns[2].ColumnName);

                //T,E,G - F does not appear because WHERE throws it out (both counts are below 42)
                VerifyRowExist(resultTable, "2000", null, null); //no records in 2000 but it is important it appears still because that is what the axis says
                VerifyRowExist(resultTable, "2001", 67, 37);
                VerifyRowExist(resultTable, "2002", 30, 41);
                VerifyRowExist(resultTable, "2003", 42, null);
                VerifyRowExist(resultTable, "2004", null, null);
                VerifyRowExist(resultTable, "2005", null, 59);
                VerifyRowExist(resultTable, "2006", null, null);
                VerifyRowExist(resultTable, "2007", null, null);
            }
            finally
            {
                Destroy(tbl, topx, configuration, catalogue, tableInfo);
            }
        }

        /// <summary>
        /// A test which checks the behaviour of Aggregate Building when there is an axis, a pivot and a TopX in which the TopX selection is the 'Top 2 count column'
        /// This translates as 'identify the top 2 pivot values which have the highest counts matching the WHERE condition and pivot those categories only (for all data)'
        /// </summary>
        /// <param name="type"></param>
        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_PivotWithSum_Top2AlphabeticalAsc_WHEREStatement(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate pivot (and axis)
            AggregateDimension axisDimension;
            AggregateDimension pivotDimension;
            var configuration = SetupAggregateWithPivot(type, extractionInformations, catalogue, out axisDimension, out pivotDimension);

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category
            configuration.SaveToDatabase();
            
            var topx = new AggregateTopX(CatalogueRepository, configuration, 2);
            topx.OrderByDirection = AggregateTopXOrderByDirection.Descending;
            topx.OrderByDimensionIfAny_ID = pivotDimension.ID;
            topx.OrderByDirection = AggregateTopXOrderByDirection.Ascending;
            topx.SaveToDatabase();

            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(axisDimension);
                builder.AddColumn(pivotDimension);
                builder.SetPivotToDimensionID(pivotDimension);

                AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder,type);

                var resultTable = GetResultForBuilder(builder, tbl);

                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.IsTrue(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                //sort in AggregateTopX is the pivot dimension asc (i.e. order alphabetically)
                Assert.AreEqual("E", resultTable.Columns[1].ColumnName);
                Assert.AreEqual("G", resultTable.Columns[2].ColumnName);

                //E,G (note that only 1 value appears for E because WHERE throws out rest).  Also note the two columns are E and G because that is Top 2 when alphabetically sorted of the pivot values (E,F,G,T) that match the filter (F doesn't)
                VerifyRowExist(resultTable, "2000", null, null); //no records in 2000 but it is important it appears still because that is what the axis says
                VerifyRowExist(resultTable, "2001", null, 53);
                VerifyRowExist(resultTable, "2002", null, null);
                VerifyRowExist(resultTable, "2003", null, null);
                VerifyRowExist(resultTable, "2004", null, null);
                VerifyRowExist(resultTable, "2005", 59, null);
                VerifyRowExist(resultTable, "2006", null, null);
                VerifyRowExist(resultTable, "2007", null, null);
            }
            finally
            {
                Destroy(tbl, topx, configuration, catalogue, tableInfo);
            }
        }

        /// <summary>
        /// Assemble an aggregate which returns the top 1 pivot dimension HAVING count(*) less than 2
        /// </summary>
        /// <param name="type"></param>
        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void GroupBy_PivotWithSum_HAVING_Top1_WHERE(DatabaseType type)
        {
            Catalogue catalogue;
            ExtractionInformation[] extractionInformations;
            TableInfo tableInfo;
            var tbl = UploadTestDataAsTableToServer(type, out catalogue, out extractionInformations, out tableInfo);

            //setup the aggregate pivot (and axis)
            AggregateDimension axisDimension;
            AggregateDimension pivotDimension;
            var configuration = SetupAggregateWithPivot(type, extractionInformations, catalogue, out axisDimension, out pivotDimension);

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category

            configuration.HavingSQL = "count(*)<5"; //throws out 'T'

            configuration.SaveToDatabase();

            var topx = new AggregateTopX(CatalogueRepository, configuration, 1); //Top 1 (highest count columns should be used for pivot)
            topx.OrderByDirection = AggregateTopXOrderByDirection.Descending;
            topx.SaveToDatabase();

            try
            {
                //get the result of the aggregate 
                var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
                builder.AddColumn(axisDimension);
                builder.AddColumn(pivotDimension);
                builder.SetPivotToDimensionID(pivotDimension);

                AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder,type);

                var resultTable = GetResultForBuilder(builder, tbl);

                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.IsTrue(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                //where logic matches T in spades but HAVING statement throws it out for having more than 4 records total
                Assert.AreEqual("E", resultTable.Columns[1].ColumnName);

                //Only E appears because of Top 1 pivot statement
                VerifyRowExist(resultTable, "2000", null); //all E records are discarded except 59 because that is the WHERE logic
                VerifyRowExist(resultTable, "2001", null);
                VerifyRowExist(resultTable, "2002", null);
                VerifyRowExist(resultTable, "2003", null);
                VerifyRowExist(resultTable, "2004", null);
                VerifyRowExist(resultTable, "2005", 59);
                VerifyRowExist(resultTable, "2006", null);
                VerifyRowExist(resultTable, "2007", null);
            }
            finally
            {
                Destroy(tbl, topx, configuration, catalogue, tableInfo);
            }
        }
    }
}
