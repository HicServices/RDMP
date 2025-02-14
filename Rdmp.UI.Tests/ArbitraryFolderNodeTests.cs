// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BrightIdeasSoftware;
using NUnit.Framework;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.Collections;
using System;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core;

namespace Rdmp.UI.Tests;

internal class ArbitraryFolderNodeTests : UITests
{
    [Test]
    [UITimeout(50000)]
    public void Test_ArbitraryFolderNode_CommandGetter_Throwing()
    {
        var tlv = new TreeListView();
        var common = new RDMPCollectionCommonFunctionality();
        common.SetUp(RDMPCollection.None, tlv, ItemActivator, null, null);

        var node = new ArbitraryFolderNode("my node", 0);

        var menu1 = common.GetMenuIfExists(node);
        Assert.That(menu1, Is.Not.Null);
        var count1 = menu1.Items.Count;
        //some you get for free e.g. Expand/Collapse
        Assert.That(count1, Is.GreaterThanOrEqualTo(2));

        //set the menu to have one command in it
        node.CommandGetter = () => new IAtomicCommand[] { new ImpossibleCommand("Do Nothing") };

        var menu2 = common.GetMenuIfExists(node);

        var count2 = menu2.Items.Count;

        // expect 2 new entries in the context menu.  The "Do Nothing" command added above
        // and a tool strip separator to divide the menu commands from the common commands
        Assert.That(count2, Is.EqualTo(count1 + 2));

        //what happens if the delegate crashes?
        node.CommandGetter = () => throw new NotSupportedException("It went wrong!");

        Assert.DoesNotThrow(() => common.GetMenuIfExists(node));

        AssertErrorWasShown(ExpectedErrorType.GlobalErrorCheckNotifier,
            "Failed to build menu for my node of Type Rdmp.Core.Providers.Nodes.ArbitraryFolderNode");
    }
}