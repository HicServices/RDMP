using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.SimpleDialogs;
using NUnit.Framework;
using ScintillaNET;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    class CatalogueItemUITests:UITests
    {
        [Test, UITimeout(20000)]
        public void Test_CatalogueItemUI_DescriptionPropogation()
        {
            //when I have two CatalogueItems that have the same name
            var catalogueItem = WhenIHaveA<CatalogueItem>();
            var catalogueItem2 = WhenIHaveA<CatalogueItem>();

            var ui = AndLaunch<CatalogueItemUI>(catalogueItem);

            //when I change the description of the first
            var scintilla = ui._scintillaDescription;
            scintilla.Text = "what is in the column";

            //and save it
            var saver = ui.GetObjectSaverButton();
            saver.Save();

            //the new description shuold be set in my class
            Assert.AreEqual("what is in the column", catalogueItem.Description);

            //and the UI should have shown the Propagate changes dialog
            Assert.AreEqual(1, ItemActivator.Results.WindowsShown.Count);
            Assert.IsInstanceOf(typeof(PropagateCatalogueItemChangesToSimilarNamedUI),ItemActivator.Results.WindowsShown.Single());
        }
    }
}