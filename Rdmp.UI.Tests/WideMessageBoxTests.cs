// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rdmp.UI.SimpleDialogs;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;
using System.Reflection;
using System;

namespace Rdmp.UI.Tests;

internal class WideMessageBoxTests : UITests
{
    [Test]
    public void Test_WideMessageBox_TinyStrings()
    {
        var args = new WideMessageBoxArgs("1", "2", "3", "4", WideMessageBoxTheme.Help);

        Assert.Multiple(() =>
        {
            //it is important that the args retain the original length e.g. so user can copy to clipboard the text
            Assert.That(args.Title, Has.Length.EqualTo(1));
            Assert.That(args.Message, Has.Length.EqualTo(1));
        });

        var wmb = new WideMessageBox(args);

        // simulate showing the control without actually blocking/firing
        var onShow = typeof(WideMessageBox).GetMethod("OnShown", BindingFlags.NonPublic | BindingFlags.Instance);
        onShow.Invoke(wmb, new[] { EventArgs.Empty });

        //pretend like we launched it
        LastUserInterfaceLaunched = wmb;

        Assert.Multiple(() =>
        {
            //the title and body should be a reasonable length
            Assert.That(GetControl<Label>().Single().Text, Has.Length.EqualTo(1));
            Assert.That(GetControl<RichTextBox>().Single().Text, Has.Length.EqualTo(1));

            //dialog shouldn't go thinner than 600 pixels
            Assert.That(wmb.Width, Is.EqualTo(600));
        });
    }

    [Test]
    public void Test_WideMessageBox_LargeStrings()
    {
        var sb = new StringBuilder();

        //send wide message box a million characters
        for (var i = 0; i < 1_000_000; i++)
            sb.Append('f');

        var s = sb.ToString();
        var args = new WideMessageBoxArgs(s, s, s, s, WideMessageBoxTheme.Help);

        Assert.Multiple(() =>
        {
            //it is important that the args retain the original length e.g. so user can copy to clipboard the text
            Assert.That(args.Title, Has.Length.EqualTo(1_000_000));
            Assert.That(args.Message, Has.Length.EqualTo(1_000_000));
        });

        var wmb = new WideMessageBox(args);

        // simulate showing the control without actually blocking/firing
        var onShow = typeof(WideMessageBox).GetMethod("OnShown", BindingFlags.NonPublic | BindingFlags.Instance);
        onShow.Invoke(wmb, new[] { EventArgs.Empty });

        //pretend like we launched it
        LastUserInterfaceLaunched = wmb;

        Assert.Multiple(() =>
        {
            //the title and body should be a reasonable length
            Assert.That(GetControl<Label>().Single().Text, Has.Length.EqualTo(WideMessageBox.MAX_LENGTH_TITLE));
            Assert.That(GetControl<RichTextBox>().Single().Text, Has.Length.EqualTo(WideMessageBox.MAX_LENGTH_BODY));
        });

        //when shown on screen it should not go off the edge of the screen

        //find the widest screen
        var availableWidth = Screen.AllScreens.Select(static sc => sc.Bounds.Width).Max();
        Assert.That(wmb.Width, Is.LessThanOrEqualTo(availableWidth));
    }
}