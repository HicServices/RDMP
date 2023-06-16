// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;

namespace Rdmp.Core.Tests.Validation;
/*
 * Unit-tests for the ItemValidator.
 * Using CHI in this case.
 *
 * Note: Normally the Host Validator instance would set the TargetProprty of its ItemValidator(s).
 * Here, we do it explicitly in the SetUp() method.
 *
 */

[Category("Unit")]
public class ItemValidatorTest
{
    private ItemValidator _v;

    [SetUp]
    public void SetUp()
    {
        _v = new ItemValidator
        {
            TargetProperty = "chi"
        };
    }

    [Test]
    public void ValidateAll_IsTypeIncompatible_ThrowsException()
    {
        _v.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong);
        _v.ExpectedType = typeof(int);

        Assert.NotNull(_v.ValidateAll(DateTime.Now, Array.Empty<object>(), Array.Empty<string>()));
    }

    [Test]
    public void ValidateAll_IsTypeIncompatible_GivesReason()
    {
        _v.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong);
        _v.ExpectedType = typeof(DateTime);

        var result = _v.ValidateAll(DateTime.Now, Array.Empty<object>(), Array.Empty<string>());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Message.StartsWith("Incompatible type"));
        Assert.IsTrue(result.Message.Contains(nameof(DateTime)));
            
    }

    [Test]
    public void ValidateAll_ValidData_Succeeds()
    {
        _v.PrimaryConstraint = new Chi();

        Assert.IsNull(_v.ValidateAll(TestConstants._VALID_CHI, Array.Empty<object>(), Array.Empty<string>()));
    }

    [Test]
    public void ValidateAll_InvalidData_ThrowsException()
    {
        _v.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong);

        Assert.NotNull(
            _v.ValidateAll(TestConstants._INVALID_CHI_CHECKSUM, Array.Empty<object>(), Array.Empty<string>()));
    }

    [Test]
    public void ValidateAll_InvalidData_GivesReason()
    {
        _v.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong);

        var result = _v.ValidateAll(TestConstants._INVALID_CHI_CHECKSUM, Array.Empty<object>(), Array.Empty<string>());

        Assert.AreEqual("CHI check digit did not match", result.Message);
    }
}