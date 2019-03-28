using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.AggregationUIs.Advanced;
using DataExportLibrary.Repositories;
using FAnsi.Implementation;
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
            var colsUi = GetPrivateField<SelectColumnUI>(ui, "selectColumnUI1");
            
            //should show two available columns
            var available = GetPrivateField<List<IColumn>>(colsUi, "_availableColumns");
            Assert.AreEqual(2,available.Count);
        }
    }
}
