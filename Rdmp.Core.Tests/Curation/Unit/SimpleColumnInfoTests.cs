// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Unit;

public class SimpleColumnInfoTests : DatabaseTests
{
    [Test]
    [TestCase("varchar(5)", 5)]
    [TestCase("int", -1)]
    [TestCase("datetime2", -1)]
    [TestCase("nchar(100)", 100)]
    [TestCase("char(11)", 11)]
    [TestCase("text", int.MaxValue)]
    [TestCase("varchar(max)", int.MaxValue)]
    public void GetColumnLength(string type, int? expectedLength)
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var t = db.CreateTable("MyTable", new[]
        {
            new DatabaseColumnRequest("MyCol", type)
        });

        Import(t, out var ti, out var cis);

        Assert.AreEqual(expectedLength,
            cis.Single().Discover(DataAccessContext.InternalDataProcessing).DataType.GetLengthIfString());

        ti.DeleteInDatabase();
    }
}