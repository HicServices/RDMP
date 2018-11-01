using System;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Primary
{
    
    class DateEuTest
    {
        private IPrimaryConstraint _date;

        [SetUp]
        public void SetUp()
        {
            _date = constraint_date();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("banana")]
        [TestCase("092567")]
        [TestCase("5.9.677")]
        [TestCase("250967")]
        [TestCase("1967")]
        [TestCase("25/09")]
        [TestCase("90/67")]
        [TestCase("09/1967")]
        [TestCase("19670925")]
        public void Validate_InvalidDate_ThrowsException(string code)
        {
            Assert.NotNull(_date.Validate(code));
        }

        [TestCase(null)]
        [TestCase("25/09.67")]
        [TestCase("25/09-67")]
        [TestCase("25-09/67")]
        [TestCase("25.09-67")]
        [TestCase("25.09.67")]
        [TestCase("25.09.1967")]
        [TestCase("25.9.1967")]
        [TestCase("5.9.67")]
        [TestCase("5.12.67")]
        public void Validate_ValidDate_Success(string code)
        {
            Assert.IsNull(_date.Validate(code));
        }

        [Test]
        public void Validate_InvalidDate_ExceptionContainsRequiredInfo()
        {
            ValidationFailure result =_date.Validate("banana");
            
            Assert.NotNull(result.SourceConstraint);
            Assert.AreEqual(typeof(Date), result.SourceConstraint.GetType());
            
        }

        // utility methods

        private static IPrimaryConstraint constraint_date()
        {
            return (IPrimaryConstraint)Validator.CreateConstraint("date",Consequence.Wrong);
        }

    }
} 
