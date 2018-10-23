using System;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Primary
{
    
    class ChiTest
    {
        private IPrimaryConstraint _chi;

        [SetUp]
        public void SetUp()
        {
            _chi = (IPrimaryConstraint)Validator.CreateConstraint("chi",Consequence.Wrong);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("banana")]
        [TestCase("092567")]
        [TestCase("020445037")]
        [TestCase("000000000")]
        [TestCase("02044503731")]
        public void Validate_InvalidChi_ThrowsException(string code)
        {
            Assert.NotNull(_chi.Validate(code));
        }

        [TestCase(null)]
        [TestCase("0204450373")]
        public void Validate_ValidChi_Success(string code)
        {
            Assert.IsNull(_chi.Validate(code));
        }

        [Test]
        public void Validate_InvalidChi_ExceptionContainsRequiredInfo()
        {
            ValidationFailure result = _chi.Validate("banana");
            
            Assert.NotNull(result.SourceConstraint);
            Assert.AreEqual(typeof(Chi), result.SourceConstraint.GetType());
            
        }

    }
} 
