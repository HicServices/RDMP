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

internal class WideMessageBoxTests:UITests
{
    [Test]
    public void Test_WideMessageBox_TinyStrings()
    {
        var args = new WideMessageBoxArgs("1","2","3", "4", WideMessageBoxTheme.Help);

        //it is important that the args retain the original length e.g. so user can copy to clipboard the text
        Assert.AreEqual(1, args.Title.Length);
        Assert.AreEqual(1, args.Message.Length);

        var wmb = new WideMessageBox(args);

        // simulate showing the control without actually blocking/firing
        var onShow = typeof(WideMessageBox).GetMethod("OnShown", BindingFlags.NonPublic | BindingFlags.Instance);
        onShow.Invoke(wmb, new[] { new EventArgs() });

        //pretend like we launched it
        LastUserInterfaceLaunched = wmb;

        //the title and body should be a reasonable length
        Assert.AreEqual(1, GetControl<Label>().Single().Text.Length);
        Assert.AreEqual(1, GetControl<RichTextBox>().Single().Text.Length);

        //dialog shouldn't go thinner than 600 pixels
        Assert.AreEqual(600, wmb.Width);
    }

    [Test]
    public void Test_WideMessageBox_LargeStrings()
    {
        var sb = new StringBuilder();

        //send wide message box a million characters
        for(var i =0;i< 1_000_000; i++)
            sb.Append("f");

        var s = sb.ToString();
        var args = new WideMessageBoxArgs(s,s,s,s,WideMessageBoxTheme.Help);

        //it is important that the args retain the original length e.g. so user can copy to clipboard the text
        Assert.AreEqual(1_000_000,args.Title.Length);
        Assert.AreEqual(1_000_000, args.Message.Length);

        var wmb = new WideMessageBox(args);

        // simulate showing the control without actually blocking/firing
        var onShow = typeof(WideMessageBox).GetMethod("OnShown", BindingFlags.NonPublic | BindingFlags.Instance);
        onShow.Invoke(wmb, new[] { new EventArgs()});

        //pretend like we launched it
        LastUserInterfaceLaunched = wmb;
            
        //the title and body should be a reasonable length
        Assert.AreEqual(WideMessageBox.MAX_LENGTH_TITLE,GetControl<Label>().Single().Text.Length);
        Assert.AreEqual(WideMessageBox.MAX_LENGTH_BODY, GetControl<RichTextBox>().Single().Text.Length);

        //when shown on screen it should not go off the edge of the screen

        //find the widest screen
        var availableWidth = Screen.AllScreens.Select(sc=>sc.Bounds.Width).Max();
        Assert.LessOrEqual(wmb.Width, availableWidth);
    }
}