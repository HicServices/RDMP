using CatalogueLibrary.Data;
using CatalogueManager.ANOEngineeringUIs;
using CatalogueManager.CommandExecution.AtomicCommands;
using NUnit.Framework;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class ForwardEngineerANOCatalogueUITests : UITests
    {
        [Test,UITimeout(50000)]
        public void Test_ForwardEngineerANOCatalogueUI_NormalState()
        {
            SetupMEF();

            var cata = WhenIHaveA<Catalogue>();

            //shouldn't be possible to launch the UI
            AssertImpossibleBecause(new ExecuteCommandCreateANOVersion(ItemActivator, cata), "does not have any Extractable Columns");

            //and if we are depersisting it that should be angry
            AndLaunch<ForwardEngineerANOCatalogueUI>(cata);

            AssertNoCrash();
        }
    }
}
