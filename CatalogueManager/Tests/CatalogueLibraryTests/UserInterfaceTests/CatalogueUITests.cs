using CatalogueLibrary.Data;
using CatalogueManager.MainFormUITabs;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using ScintillaNET;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class CatalogueUITests : UITests
    {
        [Test, UITimeout(20000)]
        public void Test_CatalogueUI_SaveDescription()
        {
            var cata = WhenIHaveA<Catalogue>();
            var ui = AndLaunch<CatalogueUI>(cata);

            //there no unsaved changes
            Assert.AreEqual(ChangeDescription.NoChanges, cata.HasLocalChanges().Evaluation);

            //but when I type text
            ui._scintillaDescription.Text = "amagad zombies";

            //my class should get the typed text but it shouldn't be saved into the database yet
            Assert.AreEqual("amagad zombies", cata.Description);
            Assert.AreEqual(ChangeDescription.DatabaseCopyDifferent, cata.HasLocalChanges().Evaluation);

            //when I press undo
            var saver = ui.GetObjectSaverButton();
            saver.Undo();

            //it should set the text editor back to blank
            Assert.AreEqual("", ui._scintillaDescription.Text);
            //and clear my class property
            Assert.AreEqual(null, cata.Description);

            //redo should update both the local class and text box
            saver.Redo();
            Assert.AreEqual("amagad zombies", ui._scintillaDescription.Text);
            Assert.AreEqual("amagad zombies", cata.Description);

            //undo a redo should still be valid
            saver.Undo();
            Assert.AreEqual("", ui._scintillaDescription.Text);
            Assert.AreEqual(null, cata.Description);

            saver.Redo();
            Assert.AreEqual("amagad zombies", ui._scintillaDescription.Text);
            Assert.AreEqual("amagad zombies", cata.Description);
            
            //when I save
            saver.Save();

            //my class should have no changes (vs the database) and should have the proper description
            Assert.AreEqual(ChangeDescription.NoChanges, cata.HasLocalChanges().Evaluation);
            Assert.AreEqual("amagad zombies", cata.Description);
        }

    }
}
