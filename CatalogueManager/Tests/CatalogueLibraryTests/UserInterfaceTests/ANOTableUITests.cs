using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.ANOEngineeringUIs;
using NUnit.Framework;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class ANOTableUITests:UITests
    {
        [Test, UITimeout(5000)]
        public void Test_NoServer()
        {
            var anoTable = WhenIHaveA<ANOTable>();
            var ui = AndLaunch<ANOTableUI>(anoTable);

            //no exceptions
            Assert.AreEqual(0,ItemActivator.Results.KilledForms.Count);
        }

        [Test, UITimeout(5000)]
        public void Test_Server_WrongType()
        {
            ExternalDatabaseServer srv;
            var anoTable = WhenIHaveA<ANOTable>(out srv);
            srv.CreatedByAssembly = null;
            srv.SaveToDatabase();

            var ui = AndLaunch<ANOTableUI>(anoTable);
            
            //no exceptions
            Assert.AreEqual(0, ItemActivator.Results.KilledForms.Count);
            Assert.AreEqual("Server is not an ANO server", ui.ServerErrorProvider.GetError(ui.llServer));
        }

        
    }
}
