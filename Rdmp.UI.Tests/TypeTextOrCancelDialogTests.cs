// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using System.Text;
using System.Windows.Forms;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.Tests;

internal class TypeTextOrCancelDialogTests : UITests
{
    [Test]
    public void Test_TypeTextOrCancelDialog_TinyStrings()
    {
        var dlg = new TypeTextOrCancelDialog("f", "m", 5000);

        //pretend like we launched it
        LastUserInterfaceLaunched = dlg;

        Assert.Multiple(() =>
        {
            //the title and body should be a reasonable length
            Assert.That(dlg.Text, Has.Length.EqualTo(1));
            Assert.That(GetControl<TextBox>()[1].Text, Has.Length.EqualTo(1));

            //dialog shouldn't go thinner than 540 or wider than 840 pixels
            Assert.That(dlg.Width, Is.GreaterThanOrEqualTo(540));
        });
        Assert.That(dlg.Width, Is.LessThanOrEqualTo(840));
    }

    [Test]
    public void Test_TypeTextOrCancelDialog_LargeStrings()
    {
        var sb = new StringBuilder();

        //send TypeTextOrCancelDialog a million characters
        for (var i = 0; i < 1_000_000; i++)
            sb.Append('f');

        var s = sb.ToString();

        var dlg = new TypeTextOrCancelDialog(s, s, 5000);

        //pretend like we launched it
        LastUserInterfaceLaunched = dlg;

        Assert.Multiple(() =>
        {
            //the title and body should be a reasonable length
            Assert.That(dlg.Text, Has.Length.EqualTo(WideMessageBox.MAX_LENGTH_TITLE));
            Assert.That(GetControl<TextBox>()[1].Text, Has.Length.EqualTo(WideMessageBox.MAX_LENGTH_BODY));

            //dialog shouldn't go thinner than 540 or wider than 840 pixels
            Assert.That(dlg.Width, Is.GreaterThanOrEqualTo(540));
        });
        Assert.That(dlg.Width, Is.LessThanOrEqualTo(840));
    }
}