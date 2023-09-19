// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rdmp.Core.Tests.Validation.TestData;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;
using Rdmp.Core.Validation.Constraints.Secondary;

namespace Rdmp.Core.Tests.Validation;

[Category("Unit")]
public class ValidatorTest
{
    private readonly ChiDomainObject _domainObjectWithValidChi = new(TestConstants._VALID_CHI);
    private readonly ChiDomainObject _domainObjectWithInvalidChi = new(TestConstants._INVALID_CHI_CHECKSUM);
    private readonly ChiAgeDomainObject _domainObjectWithValidChiAndAge = new(TestConstants._VALID_CHI, 33);

    [Test]
    public void Validate_InitialisedState_IsValid()
    {
        var validator = new Validator();

        Assert.IsNull(validator.Validate(_domainObjectWithValidChi));
    }

    [Test]
    public void GetItemValidator_InitialisedState_ReturnsNullItemValidator()
    {
        var validator = new Validator();
        var itemValidator = validator.GetItemValidator("non-existent");

        Assert.Null(itemValidator);
    }

    [Test]
    public void PassValidatorArray_Passes()
    {
        var v = new Validator();
        v.AddItemValidator(new ItemValidator(), "chi", null);
        v.ItemValidators[0].PrimaryConstraint = new Chi();

        var dt = new DataTable();
        dt.Columns.Add("chi");
        var dr = dt.Rows.Add();

        dr["chi"] = TestConstants._VALID_CHI;

        //validate the row
        Assert.IsNull(v.Validate(dr));
    }

    [Test]
    public void AddItemValidator_DuplicateCalls_ThrowsException()
    {
        var validator = new Validator();
        validator.AddItemValidator(new ItemValidator(), "foo", typeof(string));

        Assert.Throws<ArgumentException>(() => validator.AddItemValidator(new ItemValidator(), "foo", typeof(string)));
    }

    [Test]
    public void RemoveItemValidator_InitialisedState_BehavesAcceptably()
    {
        var validator = new Validator();
        Assert.False(validator.RemoveItemValidator("non-existent"),
            "Expected removal of a non-existent ItemValidator to return false.");
    }

    [Test]
    public void Validate_NonExistentTargetProperty_ThrowsException()
    {
        var validator = CreateValidatorForNonExistentProperty();

        var ex = Assert.Throws<MissingFieldException>(() => validator.Validate(_domainObjectWithValidChi));
        Assert.AreEqual("Validation failed: Target field [non-existent] not found in domain object.", ex.Message);
    }


    [Test]
    public void Validate_NonExistentTargetProperty_EmitsMessage()
    {
        var validator = CreateValidatorForNonExistentProperty();
        try
        {
            validator.Validate(_domainObjectWithValidChi);
            Assert.Fail($"Expecting a {typeof(ValidationFailure)}");
        }
        catch (MissingFieldException exception)
        {
            Assert.True(exception.Message.StartsWith("Validation failed"));
        }
    }

    [Test]
    public void Validate_ValidChi_Successful()
    {
        var validator = CreateSimpleChiValidator();

        Assert.IsNull(validator.Validate(_domainObjectWithValidChi));
    }

    [Test]
    public void Validate_InvalidChi_ThrowsException()
    {
        var validator = CreateSimpleChiValidator();

        Assert.NotNull(validator.Validate(_domainObjectWithInvalidChi));
    }


    [Test]
    public void ValidateVerbose_InvalidChi_CountOfWrongIncreases()
    {
        var validator = CreateSimpleChiValidator();
        //run once
        var results = validator.ValidateVerboseAdditive(_domainObjectWithInvalidChi, null, out _);

        Assert.IsNotNull(results);

        Assert.AreEqual(results.DictionaryOfFailure["chi"][Consequence.Wrong], 1);

        //additive --give it same row again, expect the count of wrong ones to go SetUp by 1
        results = validator.ValidateVerboseAdditive(_domainObjectWithInvalidChi, results, out _);

        Assert.AreEqual(results.DictionaryOfFailure["chi"][Consequence.Wrong], 2);
    }


    [Test]
    public void Test_XML_Generation()
    {
        var v = new Validator();
        var iv = new ItemValidator
        {
            TargetProperty = "Name",
            ExpectedType = typeof(string),
            PrimaryConstraint = new Alpha()
        };

        v.ItemValidators.Add(iv);

        var answer = v.SaveToXml(false);

        var v2 = Validator.LoadFromXml(answer);

        var answer2 = v2.SaveToXml(false);

        Assert.AreEqual(answer, answer2);
    }


    [Test]
    public void Validate_ValidChiAndAge_Successful()
    {
        var validator = CreateChiAndAgeValidators();

        Assert.IsNull(validator.Validate(_domainObjectWithValidChiAndAge));
    }

    [TestCase("chi", typeof(Chi))]
    [TestCase("date", typeof(Date))]
    public void CreatePrimaryConstraint_All_IsOfExpectedType(string name, Type expected)
    {
        var constraint = Validator.CreateConstraint(name, Consequence.Wrong);

        Assert.IsInstanceOf(typeof(IPrimaryConstraint), constraint);
        Assert.IsInstanceOf(expected, constraint);
    }

    [TestCase("bounddouble", typeof(BoundDouble))]
    [TestCase("bounddate", typeof(BoundDate))]
    [TestCase("notnull", typeof(NotNull))]
    [TestCase("regularexpression", typeof(RegularExpression))]
    public void CreateSecondaryConstraint_All_IsOfExpectedType(string name, Type expected)
    {
        var constraint = Validator.CreateConstraint(name, Consequence.Wrong);

        Assert.IsInstanceOf(typeof(ISecondaryConstraint), constraint);
        Assert.IsInstanceOf(expected, constraint);
    }

    [Test]
    public void RenameColumn_ThreeColumns_HasCorrectName()
    {
        var v = new Validator();
        v.AddItemValidator(new ItemValidator(), "OldCol2", typeof(string));


        //this constraint ensures that OldCol2 is between OldCol1 and OldcCol3
        var boundDate = new BoundDate
        {
            LowerFieldName = "OldCol1",
            UpperFieldName = "OldCol3"
        };

        v.ItemValidators[0].SecondaryConstraints.Add(boundDate);

        var dictionary  = new Dictionary<string, string> { { "OldCol2", "NewCol2" } };

        //before and after rename of col2
        Assert.AreEqual(v.ItemValidators[0].TargetProperty, "OldCol2");
        v.RenameColumns(dictionary);
        Assert.AreEqual(v.ItemValidators[0].TargetProperty, "NewCol2");
        Assert.AreEqual(((BoundDate)v.ItemValidators[0].SecondaryConstraints[0]).LowerFieldName, "OldCol1");
        Assert.AreEqual(((BoundDate)v.ItemValidators[0].SecondaryConstraints[0]).UpperFieldName, "OldCol3");

        //now rename col 1
        dictionary.Add("OldCol1", "NewCol1");
        v.RenameColumns(dictionary);
        Assert.AreEqual(v.ItemValidators[0].TargetProperty, "NewCol2");
        Assert.AreEqual(((BoundDate)v.ItemValidators[0].SecondaryConstraints[0]).LowerFieldName, "NewCol1");
        Assert.AreEqual(((BoundDate)v.ItemValidators[0].SecondaryConstraints[0]).UpperFieldName, "OldCol3");

        //finally rename col 3
        dictionary.Add("OldCol3", "NewCol3");
        v.RenameColumns(
            dictionary); //not strict because we will get not found otherwise since we already renamed the first one
        Assert.AreEqual(v.ItemValidators[0].TargetProperty, "NewCol2");
        Assert.AreEqual(((BoundDate)v.ItemValidators[0].SecondaryConstraints[0]).LowerFieldName, "NewCol1");
        Assert.AreEqual(((BoundDate)v.ItemValidators[0].SecondaryConstraints[0]).UpperFieldName, "NewCol3");
    }

    // This code is typically how a client of the API would set SetUp validation for a domain object:
    //
    // A domain object contains a number of items, each of which we may wish to validate.
    // A Validator is responsible for validating a domain object.
    // Any useful Validator contains at least one ItemValidator.
    // An ItemValidator is created for each item in the domain object you wish to validate.
    // An ItemValidator contains a single PrimaryConstraint (e.g. must be valid CHI) and zero or more secondary constraints.
    private static Validator CreateSimpleChiValidator()
    {
        // 1. Create a new Validator, passing in the domain object to be validated (and its type)
        var validator = new Validator();
        // 2. Create a new ItemValidator (in this case, must be valid CHI)
        var itemValidator = new ItemValidator
            { PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong) };
        itemValidator.PrimaryConstraint.Consequence = Consequence.Wrong;

        // 3. Add the ItemValidator, specifying the value in the domain object it should validate against
        validator.AddItemValidator(itemValidator, "chi", typeof(string));

        return validator;
    }

    private static Validator CreateChiAndAgeValidators()
    {
        var validator = new Validator();
        var vChi = new ItemValidator
            { PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong) };
        var vAge = new ItemValidator();
        var age = (BoundDouble)Validator.CreateConstraint("bounddouble", Consequence.Wrong);
        age.Lower = 0;
        age.Upper = 120;
        vAge.AddSecondaryConstraint(age);

        validator.AddItemValidator(vChi, "chi", typeof(string));
        validator.AddItemValidator(vAge, "age", typeof(int));

        return validator;
    }

    private static Validator CreateValidatorForNonExistentProperty()
    {
        var validator = new Validator();
        var itemValidator = new ItemValidator
            { PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong) };
        validator.AddItemValidator(itemValidator, "non-existent", typeof(string));

        return validator;
    }
}