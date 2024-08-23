// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Discovery.QuerySyntax.Aggregation;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.Curation.Integration.QueryBuildingTests.AggregateBuilderTests;

public class AggregateDataBasedTests : DatabaseTests
{
    private static DataTable GetTestDataTable()
    {
        var dt = new DataTable
        {
            TableName = "AggregateDataBasedTests"
        };

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

        dt.Rows.Add("2001-01-01", "E&, %a' mp;E", "37");
        dt.Rows.Add("2002-01-01", "E&, %a' mp;E", "41");
        dt.Rows.Add("2005-01-01", "E&, %a' mp;E",
            "59"); //note there are no records in 2004 it is important for axis tests (axis involves you having to build a calendar table)

        dt.Rows.Add(null, "G", "47");
        dt.Rows.Add("2001-01-01", "G", "53");

        return dt;
    }

    #region Helper methods

    private DiscoveredTable UploadTestDataAsTableToServer(DatabaseType type, out ICatalogue catalogue,
        out ExtractionInformation[] extractionInformations, out ITableInfo tableinfo)
    {
        var listener = ThrowImmediatelyDataLoadEventListener.Quiet;

        var db = GetCleanedServer(type);

        var data = GetTestDataTable();

        var uploader = new DataTableUploadDestination();
        uploader.PreInitialize(db, listener);
        uploader.ProcessPipelineData(data, listener, new GracefulCancellationToken());
        uploader.Dispose(listener, null);
        var tbl = db.ExpectTable(uploader.TargetTableName);

        Assert.That(tbl.Exists());

        catalogue = Import(tbl, out tableinfo, out _, out _, out extractionInformations);

        return tbl;
    }

    private static void Destroy(DiscoveredTable tbl, params IDeleteable[] deletablesInOrderOfDeletion)
    {
        tbl.Drop();
        foreach (var deleteable in deletablesInOrderOfDeletion)
            deleteable.DeleteInDatabase();
    }

    private static DataTable GetResultForBuilder(AggregateBuilder builder, DiscoveredTable tbl)
    {
        var sql = builder.SQL;

        using var con = tbl.Database.Server.GetConnection();
        con.Open();
        var da = tbl.Database.Server.GetDataAdapter(sql, con);
        var toReturn = new DataTable();
        da.Fill(toReturn);

        return toReturn;
    }



    private static void AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(AggregateBuilder builder, DatabaseType type)
    {
        var syntaxHelper = QuerySyntaxHelperFactory.Create(type);
        var declaration = syntaxHelper.GetParameterDeclaration("@category", new DatabaseTypeRequest(typeof(string), 1));

        var repo = new MemoryCatalogueRepository();

        var ORContainer = new SpontaneouslyInventedFilterContainer(repo, null, null, FilterContainerOperation.OR);
        var constParam = new ConstantParameter(declaration, "'T'", "T Category Only", syntaxHelper);

        //this is deliberately duplication, it tests that the parameter compiles as well as that any dynamic sql doesn't get thrown by quotes
        var filter1 = new SpontaneouslyInventedFilter(repo, ORContainer, "(Category=@category OR Category = 'T')",
            "Category Is @category",
            "ensures the records belong to the category @category", new ISqlParameter[] { constParam });
        var filter2 = new SpontaneouslyInventedFilter(repo, ORContainer, "NumberInTrouble > 42",
            "number in trouble greater than 42", "See above", null);

        ORContainer.AddChild(filter1);
        ORContainer.AddChild(filter2);

        builder.RootFilterContainer = ORContainer;
    }

    private AggregateConfiguration SetupAggregateWithAxis(ExtractionInformation[] extractionInformations,
        ICatalogue catalogue, out AggregateDimension axisDimension)
    {
        var dateDimension =
            extractionInformations.Single(
                e => e.GetRuntimeName().Equals("EventDate", StringComparison.CurrentCultureIgnoreCase));
        var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
        axisDimension = new AggregateDimension(CatalogueRepository, dateDimension, configuration);

        var axis = new AggregateContinuousDateAxis(CatalogueRepository, axisDimension)
        {
            StartDate = "'2000-01-01'",
            AxisIncrement = AxisIncrement.Year
        };
        axis.SaveToDatabase();
        return configuration;
    }

    private AggregateConfiguration SetupAggregateWithPivot(ExtractionInformation[] extractionInformations,
        ICatalogue catalogue, out AggregateDimension axisDimension, out AggregateDimension pivotDimension)
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

        var axis = new AggregateContinuousDateAxis(CatalogueRepository, axisDimension)
        {
            StartDate = "'2000-01-01'",
            AxisIncrement = AxisIncrement.Year
        };
        axis.SaveToDatabase();
        return configuration;
    }

    #endregion

    [Test]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    //[TestCase(DatabaseType.Oracle)]// doesn't quite work yet :) needs full implementation of database abstraction layer for Oracle to work
    public void Count_CorrectNumberOfRowsCalculated(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        var builder = new AggregateBuilder(null, "count(*)", null, new[] { tableInfo });
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
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_CategoryWithCount_Correct(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate
        var categoryDimension = extractionInformations.Single(e =>
            e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));
        var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
        var dimension = new AggregateDimension(CatalogueRepository, categoryDimension, configuration);

        try
        {
            //get the result of the aggregate
            var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
            builder.AddColumn(dimension);
            var resultTable = GetResultForBuilder(builder, tbl);

            VerifyRowExist(resultTable, "T", 7);
            VerifyRowExist(resultTable, "F", 2);
            VerifyRowExist(resultTable, "E&, %a' mp;E", 3);
            VerifyRowExist(resultTable, "G", 2);
        }
        finally
        {
            Destroy(tbl, configuration, catalogue, tableInfo);
        }
    }

    [Test]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_CategoryWithSum_Correct(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate
        var categoryDimension = extractionInformations.Single(e =>
            e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));
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
            VerifyRowExist(resultTable, "E&, %a' mp;E", 137);
            VerifyRowExist(resultTable, "G", 100);
            Assert.That(resultTable.Rows, Has.Count.EqualTo(4));
        }
        finally
        {
            Destroy(tbl, configuration, catalogue, tableInfo);
        }
    }

    [Test]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_CategoryWithSum_WHEREStatement(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate
        var categoryDimension = extractionInformations.Single(e =>
            e.GetRuntimeName().Equals("Category", StringComparison.CurrentCultureIgnoreCase));
        var configuration = new AggregateConfiguration(CatalogueRepository, catalogue, "GroupBy_Category");
        var dimension = new AggregateDimension(CatalogueRepository, categoryDimension, configuration);

        configuration.CountSQL = "sum(NumberInTrouble)";
        configuration.SaveToDatabase();

        try
        {
            //get the result of the aggregate
            var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
            builder.AddColumn(dimension);

            AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder, type);

            var resultTable = GetResultForBuilder(builder, tbl);

            //T is matched on all records so they are summed
            VerifyRowExist(resultTable, "T", 139);
            //VerifyRowExist(resultTable, "F", 60); //F does not have any records over 42 and isn't T so shouldnt be matched
            VerifyRowExist(resultTable, "E&, %a' mp;E", 59); //E has 1 records over 42
            VerifyRowExist(resultTable, "G", 100); //47 + 53
            Assert.That(resultTable.Rows, Has.Count.EqualTo(3));
        }
        finally
        {
            Destroy(tbl, configuration, catalogue, tableInfo);
        }
    }

    [Test]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_AxisWithSum_Correct(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate with axis
        var configuration = SetupAggregateWithAxis(extractionInformations, catalogue, out var dimension);

        configuration.CountSQL = "sum(NumberInTrouble)";
        configuration.SaveToDatabase();

        try
        {
            //get the result of the aggregate
            var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
            builder.AddColumn(dimension);

            var resultTable = GetResultForBuilder(builder, tbl);

            //axis is ordered ascending by date starting in 2000 so that row should come first
            Assert.That(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

            VerifyRowExist(resultTable, "2000",
                null); //because it is a SUM the ANSI return should be null not 0 since it is a sum of no records
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
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_AxisWithCount_HAVING(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate with axis
        var configuration = SetupAggregateWithAxis(extractionInformations, catalogue, out var dimension);

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
            Assert.That(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

            VerifyRowExist(resultTable, "2000",
                null); //records only showing where there are more than 3 records (HAVING refers to the year since there's no pivot)
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
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_AxisWithCount_WHERECorrect(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate with axis
        var configuration = SetupAggregateWithAxis(extractionInformations, catalogue, out var dimension);

        configuration.CountSQL = "count(NumberInTrouble)";
        configuration.SaveToDatabase();
        try
        {
            //get the result of the aggregate
            var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
            builder.AddColumn(dimension);

            AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder, type);

            var resultTable = GetResultForBuilder(builder, tbl);

            //axis is ordered ascending by date starting in 2000 so that row should come first
            Assert.That(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

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
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_PivotWithSum_Correct(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate pivot (and axis)
        var configuration = SetupAggregateWithPivot(extractionInformations, catalogue, out var axisDimension,
            out var pivotDimension);

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

            Assert.Multiple(() =>
            {
                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.That(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                Assert.That(resultTable.Columns[1].ColumnName, Is.EqualTo("T"));
                Assert.That(resultTable.Columns[2].ColumnName, Is.EqualTo("E&, %a' mp;E"));
                Assert.That(resultTable.Columns[3].ColumnName, Is.EqualTo("F"));
                Assert.That(resultTable.Columns[4].ColumnName, Is.EqualTo("G"));
            });

            //T,E,F,G
            VerifyRowExist(resultTable, "2000", null, null, null,
                null); //no records in 2000 but it is important it appears still because that is what the axis says
            VerifyRowExist(resultTable, "2001", 67, 37, null, 53);
            VerifyRowExist(resultTable, "2002", 30, 41, 60, null);
            VerifyRowExist(resultTable, "2003", 42, null, null, null);
            VerifyRowExist(resultTable, "2004", null, null, null, null);
            VerifyRowExist(resultTable, "2005", null, 59, null, null);
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
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_PivotWithSum_WHEREStatement(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate pivot (and axis)
        var configuration = SetupAggregateWithPivot(extractionInformations, catalogue, out var axisDimension,
            out var pivotDimension);

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

            AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder, type);

            var resultTable = GetResultForBuilder(builder, tbl);

            Assert.Multiple(() =>
            {
                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.That(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                Assert.That(resultTable.Columns[1].ColumnName, Is.EqualTo("T"));
                Assert.That(resultTable.Columns[2].ColumnName, Is.EqualTo("E&, %a' mp;E"));
                Assert.That(resultTable.Columns[3].ColumnName, Is.EqualTo("G"));
            });

            //T,E,G - F does not appear because WHERE throws it out (both counts are below 42)
            VerifyRowExist(resultTable, "2000", null, null,
                null); //no records in 2000 but it is important it appears still because that is what the axis says
            VerifyRowExist(resultTable, "2001", 67, null, 53);
            VerifyRowExist(resultTable, "2002", 30, null, null);
            VerifyRowExist(resultTable, "2003", 42, null, null);
            VerifyRowExist(resultTable, "2004", null, null, null);
            VerifyRowExist(resultTable, "2005", null, 59, null);
            VerifyRowExist(resultTable, "2006", null, null, null);
            VerifyRowExist(resultTable, "2007", null, null, null);
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
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_PivotWithSum_Top2BasedonCountColumnDesc(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate pivot (and axis)
        var configuration = SetupAggregateWithPivot(extractionInformations, catalogue, out var axisDimension,
            out var pivotDimension);

        configuration.CountSQL = "sum(NumberInTrouble)";
        configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category

        configuration.SaveToDatabase();

        var topx = new AggregateTopX(CatalogueRepository, configuration, 2)
        {
            OrderByDirection = AggregateTopXOrderByDirection.Descending
        };
        topx.SaveToDatabase();

        try
        {
            //get the result of the aggregate
            var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
            builder.AddColumn(axisDimension);
            builder.AddColumn(pivotDimension);
            builder.SetPivotToDimensionID(pivotDimension);

            var resultTable = GetResultForBuilder(builder, tbl);

            Assert.Multiple(() =>
            {
                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.That(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                Assert.That(resultTable.Columns[1].ColumnName, Is.EqualTo("T"));
                Assert.That(resultTable.Columns[2].ColumnName, Is.EqualTo("E&, %a' mp;E"));
            });

            //T,E,G - F does not appear because WHERE throws it out (both counts are below 42)
            VerifyRowExist(resultTable, "2000", null,
                null); //no records in 2000 but it is important it appears still because that is what the axis says
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
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_PivotWithSum_Top2AlphabeticalAsc_WHEREStatement(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate pivot (and axis)
        var configuration = SetupAggregateWithPivot(extractionInformations, catalogue, out var axisDimension,
            out var pivotDimension);

        configuration.CountSQL = "sum(NumberInTrouble)";
        configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category
        configuration.SaveToDatabase();

        var topx = new AggregateTopX(CatalogueRepository, configuration, 2)
        {
            OrderByDirection = AggregateTopXOrderByDirection.Descending,
            OrderByDimensionIfAny_ID = pivotDimension.ID
        };
        topx.OrderByDirection = AggregateTopXOrderByDirection.Ascending;
        topx.SaveToDatabase();

        try
        {
            //get the result of the aggregate
            var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
            builder.AddColumn(axisDimension);
            builder.AddColumn(pivotDimension);
            builder.SetPivotToDimensionID(pivotDimension);

            AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder, type);

            var resultTable = GetResultForBuilder(builder, tbl);

            Assert.Multiple(() =>
            {
                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.That(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                //sort in AggregateTopX is the pivot dimension asc (i.e. order alphabetically)
                Assert.That(resultTable.Columns[1].ColumnName, Is.EqualTo("E&, %a' mp;E"));
                Assert.That(resultTable.Columns[2].ColumnName, Is.EqualTo("G"));
            });

            //E,G (note that only 1 value appears for E because WHERE throws out rest).  Also note the two columns are E and G because that is Top 2 when alphabetically sorted of the pivot values (E,F,G,T) that match the filter (F doesn't)
            VerifyRowExist(resultTable, "2000", null,
                null); //no records in 2000 but it is important it appears still because that is what the axis says
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
    [TestCase(DatabaseType.MySql)]
    public void GroupBy_PivotWithSum_HAVING_Top1_WHERE(DatabaseType type)
    {
        var tbl = UploadTestDataAsTableToServer(type, out var catalogue, out var extractionInformations,
            out var tableInfo);

        //setup the aggregate pivot (and axis)
        var configuration = SetupAggregateWithPivot(extractionInformations, catalogue, out var axisDimension,
            out var pivotDimension);

        configuration.CountSQL = "sum(NumberInTrouble)";
        configuration.PivotOnDimensionID = pivotDimension.ID; //pivot on the Category

        configuration.HavingSQL = "count(*)<5"; //throws out 'T'

        configuration.SaveToDatabase();

        var topx = new AggregateTopX(CatalogueRepository, configuration, 1)
        {
            OrderByDirection = AggregateTopXOrderByDirection.Descending
        }; //Top 1 (highest count columns should be used for pivot)
        topx.SaveToDatabase();

        try
        {
            //get the result of the aggregate
            var builder = new AggregateBuilder(null, configuration.CountSQL, configuration);
            builder.AddColumn(axisDimension);
            builder.AddColumn(pivotDimension);
            builder.SetPivotToDimensionID(pivotDimension);

            AddWHEREToBuilder_CategoryIsTOrNumberGreaterThan42(builder, type);

            var resultTable = GetResultForBuilder(builder, tbl);

            Assert.Multiple(() =>
            {
                //axis is ordered ascending by date starting in 2000 so that row should come first
                Assert.That(AreBasicallyEquals("2000", resultTable.Rows[0][0]));

                //where logic matches T in spades but HAVING statement throws it out for having more than 4 records total
                Assert.That(resultTable.Columns[1].ColumnName, Is.EqualTo("E&, %a' mp;E"));
            });

            //Only E appears because of Top 1 pivot statement
            VerifyRowExist(resultTable, "2000",
                null); //all E records are discarded except 59 because that is the WHERE logic
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