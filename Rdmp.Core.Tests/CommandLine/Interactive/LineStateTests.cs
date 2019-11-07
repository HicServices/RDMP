// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
