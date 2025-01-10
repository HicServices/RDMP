// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Runtime.Versioning;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.MainFormUITabs;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Tests;

[SupportedOSPlatform("windows7.0")]
internal class CatalogueItemUITests : UITests
{
    [Test]
    [UITimeout(20000)]
    public void Test_CatalogueItemUI_NormalState()
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

        Assert.Multiple(() =>
        {
            //the new description shuold be set in my class
            Assert.That(catalogueItem.Description, Is.EqualTo("what is in the column"));

            //and the UI should have shown the Propagate changes dialog
            Assert.That(ItemActivator.Results.WindowsShown, Has.Count.EqualTo(1));
        });
        Assert.That(ItemActivator.Results.WindowsShown.Single(), Is.InstanceOf(typeof(PropagateCatalogueItemChangesToSimilarNamedUI)));

        AssertNoErrors(ExpectedErrorType.Any);
    }

    /// <summary>
    /// Tests that <see cref="INamedTab.GetTabName"/> works even when half way through a call
    /// to <see cref="IRDMPSingleDatabaseObjectControl.SetDatabaseObject"/>
    /// </summary>
    [Test]
    [UITimeout(20000)]
    public void Test_CatalogueItemUI_GetTabName()
    {
        var ci = WhenIHaveA<CatalogueItem>();
        var ui = AndLaunch<CatalogueItemUI>(ci);

        Assert.That(ui.GetTabName(), Is.EqualTo("MyCataItem (Mycata)"));

        //introduce database change but don't save
        ci.Name = "Fish";

        //simulates loading the UI with an out of date object
        ui = AndLaunch<CatalogueItemUI>(ci, false);

        //now what we want to ensure is that ui.GetTabName works properly even half way through SetDatabaseObject
        //so register a callback that interrogates GetTabName midway
        ItemActivator.ShouldReloadFreshCopyDelegate = () =>
        {
            ui.GetTabName();
            return true;
        };

        //and finish launching it, this should trigger the 'FreshCopyDelegate' which will exercise GetTabName.
        ui.SetDatabaseObject(ItemActivator, ci);

        Assert.That(ui.GetTabName(), Is.EqualTo("MyCataItem (Mycata)"));

        //clear the delgate for the next user
        ItemActivator.ShouldReloadFreshCopyDelegate = null;
    }
}