// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.DataLoad.Triggers.Implementations;

namespace Rdmp.Core.Tests.Curation.Integration;

public class MySqlTriggerImplementerTests
{
    [TestCase("4.0",true)]
    [TestCase("5.1",true)]
    [TestCase("8.5",false)]
    [TestCase("5.5.64-MariaDB",true)]
    [TestCase("10.5.64-MariaDB",false)]
    public void TestOldNew(string versionString, bool expectToUseOldMethod)
    {
        Assert.AreEqual(expectToUseOldMethod,MySqlTriggerImplementer.UseOldDateTimeDefaultMethod(versionString));
    }
}