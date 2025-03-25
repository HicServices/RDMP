// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Extensions;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases.Exceptions;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Swapping;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Components;

internal class ColumnSwapperTests : DatabaseTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void TestColumnSwapper_NormalUseCase(bool keepInputColumnToo)
    {
        using var dt = new DataTable();
        dt.Columns.Add("In");
        dt.Columns.Add("Out");

        dt.Rows.Add("A", 1);
        dt.Rows.Add("B", 2);
        dt.Rows.Add("C", 3);
        dt.Rows.Add("D", 4);
        dt.Rows.Add("D",
            5); //oh dear D maps to 2 out values that's a violation! but if we don't see a D it doesn't matter

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        Import(db.CreateTable("Map", dt), out _, out var mapCols);

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out")),
            KeepInputColumnToo = keepInputColumnToo
        };

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In");
        dtToSwap.Columns.Add("Name");
        dtToSwap.Columns.Add("Age");

        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("B", "Frank", 50);

        var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        //in should be there or not depending on the setting KeepInputColumnToo
        Assert.That(resultDt.Columns.Contains("In"), Is.EqualTo(keepInputColumnToo));

        AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
        Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));

        AreBasicallyEquals(1, resultDt.Rows[1]["Out"]);
        Assert.That(resultDt.Rows[1]["Name"], Is.EqualTo("Dave"));

        AreBasicallyEquals(2, resultDt.Rows[2]["Out"]);
        Assert.That(resultDt.Rows[2]["Name"], Is.EqualTo("Frank"));

        if (keepInputColumnToo)
            Assert.Multiple(() =>
            {
                Assert.That(resultDt.Rows[0]["In"], Is.EqualTo("A"));
                Assert.That(resultDt.Rows[1]["In"], Is.EqualTo("A"));
                Assert.That(resultDt.Rows[2]["In"], Is.EqualTo("B"));
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestColumnSwapper_AlternateColumnNames(bool keepInputColumnToo)
    {
        using var dtMap = new DataTable();
        dtMap.Columns.Add("In");
        dtMap.Columns.Add("Out");

        dtMap.Rows.Add("A", 1);
        dtMap.Rows.Add("B", 2);
        dtMap.Rows.Add("C", 3);
        dtMap.Rows.Add("D", 4);
        dtMap.Rows.Add("D",
            5); //oh dear D maps to 2 out values that's a violation! but if we don't see a D it doesn't matter

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        Import(db.CreateTable("Map", dtMap), out _, out var mapCols);

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out")),
            KeepInputColumnToo = keepInputColumnToo
        };

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In2");
        dtToSwap.Columns.Add("Name");
        dtToSwap.Columns.Add("Age");

        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("B", "Frank", 50);

        // Our pipeline data does not have a column called In but instead it is called In2
        var ex = Assert.Throws<Exception>(() => swapper.ProcessPipelineData(dtToSwap,
            ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
        Assert.That(ex.Message, Is.EqualTo("DataTable did not contain a field called 'In'"));

        // Tell the swapper about the new name
        swapper.InputFromColumn = "In2";
        swapper.OutputToColumn = "Out2";

        var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        //in should be there or not depending on the setting KeepInputColumnToo
        Assert.That(resultDt.Columns.Contains("In2"), Is.EqualTo(keepInputColumnToo));

        AreBasicallyEquals(1, resultDt.Rows[0]["Out2"]);
        Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));

        AreBasicallyEquals(1, resultDt.Rows[1]["Out2"]);
        Assert.That(resultDt.Rows[1]["Name"], Is.EqualTo("Dave"));

        AreBasicallyEquals(2, resultDt.Rows[2]["Out2"]);
        Assert.That(resultDt.Rows[2]["Name"], Is.EqualTo("Frank"));

        if (keepInputColumnToo)
            Assert.Multiple(() =>
            {
                Assert.That(resultDt.Rows[0]["In2"], Is.EqualTo("A"));
                Assert.That(resultDt.Rows[1]["In2"], Is.EqualTo("A"));
                Assert.That(resultDt.Rows[2]["In2"], Is.EqualTo("B"));
            });
    }


    [TestCase(true)]
    [TestCase(false)]
    public void TestColumnSwapper_InPlaceSwapNoNewCols(bool keepInputColumnToo)
    {
        using var dtMap = new DataTable();
        dtMap.Columns.Add("In");
        dtMap.Columns.Add("Out");

        dtMap.Rows.Add("A", 1);
        dtMap.Rows.Add("B", 2);
        dtMap.Rows.Add("C", 3);
        dtMap.Rows.Add("D", 4);
        dtMap.Rows.Add("D",
            5); //oh dear D maps to 2 out values that's a violation! but if we don't see a D it doesn't matter

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        Import(db.CreateTable("Map", dtMap), out _, out var mapCols);

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out")),
            KeepInputColumnToo = keepInputColumnToo
        };

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In2");
        dtToSwap.Columns.Add("Name");
        dtToSwap.Columns.Add("Age");

        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("B", "Frank", 50);

        // Tell the swapper about the new name
        swapper.InputFromColumn = "In2";
        swapper.OutputToColumn = "In2";

        var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        // in ALWAYS be there, because it is an in place update - ignore KeepInputColumnToo
        Assert.That(resultDt.Columns.Contains("In2"));

        AreBasicallyEquals(1, resultDt.Rows[0]["In2"]);
        Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));

        AreBasicallyEquals(1, resultDt.Rows[1]["In2"]);
        Assert.That(resultDt.Rows[1]["Name"], Is.EqualTo("Dave"));

        AreBasicallyEquals(2, resultDt.Rows[2]["In2"]);
        Assert.That(resultDt.Rows[2]["Name"], Is.EqualTo("Frank"));
    }

    [TestCase(AliasResolutionStrategy.CrashIfAliasesFound)]
    [TestCase(AliasResolutionStrategy.MultiplyInputDataRowsByAliases)]
    public void TestColumnSwapper_Aliases(AliasResolutionStrategy strategy)
    {
        using var dt = new DataTable();
        dt.Columns.Add("In");
        dt.Columns.Add("Out");

        dt.Rows.Add("A", 1);
        dt.Rows.Add("B", 2);
        dt.Rows.Add("C", 3);
        dt.Rows.Add("D", 4);
        dt.Rows.Add("D",
            5); //oh dear D maps to 2 out values that's a violation! but if we don't see a D it doesn't matter

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        Import(db.CreateTable("Map", dt), out _, out var mapCols);

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out")),
            AliasResolutionStrategy = strategy
        };

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In");
        dtToSwap.Columns.Add("Name");
        dtToSwap.Columns.Add("Age");

        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("D", "Dandy", 60);

        switch (strategy)
        {
            case AliasResolutionStrategy.CrashIfAliasesFound:
                Assert.Throws<AliasException>(() => swapper.ProcessPipelineData(dtToSwap,
                    ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
                break;
            case AliasResolutionStrategy.MultiplyInputDataRowsByAliases:

                var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
                    new GracefulCancellationToken());

                AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
                Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));

                //we get the first alias (4)
                AreBasicallyEquals(4, resultDt.Rows[1]["Out"]);
                Assert.That(resultDt.Rows[1]["Name"], Is.EqualTo("Dandy"));
                AreBasicallyEquals(60, resultDt.Rows[1]["Age"]);

                //and the second alias (5)
                AreBasicallyEquals(5, resultDt.Rows[2]["Out"]);
                Assert.That(resultDt.Rows[2]["Name"], Is.EqualTo("Dandy"));
                AreBasicallyEquals(60, resultDt.Rows[1]["Age"]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestColumnSwapper_MissingMappings(bool crashIfNoMappingsFound)
    {
        using var dt = new DataTable();
        dt.Columns.Add("In");
        dt.Columns.Add("Out");

        dt.Rows.Add("A", 1);
        dt.Rows.Add("B", 2);
        dt.Rows.Add("C", 3);
        dt.Rows.Add("D", 4);
        dt.Rows.Add("D",
            5); //oh dear D maps to 2 out values that's a violation! but if we don't see a D it doesn't matter

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        Import(db.CreateTable("Map", dt), out _, out var mapCols);

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out")),
            CrashIfNoMappingsFound = crashIfNoMappingsFound
        };
        swapper.WHERELogic = $"{swapper.MappingToColumn.GetFullyQualifiedName()} < 2"; //throws out all rows but A

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In");
        dtToSwap.Columns.Add("Name");
        dtToSwap.Columns.Add("Age");

        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("B", "Frank", 50);

        if (crashIfNoMappingsFound)
        {
            Assert.Throws<KeyNotFoundException>(() =>
                swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet, null));
        }
        else
        {
            var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
                new GracefulCancellationToken());

            Assert.That(resultDt.Rows, Has.Count.EqualTo(1));
            AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
            Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));
        }
    }

    [Test]
    public void TestColumnSwapper_ProjectSpecificMappings()
    {
        using var dt = new DataTable();
        dt.Columns.Add("In");
        dt.Columns.Add("Out");
        dt.Columns.Add("Proj");

        //Anonymise A and B differently depending on ProjectNumber (valid project numbers are 1 and 2)
        dt.Rows.Add("A", 1, 1);
        dt.Rows.Add("A", 2, 2);
        dt.Rows.Add("B", 3, 1);
        dt.Rows.Add("B", 4, 2);

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        Import(db.CreateTable("Map", dt), out _, out var mapCols);

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out")),
            WHERELogic = "Proj = $n"
        };

        // initialize with a mock that returns ProjectNumber 1
        swapper.PreInitialize(GetMockExtractDatasetCommand(), ThrowImmediatelyDataLoadEventListener.Quiet);

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In");
        dtToSwap.Columns.Add("Name");
        dtToSwap.Columns.Add("Age");

        dtToSwap.Rows.Add("A", "Dave", 30);
        dtToSwap.Rows.Add("B", "Frank", 50);

        using var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        Assert.That(resultDt.Rows, Has.Count.EqualTo(2));

        // Should have project specific results for A of 1 and for B of 3 because the ProjectNumber is 1
        AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
        Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));
        AreBasicallyEquals(3, resultDt.Rows[1]["Out"]);
        Assert.That(resultDt.Rows[1]["Name"], Is.EqualTo("Frank"));
    }

    /// <summary>
    ///     Tests ColumnSwapper when there are null values in the input <see cref="DataTable" /> being processed
    /// </summary>
    [Test]
    public void TestColumnSwapper_InputTableNulls()
    {
        using var dt = new DataTable();
        dt.Columns.Add("In");
        dt.Columns.Add("Out");

        dt.Rows.Add(1, 1);
        dt.Rows.Add(2, 2);

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        Import(db.CreateTable("Map", dt), out _, out var mapCols);

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out"))
        };

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In", typeof(int));
        dtToSwap.Columns.Add("Name");
        dtToSwap.Columns.Add("Age");

        dtToSwap.Rows.Add(1, "Dave", 30);
        dtToSwap.Rows.Add(null, "Bob", 30);

        var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        Assert.That(resultDt.Rows, Has.Count.EqualTo(2));
        AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
        Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));

        AreBasicallyEquals(DBNull.Value, resultDt.Rows[1]["Out"]);
        Assert.That(resultDt.Rows[1]["Name"], Is.EqualTo("Bob"));
    }

    /// <summary>
    ///     Tests ColumnSwapper when there are null values in the database mapping table
    /// </summary>
    [Test]
    public void TestColumnSwapper_MappingTableNulls()
    {
        using var dt = new DataTable();
        dt.Columns.Add("In");
        dt.Columns.Add("Out");

        dt.Rows.Add(1, 1);
        dt.Rows.Add(DBNull.Value, 3); // this value should be ignored
        dt.Rows.Add(2, 2);

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        Import(db.CreateTable("Map", dt), out _, out var mapCols);

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out"))
        };

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In", typeof(int));
        dtToSwap.Columns.Add("Name");
        dtToSwap.Columns.Add("Age");

        dtToSwap.Rows.Add(1, "Dave", 30);
        dtToSwap.Rows.Add(null, "Bob", 30);

        var toMem = new ToMemoryDataLoadEventListener(true);

        var resultDt = swapper.ProcessPipelineData(dtToSwap, toMem, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            //this is the primary thing we are testing here
            Assert.That(
                toMem.GetAllMessagesByProgressEventType()[ProgressEventType.Warning].Select(m => m.Message).ToArray(),
                Does.Contain("Discarded 1 Null key values read from mapping table"));

            Assert.That(resultDt.Rows, Has.Count.EqualTo(2));
        });
        AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
        Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));

        AreBasicallyEquals(DBNull.Value, resultDt.Rows[1]["Out"]);
        Assert.That(resultDt.Rows[1]["Name"], Is.EqualTo("Bob"));
    }

    /// <summary>
    ///     Tests the systems ability to compare an integer in the input data table with a string in the database
    /// </summary>
    [Test]
    public void TestColumnSwapper_MixedDatatypes_StringInDatabase()
    {
        using var dt = new DataTable();
        dt.Columns.Add("In");
        dt.Columns.Add("Out");

        dt.Rows.Add("1" /*string*/, 2);
        dt.Rows.Add("2", 3);
        dt.Rows.Add("3", 4);
        dt.SetDoNotReType(true);

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        DiscoveredTable mapTbl;

        Import(mapTbl = db.CreateTable("Map", dt), out _, out var mapCols);

        Assert.That(mapTbl.DiscoverColumn("In").DataType.GetCSharpDataType(), Is.EqualTo(typeof(string)),
            "Expected map to be of string datatype");

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out"))
        };

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In");
        dtToSwap.Columns.Add("Name");
        dtToSwap.Rows.Add(1 /*int*/, "Dave");

        var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        Assert.That(resultDt.Rows, Has.Count.EqualTo(1));
        AreBasicallyEquals(2, resultDt.Rows[0]["Out"]);
        Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));
    }


    /// <summary>
    ///     Tests the systems ability to compare a string input data table with an integer in the database
    /// </summary>
    [Test]
    public void TestColumnSwapper_MixedDatatypes_IntegerInDatabase()
    {
        using var dt = new DataTable();
        dt.Columns.Add("In");
        dt.Columns.Add("Out");

        dt.Rows.Add(1 /*int*/, 2);
        dt.Rows.Add(2, 3);
        dt.Rows.Add(3, 4);

        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        DiscoveredTable mapTbl;

        Import(mapTbl = db.CreateTable("Map", dt), out _, out var mapCols);

        Assert.That(mapTbl.DiscoverColumn("In").DataType.GetCSharpDataType(), Is.EqualTo(typeof(int)),
            "Expected map to be of int datatype");

        var swapper = new ColumnSwapper
        {
            MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In")),
            MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out"))
        };

        swapper.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var dtToSwap = new DataTable();

        dtToSwap.Columns.Add("In");
        dtToSwap.Columns.Add("Name");
        dtToSwap.Rows.Add("1" /*string*/, "Dave");

        var resultDt = swapper.ProcessPipelineData(dtToSwap, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        Assert.That(resultDt.Rows, Has.Count.EqualTo(1));
        AreBasicallyEquals(2, resultDt.Rows[0]["Out"]);
        Assert.That(resultDt.Rows[0]["Name"], Is.EqualTo("Dave"));
    }

    private static IExtractDatasetCommand GetMockExtractDatasetCommand()
    {
        var mockPj = Substitute.For<IProject>();
        mockPj.Name.Returns("My Project");
        mockPj.ProjectNumber.Returns(1);

        var mockConfig = Substitute.For<IExtractionConfiguration>();
        mockConfig.Project.Returns(mockPj);

        var mockSelectedDatasets = Substitute.For<ISelectedDataSets>();
        mockSelectedDatasets.ExtractionConfiguration.Returns(mockConfig);


        var mockExtractDsCmd = Substitute.For<IExtractDatasetCommand>();
        mockExtractDsCmd.Project.Returns(mockPj);
        mockExtractDsCmd.Configuration.Returns(mockConfig);
        mockExtractDsCmd.SelectedDataSets.Returns(mockSelectedDatasets);

        return mockExtractDsCmd;
    }
}