// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Tests.ReusableLibraryCode;

public class UsefulStuffUnitTests
{
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
        foreach(var suffix in new[] { "","\n", "\r","\r\n",",\r\n"})
        {
            var output = UsefulStuff.GetInstance().GetArrayOfColumnNamesFromStringPastedInByUser(input + suffix);
            Assert.AreEqual(expectedOutput, output.Single());
        }
    }


    [Test]
    [TestCase("teststringhere", "teststringhere")]
    [TestCase("test Stringhere", "testStringhere")]
    [TestCase("test String Here", "testStringHere")]
    [TestCase("Test String Here", "TestStringHere")]
    [TestCase("TEST String Here", "TESTStringHere")]
    [TestCase("Test STRING Here", "TestSTRINGHere")]
    [TestCase("Test String HERE", "TestStringHERE")]

    [TestCase("A", "A")]
    [TestCase("AS", "AS")]
    [TestCase("A String", "AString")]
    [TestCase("String A Test", "StringATest")]
    [TestCase("String AS Test", "StringASTest")]
    [TestCase("CT Head", "CTHead")]
    [TestCase("WHERE Clause", "WHEREClause")]
    [TestCase("Sql WHERE", "SqlWHERE")]

    [TestCase("Test String", "Test_String")]
    [TestCase("Test string", "Test_string")]
    [TestCase("test String", "_testString")]
    [TestCase("test String", "_testString_")]
    public void PascalCaseStringToHumanReadable(string human, string pascal)
    {
        Assert.AreEqual(human, UsefulStuff.PascalCaseStringToHumanReadable(pascal));
    }

}