// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests;

internal class IHasSummaryTests : UnitTests
{
    [Test]
    public void AllObjects_SupportSummary()
    {
        int objectCount = 0;

        foreach (DatabaseEntity obj in WhenIHaveAll())
        {
            try
            {
                var text = obj.GetSummary(true, true);
                Assert.IsNotNull(text);

            }
            catch (Exception ex)
            {
                throw new Exception($"GetSummary is broken for {obj.GetType().Name}", ex);
            }
        }

        Console.WriteLine($"Checked GetSummary for {objectCount} objects");
    }
}