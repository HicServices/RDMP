// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.AggregationUIs;
using CatalogueManager.AggregationUIs.Advanced;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Refreshing;
using NUnit.Framework;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class AggregateEditorUITests:UITests
    {
        [SetUp]
        public void CallLoadDatabaseImplementations()
        {
            LoadDatabaseImplementations();
        }

        [Test,UITimeout(5000)]
        public void Test_AggregateEditorUI()
        {
            var config = WhenIHaveA<AggregateConfiguration>();
            var ui = AndLaunch<AggregateEditorUI>(config);

            //The selected columns ui
            var colsUi = ui.selectColumnUI1;
            
            //should show two available columns
            var available = colsUi.AvailableColumns;
            Assert.AreEqual(2,available.Count);

            //the count(*) column
            var included = colsUi.IncludedColumns;
            Assert.AreEqual(1, included.Count);

            //before we have added any columns it should not be possible to launch the graph
            var cmdExecuteGraph = new ExecuteCommandExecuteAggregateGraph(ItemActivator, config);
            Assert.IsTrue(cmdExecuteGraph.IsImpossible);
            StringAssert.Contains("No tables could be identified for the query.  Try adding a column or a force join",cmdExecuteGraph.ReasonCommandImpossible);

            var ei = config.Catalogue.CatalogueItems[0].ExtractionInformation;

            //create a new dimension to the config in the database
            var dim = new AggregateDimension(Repository, ei, config);
            
            //publish a refresh
            ItemActivator.RefreshBus.Publish(this, new RefreshObjectEventArgs(config));

            //should show one available columns
            available = colsUi.AvailableColumns;
            Assert.AreEqual(1, available.Count);

            //the count(*) column and the added dimension
            included = colsUi.IncludedColumns;
            Assert.AreEqual(2, included.Count);

            //should now be possible to launch the graph
            cmdExecuteGraph = new ExecuteCommandExecuteAggregateGraph(ItemActivator, config);
            Assert.IsFalse(cmdExecuteGraph.IsImpossible);
        }

        [Test, UITimeout(5000)]
        public void Test_AggregateEditorUI_AxisOnlyShowsDateDimensions()
        {
            ExtractionInformation dateEi;
            ExtractionInformation otherEi;
            var config = WhenIHaveA<AggregateConfiguration>(out dateEi,out otherEi);

            var dimDate = new AggregateDimension(Repository, dateEi, config);
            var dimOther = new AggregateDimension(Repository, otherEi, config);
            
            var ui = AndLaunch<AggregateEditorUI>(config);

            //only date should be an option for axis dimension
            Assert.AreEqual(1, ui.ddAxisDimension.Items.Count);
            Assert.AreEqual(dimDate,ui.ddAxisDimension.Items[0]);

            //dates are not valid for pivots
            Assert.AreEqual(1, ui.ddPivotDimension.Items.Count);
            Assert.AreEqual(dimOther, ui.ddPivotDimension.Items[0]);
        }

        [Test, UITimeout(5000)]
        public void Test_AggregateEditorUI_NoExtractableColumns()
        {
            //Create a Catalogue with an AggregateConfiguration that doesn't have any extractable columns yet
            var cata = WhenIHaveA<Catalogue>();
            var config = new AggregateConfiguration(Repository,cata,"my config");
            config.SaveToDatabase();

            //these commands should be impossible
            var cmd = new ExecuteCommandAddNewAggregateGraph(ItemActivator, cata);
            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.Contains("no extractable columns",cmd.ReasonCommandImpossible);
            
            //and if the broken config is activated
            var ui = AndLaunch<AggregateEditorUI>(config);

            //it should not launch and instead show the following message
            var killed = ItemActivator.Results.KilledForms.Single();
            Assert.AreEqual(ui.ParentForm,killed.Key);
            StringAssert.Contains("no extractable columns", killed.Value.Message);
        }
    }
}
