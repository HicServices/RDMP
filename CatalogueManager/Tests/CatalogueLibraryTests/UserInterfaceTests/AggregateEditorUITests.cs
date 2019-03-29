using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.AggregationUIs.Advanced;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Refreshing;
using NUnit.Framework;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class AggregateEditorUITests:UITests
    {

        [Test,UITimeout(5000)]
        public void Test_AggregateEditorUI()
        {
            LoadDatabaseImplementations();

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
        public void Test_AggregateEditorUI_NoExtractableColumns()
        {
            LoadDatabaseImplementations();

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
