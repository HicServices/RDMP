// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints.Secondary;

namespace Rdmp.Core.Tests.Validation.Constraints.Secondary;

[Category("Unit")]
internal class RegularExpressionTest
{
    private RegularExpression _regex;

    [SetUp]
    public void SetUp()
    {
        _regex = (RegularExpression)Validator.CreateRegularExpression("^[MF]");
    }

    [Test]
    public void Validate_NullValue_IsIgnored()
    {
        Assert.That(_regex.Validate(null, null, null), Is.Null);
    }

    [Test]
    public void Validate_EmpytyValue_Invalid()
    {
        Assert.That(_regex.Validate("", null, null), Is.Not.Null);
    }

    [Test]
    public void Validate_int_Succeeds()
    {
        _regex = new RegularExpression("^[0-9]+$");
        Assert.That(_regex.Validate(5, null, null), Is.Null);
    }

    [Test]
    public void Validate_ValidValue_Valid()
    {
        Assert.That(_regex.Validate("M", null, null), Is.Null);
    }

    [Test]
    public void Validate_InvalidValue_Invalid()
    {
        Assert.That(_regex.Validate("INVALID", null, null), Is.Not.Null);
    }
}