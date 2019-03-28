using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.AggregationUIs.Advanced;
using CatalogueManager.CommandExecution.AtomicCommands;
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
        }


        [Test, UITimeout(5000)]
        public void Test_AggregateEditorUI_NoExtractableColumns()
        {
            LoadDatabaseImplementations();

            var cata = WhenIHaveA<Catalogue>();
            var config = new AggregateConfiguration(Repository,cata,"my config");
            config.SaveToDatabase();

            var cmd = new ExecuteCommandAddNewAggregateGraph(ItemActivator, cata);
            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.Contains("no extractable columns",cmd.ReasonCommandImpossible);
            
            var ui = AndLaunch<AggregateEditorUI>(config);

            var killed = ItemActivator.Results.KilledForms.Single();
            Assert.AreEqual(ui.ParentForm,killed.Key);
            StringAssert.Contains("no extractable columns", killed.Value.Message);
        }
    }
}
