// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;


namespace Rdmp.UI.Tests;

[Category("Unit")]
public class RecentHistoryOfControlsTests
{
    [Test]
    public void TestOverflowPrevention()
    {
        var t = new System.Windows.Forms.TextBox();

        var c = new RecentHistoryOfControls(t, new Guid("b3ccaf14-702a-438a-8cf4-d550d6d7775d"));

        c.Clear();
        var overFlowCounter = 100000;

        for (var i = 0; i < overFlowCounter; i++)
            c.AddResult($"testOverflowValue{Guid.NewGuid()}",
                i % 1000 == 0); //only save every X values added for performance

        c.Clear();
    }
}