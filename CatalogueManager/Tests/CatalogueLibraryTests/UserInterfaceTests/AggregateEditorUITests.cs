using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.AggregationUIs.Advanced;
using DataExportLibrary.Repositories;
using NUnit.Framework;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class AggregateEditorUITests:UITests
    {

        [Test,UITimeout(5000)]
        public void Test_AggregateEditorUI()
        {
            var config = WhenIHaveA<AggregateConfiguration>();
            var ui = AndLaunch<AggregateEditorUI>(config);
        }
    }
}
