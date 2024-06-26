﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Tests.ReusableCodeTests;

public class UsefulStuffUnitTests
{
    [TestCase("BadValue", false)]
    [TestCase("120356785", false)]
    [TestCase("1203567850", false)]
    [TestCase("1203567855", true)]
    public void ChiValidationTests(string chi, bool valid)
    {
        Assert.That(UsefulStuff.CHIisOK(chi), Is.EqualTo(valid), $"Validation gave incorrect answer for CHI '{chi}'");
    }

    [TestCase("[ff ff]", "ff ff")]
    [TestCase("`ff ff`", "ff ff")]
    [TestCase("'ff ff'", "ff ff")]
    [TestCase("\"ff ff\"", "ff ff")]
    [TestCase("ff ff", "ff ff")]
    [TestCase("ab.cd", "cd")]
    [TestCase("[aa]..[ff],", "ff")]
    [TestCase("[bb]..[ff],", "ff")]
    [TestCase("[c d]..[we ef],", "we ef")]
    public void TestGetArrayOfColumnNamesFromStringPastedInByUser(string input, string expectedOutput)
    {
        foreach (var suffix in new[] { "", "\n", "\r", "\r\n", ",\r\n" })
        {
            var output = UsefulStuff.GetArrayOfColumnNamesFromStringPastedInByUser($"{input}{suffix}");
            Assert.That(output.Single(), Is.EqualTo(expectedOutput));
        }
    }


    [Test]
    public void ClipboardHtmlTest()
    {
        const string test = "Version:1.0\r\nStartHTML:000051\r\nEndHTML:0000000055\r\nTest";
        Assert.That(UsefulStuff.GetClipboardFormattedHtmlStringFromHtmlString("Test"), Is.EqualTo(test));
    }
}