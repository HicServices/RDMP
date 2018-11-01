using System;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests
{
    /*
     * Unit-tests for the ItemValidator.
     * Using CHI in this case.
     * 
     * Note: Normally the Host Validator instance would set the TargetProprty of its ItemValidator(s).
     * Here, we do it explicitly in the SetUp() method.
     * 
     */

    
    public class ItemValidatorTest
    {
        private ItemValidator _v;

        [SetUp]
        public void SetUp()
        {
            _v = new ItemValidator();
            _v.TargetProperty = "chi";
        }

        [Test]
        public void ValidateAll_IsTypeIncompatible_ThrowsException()
        {
            _v.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi",Consequence.Wrong);
            _v.ExpectedType = typeof(int);

            Assert.NotNull(_v.ValidateAll(DateTime.Now,  new object[0], new string[0]));
        }

        [Test]
        public void ValidateAll_IsTypeIncompatible_GivesReason()
        {
            _v.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi",Consequence.Wrong);
            _v.ExpectedType = typeof(DateTime);
            
            ValidationFailure result = _v.ValidateAll(DateTime.Now, new object[0], new string[0]);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Message.StartsWith("Incompatible type"));
            Assert.IsTrue(result.Message.Contains(typeof(DateTime).Name));
            
        }
        
        [Test]
        public void ValidateAll_ValidData_Succeeds()
        {
            _v.PrimaryConstraint = new Chi();
         
            Assert.IsNull(_v.ValidateAll(TestConstants._VALID_CHI, new object[0], new string[0]));
        }

        [Test]
        public void ValidateAll_InvalidData_ThrowsException()
        {
            _v.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi",Consequence.Wrong);

            Assert.NotNull(_v.ValidateAll(TestConstants._INVALID_CHI_CHECKSUM, new object[0], new string[0]));
        }

        [Test]
        public void ValidateAll_InvalidData_GivesReason()
        {
            _v.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint("chi",Consequence.Wrong);

            ValidationFailure result = _v.ValidateAll(TestConstants._INVALID_CHI_CHECKSUM, new object[0], new string[0]);
            
            Assert.AreEqual("CHI check digit did not match", result.Message);
            
        }


    
    }

}
