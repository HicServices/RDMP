using System;
using CatalogueLibrary.Data;
using CatalogueManager.MainFormUITabs;
using DataExportLibrary.Repositories;
using NUnit.Framework;
using ScintillaNET;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class CatalogueUITests : UITests
    {
        [Test, UITimeout(20000)]
        public void Test_CatalogueUI_SaveDescription()
        {
            //create catalogue
            var memory = new MemoryDataExportRepository();
            var cata = new Catalogue(memory, "Mycata");
            cata.SaveToDatabase();

            var ui = GetSingleDatabaseObjectControlForm<CatalogueUI>();
            ui.SetDatabaseObject(new TestActivateItems(memory), cata);

            var scintilla = GetPrivateField<Scintilla>(ui, "_scintillaDescription");
            scintilla.Text = "amagad zombies";

            var saver = ui.GetObjectSaverButton();
            saver.Save();

            Assert.AreEqual("amagad zombies", cata.Description);
        }
        
    }
}
