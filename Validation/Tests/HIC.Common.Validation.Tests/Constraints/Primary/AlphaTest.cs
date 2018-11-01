using System;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Primary
{
    
    class AlphaTest
    {
        private IPrimaryConstraint _alpha;

        [SetUp]
        public void SetUp()
        {
            _alpha = (IPrimaryConstraint)Validator.CreateConstraint("alpha", Consequence.Wrong);
        }

        [TestCase("12345")]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("A1")]
        [TestCase("A1 ")]
        public void Validate_Invalid_ThrowsException(string code)
        {
            Assert.NotNull(_alpha.Validate(code));
        }

        [TestCase(null)]
        [TestCase("A")]
        [TestCase("a")]
        [TestCase("Ab")]
        [TestCase("Ba")]
        [TestCase("aaaaaa")]
        [TestCase("AAAAAA")]
        public void Validate_Valid_Success(string code)
        {
            Assert.IsNull(_alpha.Validate(code));
        }

        [Test]
        public void Validate_Invalid_ExceptionContainsRequiredInfo()
        {
            
            ValidationFailure result = _alpha.Validate("9");
            
            Assert.NotNull(result.SourceConstraint);
            Assert.AreEqual(typeof(Alpha), result.SourceConstraint.GetType());
            
        }

    }
} 
