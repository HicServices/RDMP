using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Secondary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Secondary
{
    [TestFixture]
    class RegularExpressionTest
    {
        private RegularExpression _regex;

        [SetUp]
        public void SetUp()
        {
             _regex = (RegularExpression)Validator.CreateRegularExpression("^[MF]");
        }

        [Test]
        public void Validate_NullValue_IsIgnored()
        {
            Assert.IsNull(_regex.Validate(null, null, null));
        }

        [Test]
        public void Validate_EmpytyValue_Invalid()
        {
            Assert.NotNull(_regex.Validate("", null, null));
        }

        [Test]
        public void Validate_int_Succeeds()
        {
            _regex = new RegularExpression("^[0-9]+$");
            Assert.IsNull(_regex.Validate(5, null, null));
        }

        [Test]
        public void Validate_ValidValue_Valid()
        {
            Assert.IsNull(_regex.Validate("M", null, null));
        }

        [Test]
        public void Validate_InvalidValue_Invalid()
        {
            Assert.NotNull(_regex.Validate("INVALID", null, null));
        }
    }
}
