// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.IO;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode.Extensions;

namespace Rdmp.Core.Tests.ReusableCodeTests;

[Category("Unit")]
class DataTableExtensionsTests
{
    [Test]
    public void TestEscaping_CommaInCell()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "out.csv");

        using var dt = new DataTable();
        dt.Columns.Add("Phrase");
        dt.Columns.Add("Car");

        dt.Rows.Add("omg,why me!", "Ferrari");

        dt.SaveAsCsv(path);

        var answer = File.ReadAllText(path);

        Assert.AreEqual(answer,
            @"Phrase,Car
""omg,why me!"",Ferrari
");

    }

    [Test]
    public void TestEscaping_CommaAndQuotesInCell()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory,"out.csv");

        using var dt = new DataTable();
        dt.Columns.Add("Phrase");
        dt.Columns.Add("Car");

        dt.Rows.Add("omg,\"why\" me!","Ferrari");

        dt.SaveAsCsv(path);

        var answer = File.ReadAllText(path);

        Assert.AreEqual(answer,
            @"Phrase,Car
""omg,""""why"""" me!"",Ferrari
");
    }


    [Test]
    public void TestEscaping_CommaAndQuotesInCell2()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "out.csv");

        using var dt = new DataTable();
        dt.Columns.Add("Phrase");
        dt.Columns.Add("Car");

        dt.Rows.Add("\"omg,why me!\"", "Ferrari");

        dt.SaveAsCsv(path);

        var answer = File.ReadAllText(path);

        Assert.AreEqual(answer,
            @"Phrase,Car
""""""omg,why me!"""""",Ferrari
");
    }
}