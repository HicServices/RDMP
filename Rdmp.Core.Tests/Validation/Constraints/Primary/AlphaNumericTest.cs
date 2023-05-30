// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;

namespace Rdmp.Core.Tests.Validation.Constraints.Primary;

[Category("Unit")]
class AlphaNumericTest : ValidationTests
{
    private IPrimaryConstraint _alphanum;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _alphanum = (IPrimaryConstraint)Validator.CreateConstraint("alphanumeric", Consequence.Wrong);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("A1 ")]
    [TestCase("A ")]
    [TestCase(" 1")]
    public void Validate_Invalid_ThrowsException(string code)
    {
        Assert.NotNull(_alphanum.Validate(code));
    }

    [TestCase(null)]
    [TestCase("1A")]
    [TestCase("a1")]
    [TestCase("1")]
    [TestCase("1b2")]
    [TestCase("aaaaaa")]
    [TestCase("AAAAAA")]
    public void Validate_Valid_Success(string code)
    {
        Assert.IsNull(_alphanum.Validate(code));
    }

    [Test]
    public void Validate_Invalid_ExceptionContainsRequiredInfo()
    {
        var result = _alphanum.Validate(" ");
            
        Assert.NotNull(result.SourceConstraint);
        Assert.AreEqual(typeof(AlphaNumeric), result.SourceConstraint.GetType());
            
    }

}