// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Runtime.Versioning;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.UI.MainFormUITabs;

namespace Rdmp.UI.Tests;

[SupportedOSPlatform("windows7.0")]
public class CatalogueUITests : UITests
{
    [Test]
    [UITimeout(20000)]
    public void Test_CatalogueUI_NormalState()
    {
        var cata = WhenIHaveA<Catalogue>();
        var ui = AndLaunch<CatalogueUI>(cata);

        //there no unsaved changes
        Assert.That(cata.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges));

        //but when I type text
        ui._scintillaDescription.Text = "amagad zombies";

        Assert.Multiple(() =>
        {
            //my class should get the typed text but it shouldn't be saved into the database yet
            Assert.That(cata.Description, Is.EqualTo("amagad zombies"));
            Assert.That(cata.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.DatabaseCopyDifferent));
        });

        //when I press undo
        var saver = ui.GetObjectSaverButton();
        saver.Undo();

        Assert.Multiple(() =>
        {
            //it should set the text editor back to blank
            Assert.That(ui._scintillaDescription.Text, Is.EqualTo(""));
            //and clear my class property
            Assert.That(cata.Description, Is.EqualTo(null));
        });

        //redo should update both the local class and text box
        saver.Redo();
        Assert.Multiple(() =>
        {
            Assert.That(ui._scintillaDescription.Text, Is.EqualTo("amagad zombies"));
            Assert.That(cata.Description, Is.EqualTo("amagad zombies"));
        });

        //undo a redo should still be valid
        saver.Undo();
        Assert.Multiple(() =>
        {
            Assert.That(ui._scintillaDescription.Text, Is.EqualTo(""));
            Assert.That(cata.Description, Is.EqualTo(null));
        });

        saver.Redo();
        Assert.Multiple(() =>
        {
            Assert.That(ui._scintillaDescription.Text, Is.EqualTo("amagad zombies"));
            Assert.That(cata.Description, Is.EqualTo("amagad zombies"));
        });

        //when I save
        saver.Save();

        Assert.Multiple(() =>
        {
            //my class should have no changes (vs the database) and should have the proper description
            Assert.That(cata.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges));
            Assert.That(cata.Description, Is.EqualTo("amagad zombies"));
        });

        AssertNoErrors(ExpectedErrorType.Any);

        //clear the name
        cata.Name = null;
        AssertErrorWasShown(ExpectedErrorType.ErrorProvider, "Value cannot be null");
        cata.Name = "omg";
        AssertNoErrors(ExpectedErrorType.Any);
    }

    [Test]
    [UITimeout(50000)]
    public void Test_CatalogueUI_AcronymDuplicates()
    {
        var cata1 = WhenIHaveA<Catalogue>();
        var cata2 = WhenIHaveA<Catalogue>();

        cata1.Name = "hey";
        cata1.Acronym = null;
        cata1.SaveToDatabase();

        cata2.Name = "fish";
        cata2.Acronym = null;
        cata2.SaveToDatabase();

        var ui = AndLaunch<CatalogueUI>(cata1);
        AssertNoErrors(ExpectedErrorType.Any);

        //now cata 2 has an acronym
        cata2.Acronym = "AB";
        cata2.SaveToDatabase();

        AssertNoErrors(ExpectedErrorType.Any);

        //it tells me that I have to make it unique
        AssertErrorWasShown(ExpectedErrorType.ErrorProvider, "Must be unique");

        //and all is good again
        AssertNoErrors(ExpectedErrorType.Any);
    }
}