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
