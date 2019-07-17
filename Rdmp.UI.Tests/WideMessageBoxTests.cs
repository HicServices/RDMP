// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.Tests
{
    class WideMessageBoxTests:UITests
    {
        [Test]
        public void Test_WideMessageBox_LargeStrings()
        {
            StringBuilder sb = new StringBuilder();

            //send wide message box a million characters
            for(int i=0;i< 1_000_000; i++)
                sb.Append("f");

            var s = sb.ToString();
            var args = new WideMessageBoxArgs(s,s,s,s,WideMessageBoxTheme.Help);

            //it is important that the args retain the original length e.g. so user can copy to clipboard the text
            Assert.AreEqual(1_000_000,args.Title.Length);
            Assert.AreEqual(1_000_000, args.Message.Length);

            var wmb = new WideMessageBox(args);

            //pretend like we launched it
            LastUserInterfaceLaunched = wmb;
            
            //the title and body should be a reasonable length
            Assert.AreEqual(WideMessageBox.MAX_LENGTH_TITLE,GetControl<Label>().Single().Text.Length);
            Assert.AreEqual(WideMessageBox.MAX_LENGTH_BODY, GetControl<RichTextBox>().Single().Text.Length);
        }
    }
}
