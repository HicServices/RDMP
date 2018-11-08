using System;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Primary
{
    
    class AlphaNumericTest
    {
        private IPrimaryConstraint _alphanum;

        [SetUp]
        public void SetUp()
        {
            _alphanum = (IPrimaryConstraint)Validator.CreateConstraint("alphanumeric", Consequence.Wrong);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("A1 ")]
        [TestCase("A ")]
        [TestCase(" 1")]
        public void Validate_Invalid_ThrowsException(string code)
        {
            Assert.NotNull(_alphanum.Validate(code));
        }

        [TestCase(null)]
        [TestCase("1A")]
        [TestCase("a1")]
        [TestCase("1")]
        [TestCase("1b2")]
        [TestCase("aaaaaa")]
        [TestCase("AAAAAA")]
        public void Validate_Valid_Success(string code)
        {
            Assert.IsNull(_alphanum.Validate(code));
        }

        [Test]
        public void Validate_Invalid_ExceptionContainsRequiredInfo()
        {
            ValidationFailure result = _alphanum.Validate(" ");
            
            Assert.NotNull(result.SourceConstraint);
            Assert.AreEqual(typeof(AlphaNumeric), result.SourceConstraint.GetType());
            
        }

    }
} 
