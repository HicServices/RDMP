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
internal class AlphaTest : ValidationTests
{
    private IPrimaryConstraint _alpha;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _alpha = (IPrimaryConstraint)Validator.CreateConstraint("alpha", Consequence.Wrong);
    }

    [TestCase("12345")]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("A1")]
    [TestCase("A1 ")]
    public void Validate_Invalid_ThrowsException(string code)
    {
        Assert.NotNull(_alpha.Validate(code));
    }

    [TestCase(null)]
    [TestCase("A")]
    [TestCase("a")]
    [TestCase("Ab")]
    [TestCase("Ba")]
    [TestCase("aaaaaa")]
    [TestCase("AAAAAA")]
    public void Validate_Valid_Success(string code)
    {
        Assert.IsNull(_alpha.Validate(code));
    }

    [Test]
    public void Validate_Invalid_ExceptionContainsRequiredInfo()
    {
        var result = _alpha.Validate("9");

        Assert.NotNull(result.SourceConstraint);
        Assert.AreEqual(typeof(Alpha), result.SourceConstraint.GetType());
    }
}