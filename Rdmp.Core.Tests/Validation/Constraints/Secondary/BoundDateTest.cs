// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Secondary;

namespace Rdmp.Core.Tests.Validation.Constraints.Secondary;

[Category("Unit")]
internal class BoundDateTest
{
    [Test]
    public void Validate_IsValid_Succeeds()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);

        b.LowerFieldName = "dob";
        b.Upper = DateTime.MaxValue;

        var result = CallValidateOnValidData("admission_date", b);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Validate_DateIsSame_Succeeds()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);

        Assert.That(b.Inclusive);


        var cols = new object[] { DateTime.Parse("2007-10-09 00:00:00.0000000") };
        var names = new string[] { "dob2" };
        b.LowerFieldName = "dob2";

        Assert.That(b.Validate(DateTime.Parse("2007-10-09 00:00:00.0000000"), cols, names), Is.Null);
    }

    [Test]
    public void Validate_IsInvalid_ThrowsException()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);

        b.LowerFieldName = "dob";
        b.Upper = DateTime.MaxValue;

        Assert.That(CallValidateOnInvalidData("admission_date", b), Is.Not.Null);
    }


    [Test]
    public void Validate_IsInvalid_ThrowsExceptionWithConsequence()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Consequence = Consequence.InvalidatesRow;

        b.LowerFieldName = "dob";
        b.Upper = DateTime.MaxValue;

        var result = CallValidateOnInvalidData("admission_date", b);

        if (result == null)
            Assert.Fail("Expected validation exception, but none came");


        Assert.That(result?.SourceConstraint, Is.Not.Null);
        Assert.That(result.SourceConstraint.Consequence, Is.EqualTo(Consequence.InvalidatesRow));
    }


    [Test]
    public void Validate_IsValidButNull_Succeeds()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);

        b.UpperFieldName = "appointmentDate";

        var cols = new object[] { DBNull.Value };

        var names = new string[] { "appointmentDate" };

        b.Validate(null, cols, names);
    }


    private static ValidationFailure CallValidateOnValidData(string targetProperty, BoundDate b)
    {
        var d = TestConstants.AdmissionDateOccursAfterDob;
        return CallValidate(targetProperty, b, d);
    }

    private static ValidationFailure CallValidateOnInvalidData(string targetProperty, BoundDate b)
    {
        var d = TestConstants.AdmissionDateOccursBeforeDob;
        return CallValidate(targetProperty, b, d);
    }


    private static ValidationFailure CallValidate(string targetProperty, BoundDate b, Dictionary<string, object> d)
    {
        var keys = new string[d.Keys.Count];
        var vals = new object[d.Values.Count];
        d.Keys.CopyTo(keys, 0);
        d.Values.CopyTo(vals, 0);

        d.TryGetValue(targetProperty, out var o);

        return b.Validate(o, vals, keys);
    }
}