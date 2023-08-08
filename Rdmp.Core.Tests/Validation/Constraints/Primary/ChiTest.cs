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
internal class ChiTest : ValidationTests
{
    private IPrimaryConstraint _chi;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _chi = (IPrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("banana")]
    [TestCase("092567")]
    [TestCase("020445037")]
    [TestCase("000000000")]
    [TestCase("02044503731")]
    public void Validate_InvalidChi_ThrowsException(string code)
    {
        Assert.NotNull(_chi.Validate(code));
    }

    [TestCase(null)]
    [TestCase("0204450373")]
    public void Validate_ValidChi_Success(string code)
    {
        Assert.IsNull(_chi.Validate(code));
    }

    [Test]
    public void Validate_InvalidChi_ExceptionContainsRequiredInfo()
    {
        var result = _chi.Validate("banana");

        Assert.NotNull(result.SourceConstraint);
        Assert.AreEqual(typeof(Chi), result.SourceConstraint.GetType());
    }
}