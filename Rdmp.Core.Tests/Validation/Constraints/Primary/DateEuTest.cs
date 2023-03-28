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
class DateEuTest : ValidationTests
{
    private IPrimaryConstraint _date;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();
        _date = constraint_date();
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("banana")]
    [TestCase("092567")]
    [TestCase("5.9.677")]
    [TestCase("250967")]
    [TestCase("1967")]
    [TestCase("25/09")]
    [TestCase("90/67")]
    [TestCase("09/1967")]
    [TestCase("19670925")]
    public void Validate_InvalidDate_ThrowsException(string code)
    {
        Assert.NotNull(_date.Validate(code));
    }

    [TestCase(null)]
    [TestCase("25/09.67")]
    [TestCase("25/09-67")]
    [TestCase("25-09/67")]
    [TestCase("25.09-67")]
    [TestCase("25.09.67")]
    [TestCase("25.09.1967")]
    [TestCase("25.9.1967")]
    [TestCase("5.9.67")]
    [TestCase("5.12.67")]
    public void Validate_ValidDate_Success(string code)
    {
        Assert.IsNull(_date.Validate(code));
    }

    [Test]
    public void Validate_InvalidDate_ExceptionContainsRequiredInfo()
    {
        ValidationFailure result =_date.Validate("banana");
            
        Assert.NotNull(result.SourceConstraint);
        Assert.AreEqual(typeof(Date), result.SourceConstraint.GetType());
            
    }

    // utility methods

    private static IPrimaryConstraint constraint_date()
    {
        return (IPrimaryConstraint)Validator.CreateConstraint("date",Consequence.Wrong);
    }

}