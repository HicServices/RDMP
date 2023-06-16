// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Logging.PastEvents;
using System;

namespace Rdmp.Core.Tests.Logging;

class ArchivalDataLoadInfoTests
{
    [Test]
    public void ArchivalDataLoadInfo_ToString()
    {
        var adi = new ArchivalDataLoadInfo
        {
            StartTime = new DateTime(2010, 1, 1,12,0,0),
            EndTime = new DateTime(2010, 1, 3,13,20,23)
        };

        // This dle took 2 days, 1 hour, 20 mins and 23 seconds
        StringAssert.Contains("(49:20:23)", adi.ToString());
    }
}