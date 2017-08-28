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

            //get the result of the aggregate 
            var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
            builder.AddColumn(dimension);
            var resultTable = GetResultForBuilder(builder, tbl);

            VerifyRowExist(resultTable, "T", 139);
            VerifyRowExist(resultTable, "F", 60);
            VerifyRowExist(resultTable, "E", 137);
            VerifyRowExist(resultTable, "G", 100);
            Assert.AreEqual(4,resultTable.Rows.Count);

            Destroy(tbl, configuration, catalogue, tableInfo);
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

            Destroy(tbl, configuration, catalogue, tableInfo);
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

            //setup the aggregate
            var dateDimension = extractionInformations.Single(e => e.GetRuntimeName().Equals("EventDate", StringComparison.CurrentCultureIgnoreCase));
            var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
            var dimension = new AggregateDimension(CatalogueRepository, dateDimension, configuration);

            var syntaxHelper = new QuerySyntaxHelperFactory().Create(type);

            var axis = new AggregateContinuousDateAxis(CatalogueRepository, dimension);
            axis.StartDate = "'2000-01-01'";
            axis.EndDate = syntaxHelper.GetScalarFunctionSql(MandatoryScalarFunctions.GetTodaysDate);
            axis.AxisIncrement = AxisIncrement.Year;
            axis.SaveToDatabase();

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.SaveToDatabase();

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

            Destroy(tbl, configuration, catalogue, tableInfo);
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

            //setup the aggregate
            var dateDimension = extractionInformations.Single(e => e.GetRuntimeName().Equals("EventDate", StringComparison.CurrentCultureIgnoreCase));
            var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
            var dimension = new AggregateDimension(CatalogueRepository, dateDimension, configuration);
            
            var axis = new AggregateContinuousDateAxis(CatalogueRepository, dimension);
            axis.StartDate = "'2000-01-01'";
            axis.EndDate = "'2003-01-01'";
            axis.AxisIncrement = AxisIncrement.Year;
            axis.SaveToDatabase();

            configuration.CountSQL = "count(NumberInTrouble)";
            configuration.SaveToDatabase();

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

            Destroy(tbl, configuration, catalogue, tableInfo);
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

            //setup the aggregate
            var axisCol = extractionInformations.Single(e => e.GetRuntimeName().Equals("EventDate", StringComparison.CurrentCultureIgnoreCase));
            var categoryCol = extractionInformations.Single(e => e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));


            var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
            var axisDimension = new AggregateDimension(CatalogueRepository, axisCol, configuration);
            var pivotDimension = new AggregateDimension(CatalogueRepository, categoryCol, configuration);

            var syntaxHelper = new QuerySyntaxHelperFactory().Create(type);

            var axis = new AggregateContinuousDateAxis(CatalogueRepository, axisDimension);
            axis.StartDate = "'2000-01-01'";
            axis.EndDate = syntaxHelper.GetScalarFunctionSql(MandatoryScalarFunctions.GetTodaysDate);
            axis.AxisIncrement = AxisIncrement.Year;
            axis.SaveToDatabase();

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category

            configuration.SaveToDatabase();

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

            //T,F,E,G
            VerifyRowExist(resultTable, "2000", null,null,null,null); //because it is a SUM the ANSI return should be null not 0 since it is a sum of no records
            VerifyRowExist(resultTable, "2001", 67, 37, null, 53);
            VerifyRowExist(resultTable, "2002", 30, 41, 60, null);
            VerifyRowExist(resultTable, "2003", 42, null, null, null);
            VerifyRowExist(resultTable, "2004", null, null, null, null);
            VerifyRowExist(resultTable, "2005", null, 59,null , null);
            VerifyRowExist(resultTable, "2006", null, null, null, null);
            VerifyRowExist(resultTable, "2007", null, null, null, null);

            Destroy(tbl, configuration, catalogue, tableInfo);
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

            //setup the aggregate
            var axisCol = extractionInformations.Single(e => e.GetRuntimeName().Equals("EventDate", StringComparison.CurrentCultureIgnoreCase));
            var categoryCol = extractionInformations.Single(e => e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));


            var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
            var axisDimension = new AggregateDimension(CatalogueRepository, axisCol, configuration);
            var pivotDimension = new AggregateDimension(CatalogueRepository, categoryCol, configuration);

            var syntaxHelper = new QuerySyntaxHelperFactory().Create(type);

            var axis = new AggregateContinuousDateAxis(CatalogueRepository, axisDimension);
            axis.StartDate = "'2000-01-01'";
            axis.EndDate = syntaxHelper.GetScalarFunctionSql(MandatoryScalarFunctions.GetTodaysDate);
            axis.AxisIncrement = AxisIncrement.Year;
            axis.SaveToDatabase();

            configuration.CountSQL = "sum(NumberInTrouble)";
            configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category

            configuration.SaveToDatabase();

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

            //T,F,E,G
            VerifyRowExist(resultTable, "2000", null, null, null); //because it is a SUM the ANSI return should be null not 0 since it is a sum of no records
            VerifyRowExist(resultTable, "2001", 67, null, 53);
            VerifyRowExist(resultTable, "2002", 30, null, null);
            VerifyRowExist(resultTable, "2003", 42, null, null);
            VerifyRowExist(resultTable, "2004", null, null, null);
            VerifyRowExist(resultTable, "2005", null, 59,  null);
            VerifyRowExist(resultTable, "2006", null, null, null);
            VerifyRowExist(resultTable, "2007", null, null,  null);

            Destroy(tbl, configuration, catalogue, tableInfo);
        }
    }
}
