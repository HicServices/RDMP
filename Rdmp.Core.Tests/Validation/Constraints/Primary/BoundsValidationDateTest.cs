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

namespace Rdmp.Core.Tests.Validation.Constraints.Primary;

[Category("Unit")]
internal class BoundsValidationDateTest: ValidationTests
{
    private Dictionary<string, object> _d;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _d = new Dictionary<string, object> { { "somedate", new DateTime(2013, 06, 13) } };
    }

    #region Literal Dates

    [Test]
    public void must_occur_between_two_literal_dates_VALID()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.Upper = DateTime.MaxValue;
        var v = CreateLiteralDateValidator(b);

        Assert.IsNull(v.Validate(_d));
    }

    [Test]
    public void must_occur_between_two_literal_dates_INVALID_after()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.Upper = DateTime.MinValue.AddYears(1);
        var v = CreateLiteralDateValidator(b);

        Assert.NotNull(v.Validate(_d));
    }

    [Test]
    public void must_occur_between_two_literal_dates_INVALID_before()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MaxValue.AddYears(-1);
        b.Upper = DateTime.MaxValue;
        var v = CreateLiteralDateValidator(b);

        Assert.NotNull(v.Validate(_d));
    }

    [Test]
    public void must_occur_between_two_literal_dates_INVALID_onlower()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = new DateTime(2013, 06, 13);
        b.Upper = DateTime.MaxValue;
        b.Inclusive = false;

        var v = CreateLiteralDateValidator(b);

        Assert.NotNull(v.Validate(_d));
    }

    [Test]
    public void must_occur_between_two_literal_dates_INVALID_onupper()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.Upper = new DateTime(2013, 06, 13);
        b.Inclusive = false;
        var v = CreateLiteralDateValidator(b);

        Assert.NotNull(v.Validate(_d));
    }

    [Test]
    public void must_occur_inclusively_between_two_literal_dates_VALID_onlower()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = new DateTime(2013, 06, 13);
        b.Upper = DateTime.MaxValue;
        b.Inclusive = true;
        var v = CreateLiteralDateValidator(b);

        Assert.IsNull(v.Validate(_d));
    }

    [Test]
    public void must_occur_inclusively_between_two_literal_dates_VALID_onupper()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.Upper = new DateTime(2013, 06, 13);
        b.Inclusive = true;
        var v = CreateLiteralDateValidator(b);

        Assert.IsNull(v.Validate(_d));
    }

    #endregion

    #region Other Date Fields

    [Test]
    public void must_occur_after_field_VALID()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "dob";
        b.Upper = DateTime.MaxValue;

        var v = CreateAdmissionDateValidator(b);

        Assert.IsNull(v.Validate(TestConstants.AdmissionDateOccursAfterDob));
    }


    [Test]
    public void must_occur_after_field_INVALID_before()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "dob";
        b.Upper = DateTime.MaxValue;

        var v = CreateAdmissionDateValidator(b);

        Assert.NotNull(v.Validate(TestConstants.AdmissionDateOccursBeforeDob));
    }

    [Test]
    public void must_occur_after_field_INVALID_same()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "dob";
        b.Upper = DateTime.MaxValue;
        b.Inclusive = false;
        var v = CreateAdmissionDateValidator(b);

        Assert.NotNull(v.Validate(TestConstants.AdmissionDateOccursOnDob));
    }

    [Test]
    public void must_occur_after_field_INVALID_violation_report()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "dob";
        b.Upper = DateTime.MaxValue;

        var v = CreateAdmissionDateValidator(b);

        var result = v.Validate(TestConstants.AdmissionDateOccursBeforeDob);

        if (result == null)
            Assert.Fail();

        var l = result.GetExceptionList();

        StringAssert.EndsWith($"Expected a date greater than [{b.LowerFieldName}].", l[0].Message);
        Console.WriteLine(result.Message);
    }

    [Test]
    public void must_occur_before_field_VALID()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.UpperFieldName = "dob";

        var v = CreateParentDobValidator(b);

        Assert.IsNull(v.Validate(TestConstants.ParentDobOccursBeforeDob));
    }

    [Test]
    public void must_occur_before_field_INVALID_same()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.UpperFieldName = "dob";

        var v = CreateParentDobValidator(b);

        Assert.NotNull(v.Validate(TestConstants.ParentDobOccursOnDob));
    }

    [Test]
    public void must_occur_before_field_INVALID_after()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.UpperFieldName = "dob";

        var v = CreateParentDobValidator(b);

        Assert.NotNull(v.Validate(TestConstants.ParentDobOccursAfterDob));
    }

    [Test]
    public void must_occur_before_field_INVALID_violation_report()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.UpperFieldName = "dob";

        var v = CreateParentDobValidator(b);

        var result = v.Validate(TestConstants.ParentDobOccursAfterDob);

        if (result == null)
            Assert.Fail();

        var l = result.GetExceptionList();

        StringAssert.EndsWith($"Expected a date less than [{b.UpperFieldName}].", l[0].Message);
        Console.WriteLine(result.Message);
    }

    [Test]
    public void must_occur_between_fields_VALID()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "admission_date";
        b.UpperFieldName = "discharge_date";

        var v = CreateOperationDateValidator(b);

        Assert.IsNull(v.Validate(TestConstants.OperationOccursDuringStay));
    }

    [Test]
    public void must_occur_inclusively_between_fields_VALID_onstart()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "admission_date";
        b.UpperFieldName = "discharge_date";
        b.Inclusive = true;

        var v = CreateOperationDateValidator(b);

        Assert.IsNull(v.Validate(TestConstants.OperationOccursOnStartOfStay));
    }

    [Test]
    public void must_occur_inclusively_between_fields_VALID_onend()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "admission_date";
        b.UpperFieldName = "discharge_date";
        b.Inclusive = true;

        var v = CreateOperationDateValidator(b);

        Assert.IsNull(v.Validate(TestConstants.OperationOccursOnEndOfStay));
    }

    [Test]
    public void must_occur_between_fields_INVALID_onstart()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "admission_date";
        b.UpperFieldName = "discharge_date";
        b.Inclusive = false;
        var v = CreateOperationDateValidator(b);

        Assert.NotNull(v.Validate(TestConstants.OperationOccursOnStartOfStay));
    }

    [Test]
    public void must_occur_between_fields_INVALID_onend()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "admission_date";
        b.UpperFieldName = "discharge_date";
        b.Inclusive = false;
        var v = CreateOperationDateValidator(b);

        Assert.NotNull(v.Validate(TestConstants.OperationOccursOnEndOfStay));
    }

    [Test]
    public void must_occur_between_fields_INVALID_before()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "admission_date";
        b.UpperFieldName = "discharge_date";

        var v = CreateOperationDateValidator(b);

        Assert.NotNull(v.Validate(TestConstants.OperationOccursBeforeStay));
    }

    [Test]
    public void must_occur_between_fields_INVALID_after()
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.LowerFieldName = "admission_date";
        b.UpperFieldName = "discharge_date";

        var v = CreateOperationDateValidator(b);

        Assert.NotNull(v.Validate(TestConstants.OperationOccursAfterStay));
    }

    #endregion

    #region Fluent API experiment

    [Test]
    public void f_invalid_target_field_evokes_exception()
    {
        var v = new Validator();
        v.EnsureThatValue("INVALID").OccursAfter("dob");

        Assert.Throws<InvalidOperationException>(() => v.Validate(TestConstants.AdmissionDateOccursAfterDob));
    }

    [Test]
    public void f_invalid_comparator_field_evokes_exception()
    {
        var v = new Validator();
        v.EnsureThatValue("admission_date").OccursAfter("INVALID");

        Assert.Throws<InvalidOperationException>(() => v.Validate(TestConstants.AdmissionDateOccursAfterDob));
    }

    [Test]
    public void f_must_occur_after_field_VALID()
    {
        var v = new Validator();
        v.EnsureThatValue("admission_date").OccursAfter("dob");

        Assert.IsNull(v.Validate(TestConstants.AdmissionDateOccursAfterDob));
    }

    [Test]
    public void f_must_occur_after_field_INVALID_before()
    {
        var v = new Validator();
        v.EnsureThatValue("admission_date").OccursAfter("dob");

        Assert.NotNull(v.Validate(TestConstants.AdmissionDateOccursBeforeDob));
    }

    [Test]
    public void f_must_occur_after_field_INVALID_same()
    {
        var v = new Validator();
        v.EnsureThatValue("admission_date").OccursAfter("dob");

        Assert.NotNull(v.Validate(TestConstants.AdmissionDateOccursOnDob));
    }

    [Test]
    public void f_must_occur_before_field_VALID()
    {
        var v = new Validator();
        v.EnsureThatValue("parent_dob").OccursBefore("dob");

        Assert.IsNull(v.Validate(TestConstants.ParentDobOccursBeforeDob));
    }

    [Test]
    public void f_must_occur_before_field_INVALID_after()
    {
        var v = new Validator();
        v.EnsureThatValue("parent_dob").OccursBefore("dob");

        Assert.NotNull(v.Validate(TestConstants.ParentDobOccursAfterDob));
    }

    [Test]
    public void f_must_occur_before_field_INVALID_same()
    {
        var v = new Validator();
        v.EnsureThatValue("parent_dob").OccursBefore("dob");

        Assert.NotNull(v.Validate(TestConstants.ParentDobOccursOnDob));
    }

    #endregion

    #region Helper Methods

    private static Validator CreateLiteralDateValidator(BoundDate b)
    {
        var v = new Validator();
        var i = new ItemValidator();
        i.AddSecondaryConstraint(b);
        v.AddItemValidator(i, "somedate", typeof(DateTime));

        return v;
    }

    private static Validator CreateAdmissionDateValidator(BoundDate b)
    {
        var v = new Validator();
        var i = new ItemValidator();
        i.AddSecondaryConstraint(b);
        v.AddItemValidator(i, "admission_date", typeof(DateTime));

        return v;
    }

    private static Validator CreateParentDobValidator(BoundDate b)
    {
        var v = new Validator();
        var i = new ItemValidator();
        i.AddSecondaryConstraint(b);
        v.AddItemValidator(i, "parent_dob", typeof(DateTime));

        return v;
    }

    private static Validator CreateOperationDateValidator(BoundDate b)
    {
        var v = new Validator();
        var i = new ItemValidator();
        i.AddSecondaryConstraint(b);
        v.AddItemValidator(i, "operation_date", typeof(DateTime));

        return v;
    }

    #endregion
}