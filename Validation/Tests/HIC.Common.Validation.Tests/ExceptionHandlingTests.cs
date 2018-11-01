using System.Collections.Generic;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using HIC.Common.Validation.Constraints.Secondary;
using HIC.Common.Validation.Constraints.Secondary.Predictor;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests
{
    
    class ExceptionHandlingTests
    {

        [Test]
        public void Validate_WhenMultipleErrors_ReturnsAllErrors()
        {
            var validator = new Validator();
            
            var chi = new ItemValidator();
            chi.PrimaryConstraint = (PrimaryConstraint) Validator.CreateConstraint("chi",Consequence.Wrong);
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            chi.AddSecondaryConstraint(prediction);
            validator.AddItemValidator(chi, "chi", typeof(string));

            var age = new ItemValidator();
            BoundDouble ageConstraint = (BoundDouble)Validator.CreateConstraint("bounddouble",Consequence.Wrong);
            ageConstraint.Lower = 0;
            ageConstraint.Upper = 30;
            age.AddSecondaryConstraint(ageConstraint);
            validator.AddItemValidator(age, "age", typeof(int));

            var row = new Dictionary<string, object>();
            row.Add("chi", TestConstants._INVALID_CHI_CHECKSUM);
            row.Add("age", 31);
            row.Add("gender", "F");

            ValidationFailure result =  validator.Validate(row);

            Assert.AreEqual(2, result.GetExceptionList().Count);
            

        }
    }
}
