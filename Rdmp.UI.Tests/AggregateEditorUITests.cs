// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.UI.AggregationUIs.Advanced;

namespace Rdmp.UI.Tests;

internal class AggregateEditorUITests : UITests
{
    [Test]
    [UITimeout(50000)]
    public void Test_AggregateEditorUI_NormalState()
    {
        var config = GetAggregateConfigurationWithNoDimensions();
        var ui = AndLaunch<AggregateEditorUI>(config);

        //The selected columns ui
        var colsUi = ui.selectColumnUI1;

        //should show two available columns
        var available = colsUi.AvailableColumns;
        Assert.AreEqual(2, available.Count);

        //the count(*) column
        var included = colsUi.IncludedColumns;
        Assert.AreEqual(1, included.Count);

        //before we have added any columns it should not be possible to launch the graph
        var cmdExecuteGraph = new ExecuteCommandExecuteAggregateGraph(ItemActivator, config);
        Assert.IsTrue(cmdExecuteGraph.IsImpossible);
        StringAssert.Contains("No tables could be identified for the query.  Try adding a column or a force join",
            cmdExecuteGraph.ReasonCommandImpossible);

        var ei = config.Catalogue.CatalogueItems[0].ExtractionInformation;

        //create a new dimension to the config in the database
        var dim = new AggregateDimension(Repository, ei, config);

        //publish a refresh
        Publish(config);

        //should show one available columns
        available = colsUi.AvailableColumns;
        Assert.AreEqual(1, available.Count);

        //the count(*) column and the added dimension
        included = colsUi.IncludedColumns;
        Assert.AreEqual(2, included.Count);

        //should now be possible to launch the graph
        cmdExecuteGraph = new ExecuteCommandExecuteAggregateGraph(ItemActivator, config);
        Assert.IsFalse(cmdExecuteGraph.IsImpossible);

        AssertNoErrors(ExpectedErrorType.Any);
    }

    [Test]
    [UITimeout(50000)]
    public void Test_AggregateEditorUI_AxisOnlyShowsDateDimensions()
    {
        var config = GetAggregateConfigurationWithNoDimensions(out var dateEi, out var otherEi);

        var dimDate = new AggregateDimension(Repository, dateEi, config);
        var dimOther = new AggregateDimension(Repository, otherEi, config);
        config.ClearAllInjections();

        var ui = AndLaunch<AggregateEditorUI>(config);

        //only date should be an option for axis dimension
        Assert.AreEqual(1, ui.ddAxisDimension.Items.Count);
        Assert.AreEqual(dimDate, ui.ddAxisDimension.Items[0]);

        //dates are not valid for pivots
        Assert.AreEqual(1, ui.ddPivotDimension.Items.Count);
        Assert.AreEqual(dimOther, ui.ddPivotDimension.Items[0]);

        //it wants us to pick either a pivot or an axis
        AssertErrorWasShown(ExpectedErrorType.FailedCheck,
            "In order to have 2 columns, one must be selected as a pivot");
        config.PivotOnDimensionID = dimOther.ID;
        config.SaveToDatabase();

        Publish(config);

        AssertNoErrors(ExpectedErrorType.Any);
    }

    [Test]
    [UITimeout(50000)]
    public void Test_AggregateEditorUI_NoExtractableColumns()
    {
        //Create a Catalogue with an AggregateConfiguration that doesn't have any extractable columns yet
        var cata = WhenIHaveA<Catalogue>();
        var config = new AggregateConfiguration(Repository, cata, "My config");

        //these commands should be impossible
        var cmd = new ExecuteCommandAddNewAggregateGraph(ItemActivator, cata);
        Assert.IsTrue(cmd.IsImpossible);
        StringAssert.Contains("no extractable columns", cmd.ReasonCommandImpossible);

        //and if the broken config is activated
        var ui = AndLaunch<AggregateEditorUI>(config);

        //it should not launch and instead show the following message
        var killed = ItemActivator.Results.KilledForms.Single();
        Assert.AreEqual(ui.ParentForm, killed.Key);
        StringAssert.Contains("no extractable columns", killed.Value.Message);
    }


    private AggregateConfiguration GetAggregateConfigurationWithNoDimensions() =>
        GetAggregateConfigurationWithNoDimensions(out var dateEi, out var otherEi);

    private AggregateConfiguration GetAggregateConfigurationWithNoDimensions(out ExtractionInformation dateEi,
        out ExtractionInformation otherEi)
    {
        var config = WhenIHaveA<AggregateConfiguration>(Repository, out dateEi, out otherEi);

        //remove any existing dimensions
        foreach (var d in config.AggregateDimensions)
            d.DeleteInDatabase();

        config.ClearAllInjections();
        return config;
    }
}