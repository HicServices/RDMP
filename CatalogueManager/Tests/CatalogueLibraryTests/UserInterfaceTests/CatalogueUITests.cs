using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.MainFormUITabs;
using DataExportLibrary.Repositories;
using NUnit.Framework;
using ScintillaNET;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class CatalogueUITests
    {
        [Test]
        public void TestCatalogueUI_Saving()
        {
            CatalogueUI ui = new CatalogueUI();
              
            var memory = new MemoryDataExportRepository();
            
            var cata = new Catalogue(memory, "Mycata");

            Form f = new Form();

            f.Controls.Add(ui);

            ui.SetDatabaseObject(new TestActivateItems(memory), cata);

            var scintilla =  (Scintilla)typeof(CatalogueUI).GetField("_scintillaDescription", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ui);
            scintilla.Text = "amagad zombies";

            var saver = ui.GetObjectSaverButton();
            saver.Save();

            Assert.AreEqual("amagad zombies", cata.Description);
        }
    }
}
