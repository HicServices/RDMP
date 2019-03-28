using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.SimpleDialogs;
using DataExportLibrary.Repositories;
using NUnit.Framework;
using ScintillaNET;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class CatalogueItemUITests:UITests
    {
        [Test, UITimeout(20000)]
        public void Test_CatalogueItemUI_DescriptionPropogation()
        {
            //create two catalogues that share a field with the same name
            var memory = new MemoryDataExportRepository();
            var cata = new Catalogue(memory, "Mycata");
            var catalogueItem = new CatalogueItem(memory,cata, "MyCataItem");
            catalogueItem.SaveToDatabase();

            //the second one, propagation should trigger on this one
            var cata2 = new Catalogue(memory, "Mycata");
            var catalogueItem2 = new CatalogueItem(memory, cata, "MyCataItem");

            var activator = new TestActivateItems(memory);
                
            var ui = GetSingleDatabaseObjectControlForm<CatalogueItemUI>();
            ui.SetDatabaseObject(activator, catalogueItem);

            var scintilla = GetPrivateField<Scintilla>(ui, "_scintillaDescription");
            scintilla.Text = "what is in the column";

            var saver = ui.GetObjectSaverButton();
            saver.Save();

            Assert.AreEqual("what is in the column", catalogueItem.Description);

            Assert.AreEqual(1, activator.WindowsShown.Count);
            Assert.IsInstanceOf(typeof(PropagateCatalogueItemChangesToSimilarNamedUI),activator.WindowsShown.Single());
        }
    }

    /*
    class Test
    {
        
        //does not work
        [Test, UITimeout(5000)]
        public void Test1()
        {
            //blocks test and timeout is not respected
            MessageBox.Show("It went wrong");
        }

        //works but is ugly
        [Test]
        public void Test2()
        {
            Task runUIStuff = new Task(()=>
            {
                MessageBox.Show("It went wrong"); 
    
            });
            runUIStuff.Start();

            Task.WaitAny(Task.Delay(5000), runUIStuff);

            if(!runUIStuff.IsCompleted)
            {
                Process.GetCurrentProcess().CloseMainWindow();
                Assert.Fail("Test did not complete after timeout");
            }
        }
    }*/
}