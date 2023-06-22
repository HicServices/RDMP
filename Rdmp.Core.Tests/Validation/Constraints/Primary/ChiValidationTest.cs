// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;
using Rdmp.Core.Validation.Constraints.Secondary;

namespace Rdmp.Core.Tests.Validation.Constraints.Primary;

[Category("Unit")]
internal class ChiValidationTest : ValidationTests
{

    [Test]
    public void validation_scenario_CHI()
    {
        // 1. Create a new Validator - this is responsible for validating the entire target object (dictionary)
        var validator = new Validator();

        // 2. Create new ItemValidator - this is respoonsible for validating an individual item in the target object
        var chi = new ItemValidator
        {
            // 3. Set the ItemValidator's PrimaryConstraint (must be valid CHI)
            // (using Validator's CreateConstraint() method to create a Primary Constraint (CHI))
            PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong)
        };

        // 4. Add the ItemValidator to our Validator, specifying the item it should validate against
        validator.AddItemValidator(chi, "chi", typeof(string));

        // 5. Create a target object (dictionary) against which to validate
        var domainObject = new Dictionary<string, object> { { "chi", TestConstants._VALID_CHI } };

        // 6. Validate, passing in the target object to be validated against - ValidationFailure or null is returned
        Assert.IsNull(validator.Validate(domainObject));
    }

    [Test]
    public void validation_scenario_CHI_and_age()
    {
        // 1. Create a new Validator - this is responsible for validating the entire target object (dictionary)
        var validator = new Validator();

        // 2. Create new ItemValidator - this is respoonsible for validating an individual item in the target object
        var chi = new ItemValidator
        {
            // 3. Set the ItemValidator's PrimaryConstraint (must be valid CHI)
            // (using Validator's CreateConstraint() method to create a Primary Constraint (CHI))
            PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi",Consequence.Wrong)
        };

        // 4. Add the ItemValidator to our Validator, specifying the item it should validate against
        validator.AddItemValidator(chi, "chi", typeof(string));

        // 5. Create a new ItemValidator (in this case, must be valid CHI and sensible age value)
        var age = new ItemValidator();

        // 6. No PrimaryConstraint. In this case we ADD a SecondaryConstraint (age)
        var ageConstraint = (BoundDouble)Validator.CreateConstraint("bounddouble",Consequence.Wrong);
        ageConstraint.Lower = 0;
        ageConstraint.Upper = 30;
            
        age.AddSecondaryConstraint(ageConstraint);

        // 7. Add the ItemValidator to our Validator, specifying the item it should validate against
        validator.AddItemValidator(age, "age", typeof(int));

        // 8. Create a target object (dictionary) against which to validate
        var domainObject = new Dictionary<string, object>
        {
            { "chi", TestConstants._VALID_CHI },
            { "age", 12 }
        };

        // 9. Validate, passing in the target object to be validated against
        Assert.IsNull(validator.Validate(domainObject));
    }

}