// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueLibrary.Data;
using CatalogueManager.MainFormUITabs;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class CatalogueUITests : UITests
    {
        [Test, UITimeout(20000)]
        public void Test_CatalogueUI_NormalState()
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
            
            AssertNoCrash();
        }

    }
}
