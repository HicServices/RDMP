using System;
using System.Collections.Generic;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Secondary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Secondary
{
    [TestFixture]
    class BoundDateTest
    {
        [Test]
        public void Validate_IsValid_Succeeds()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);

            b.LowerFieldName = "dob";
            b.Upper = DateTime.MaxValue;

            ValidationFailure result = CallValidateOnValidData("admission_date", b);

            Assert.IsNull(result);
        }

        [Test]
        public void Validate_DateIsSame_Succeeds()
        {
            var b = (BoundDate) Validator.CreateConstraint("bounddate",Consequence.Wrong);

            Assert.IsTrue(b.Inclusive);


            object[] cols = new object[] { DateTime.Parse("2007-10-09 00:00:00.0000000") };
            string[] names = new string[]{"dob2"};
            b.LowerFieldName = "dob2";

            Assert.IsNull(b.Validate(DateTime.Parse("2007-10-09 00:00:00.0000000"),cols,names));
        }

        [Test]
        public void Validate_IsInvalid_ThrowsException()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);

            b.LowerFieldName = "dob";
            b.Upper = DateTime.MaxValue;

            Assert.NotNull(CallValidateOnInvalidData("admission_date", b)); ;
        }


        [Test]
        public void Validate_IsInvalid_ThrowsExceptionWithConsequence()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Consequence = Consequence.InvalidatesRow;

            b.LowerFieldName = "dob";
            b.Upper = DateTime.MaxValue;
            
            ValidationFailure result = CallValidateOnInvalidData("admission_date", b);

            if(result == null)
              Assert.Fail("Expected validation exception, but none came");


            Assert.NotNull(result.SourceConstraint);
            Assert.AreEqual(result.SourceConstraint.Consequence, Consequence.InvalidatesRow);
            
            
        }


        [Test]
        public void Validate_IsValidButNull_Succeeds()
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);

            b.UpperFieldName = "appointmentDate";

            object[] cols = new object[] { DBNull.Value};

            String[] names= new string[]{"appointmentDate"};

            b.Validate(null,cols,names);
        }


        private ValidationFailure CallValidateOnValidData(string targetProperty, BoundDate b)
        {
            Dictionary<string, object> d = TestConstants.AdmissionDateOccursAfterDob;
            return CallValidate(targetProperty, b, d);
        }

        private ValidationFailure CallValidateOnInvalidData(string targetProperty, BoundDate b)
        {
            Dictionary<string, object> d = TestConstants.AdmissionDateOccursBeforeDob;
            return CallValidate(targetProperty, b, d);
        }



        private static ValidationFailure CallValidate(string targetProperty, BoundDate b, Dictionary<string, object> d)
        {
            var keys = new string[d.Keys.Count];
            var vals = new object[d.Values.Count];
            d.Keys.CopyTo(keys, 0);
            d.Values.CopyTo(vals, 0);

            object o;
            d.TryGetValue(targetProperty, out o);

            return b.Validate(o, vals, keys);
        }
    }
}
