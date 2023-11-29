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
        Assert.That(available, Has.Count.EqualTo(2));

        //the count(*) column
        var included = colsUi.IncludedColumns;
        Assert.That(included, Has.Count.EqualTo(1));

        //before we have added any columns it should not be possible to launch the graph
        var cmdExecuteGraph = new ExecuteCommandExecuteAggregateGraph(ItemActivator, config);
        Assert.That(cmdExecuteGraph.IsImpossible);
        Assert.That(cmdExecuteGraph.ReasonCommandImpossible, Does.Contain("No tables could be identified for the query.  Try adding a column or a force join"));

        var ei = config.Catalogue.CatalogueItems[0].ExtractionInformation;

        //create a new dimension to the config in the database
        _ = new AggregateDimension(Repository, ei, config);

        //publish a refresh
        Publish(config);

        //should show one available columns
        available = colsUi.AvailableColumns;
        Assert.That(available, Has.Count.EqualTo(1));

        //the count(*) column and the added dimension
        included = colsUi.IncludedColumns;
        Assert.That(included, Has.Count.EqualTo(2));

        //should now be possible to launch the graph
        cmdExecuteGraph = new ExecuteCommandExecuteAggregateGraph(ItemActivator, config);
        Assert.That(cmdExecuteGraph.IsImpossible, Is.False);

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
        Assert.That(ui.ddAxisDimension.Items, Has.Count.EqualTo(1));
        Assert.That(ui.ddAxisDimension.Items[0], Is.EqualTo(dimDate));

        //dates are not valid for pivots
        Assert.That(ui.ddPivotDimension.Items, Has.Count.EqualTo(1));
        Assert.That(ui.ddPivotDimension.Items[0], Is.EqualTo(dimOther));

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
        Assert.That(cmd.IsImpossible);
        Assert.That(cmd.ReasonCommandImpossible, Does.Contain("no extractable columns"));

        //and if the broken config is activated
        var ui = AndLaunch<AggregateEditorUI>(config);

        //it should not launch and instead show the following message
        var killed = ItemActivator.Results.KilledForms.Single();
        Assert.That(killed.Key, Is.EqualTo(ui.ParentForm));
        Assert.That(killed.Value.Message, Does.Contain("no extractable columns"));
    }


    private AggregateConfiguration GetAggregateConfigurationWithNoDimensions() =>
        GetAggregateConfigurationWithNoDimensions(out _, out _);

    private AggregateConfiguration GetAggregateConfigurationWithNoDimensions(out ExtractionInformation dateEi,
        out ExtractionInformation otherEi)
    {
        var config = WhenIHaveA(Repository, out dateEi, out otherEi);

        //remove any existing dimensions
        foreach (var d in config.AggregateDimensions)
            d.DeleteInDatabase();

        config.ClearAllInjections();
        return config;
    }
}