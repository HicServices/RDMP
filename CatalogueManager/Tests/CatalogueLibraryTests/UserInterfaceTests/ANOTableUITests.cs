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
        public void Test_ANOTableUI_NormalState()
        {
            var anoTable = WhenIHaveA<ANOTable>();
            AndLaunch<ANOTableUI>(anoTable);
            AssertNoCrash();
        }

        [Test, UITimeout(5000)]
        public void Test_ANOTableUI_ServerWrongType()
        {
            ExternalDatabaseServer srv;
            var anoTable = WhenIHaveA<ANOTable>(out srv);
            srv.CreatedByAssembly = null;
            srv.SaveToDatabase();

            var ui = AndLaunch<ANOTableUI>(anoTable);
            
            //no exceptions
            AssertNoCrash();

            //but there should be an error on this UI element
            Assert.AreEqual("Server is not an ANO server", ui.ServerErrorProvider.GetError(ui.llServer));
        }

        
    }
}
