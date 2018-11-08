using System.Collections.Generic;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using HIC.Common.Validation.Constraints.Secondary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Primary
{

    
    public class ChiValidationTest
    {

        [Test]
        public void validation_scenario_CHI()
        {
            // 1. Create a new Validator - this is responsible for validating the entire target object (dictionary)
            var validator = new Validator();

            // 2. Create new ItemValidator - this is respoonsible for validating an individual item in the target object
            var chi = new ItemValidator();

            // 3. Set the ItemValidator's PrimaryConstraint (must be valid CHI)
            // (using Validator's CreateConstraint() method to create a Primary Constraint (CHI))
            chi.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi", Consequence.Wrong);

            // 4. Add the ItemValidator to our Validator, specifying the item it should validate against
            validator.AddItemValidator(chi, "chi", typeof(string));

            // 5. Create a target object (dictionary) against which to validate
            var domainObject = new Dictionary<string, object>();
            domainObject.Add("chi", TestConstants._VALID_CHI);

            // 6. Validate, passing in the target object to be validated against - ValidationFailure or null is returned
            Assert.IsNull(validator.Validate(domainObject));
        }

        [Test]
        public void validation_scenario_CHI_and_age()
        {
            // 1. Create a new Validator - this is responsible for validating the entire target object (dictionary)
            var validator = new Validator();

            // 2. Create new ItemValidator - this is respoonsible for validating an individual item in the target object
            var chi = new ItemValidator();

            // 3. Set the ItemValidator's PrimaryConstraint (must be valid CHI)
            // (using Validator's CreateConstraint() method to create a Primary Constraint (CHI))
            chi.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi",Consequence.Wrong);

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
            var domainObject = new Dictionary<string, object>();
            domainObject.Add("chi", TestConstants._VALID_CHI);
            domainObject.Add("age", 12);

            // 9. Validate, passing in the target object to be validated against
            Assert.IsNull(validator.Validate(domainObject));
        }

    }
}
