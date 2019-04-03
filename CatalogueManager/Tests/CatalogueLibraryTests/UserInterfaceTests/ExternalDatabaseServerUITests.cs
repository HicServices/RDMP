using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.MainFormUITabs.SubComponents;
using NUnit.Framework;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class ExternalDatabaseServerUITests:UITests
    {
        [Test, UITimeout(5000)]
        public void Test_ExternalDatabaseServerUITests_NormalState()
        {
            var server = WhenIHaveA<ExternalDatabaseServer>();
            var ui = AndLaunch<ExternalDatabaseServerUI>(server);

            AssertNoErrors(ExpectedErrorType.Any);

            ui.tbUsername.Text = "fish";
            Assert.AreEqual("fish", server.Username);

            ui.GetObjectSaverButton().Save();
            Assert.AreEqual("fish", server.Username);

            ui.tbUsername.Text = "";
            ui.GetObjectSaverButton().Save();
            Assert.AreEqual("", server.Username);
        }
        
    }
}