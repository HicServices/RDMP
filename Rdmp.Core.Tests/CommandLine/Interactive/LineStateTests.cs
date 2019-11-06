using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Interactive;

namespace Rdmp.Core.Tests.CommandLine.Interactive
{
    class LineStateTests
    {
        [Test]
        public void Test_LineState_AfterSingleKeypress()
        {
            //This is how LineState looks after the user has pressed a single keystroke in the console (note that the cursor position
            //is off the end of the line string - as you would expect)
            var line = new LineState("t", 1);
            Assert.AreEqual("t",line.LineBeforeCursor);
            Assert.AreEqual("",line.LineAfterCursor);
        }

        [Test]
        public void Test_LineState_IsInclusive()
        {
            //This is how LineState looks after the user has pressed a then b then the back arrow (to move cursor back 1 position)
            var line = new LineState("ab",1);

            Assert.AreEqual("a",line.LineBeforeCursor);
            Assert.AreEqual("b",line.LineAfterCursor);

        }
    }
}
