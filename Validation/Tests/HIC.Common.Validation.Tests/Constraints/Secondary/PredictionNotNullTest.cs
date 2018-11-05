using HIC.Common.Validation;
using HIC.Common.Validation.Constraints.Secondary.Predictor;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Secondary
{
    
    class PredictionNotNullTest
    {

        [Test]
        public void Validate_ValueNotNullAndRelatedValueNotNull_Succeeds()
        {

            var p = new Prediction(new ValuePredictsOtherValueNullness(), "someColumn");
            var otherCols = new object[] { "not null" };
            var otherColsNames = new string[] { "someColumn" };
            Assert.IsNull(p.Validate("this is not null", otherCols, otherColsNames));
        }

        [Test]
        public void Validate_ValueNotNullAndRelatedValueIsNull_ThrowsException()
        {
            var p = new Prediction(new ValuePredictsOtherValueNullness(), "someColumn");
            var otherCols = new object[] { null };
            var otherColsNames = new string[] { "someColumn" };
            Assert.NotNull(p.Validate("this is not null", otherCols, otherColsNames));
        }

        [Test]
        public void Validate_ValueIsNullAndRelatedValueNotNull_Succeeds()
        {
            var p = new Prediction(new ValuePredictsOtherValueNullness(), "someColumn");
            var otherCols = new object[] { "not null" };
            var otherColsNames = new string[] { "someColumn" };
            Assert.IsNull(p.Validate(null, otherCols, otherColsNames));
        }

        [Test]
        public void Validate_ValueIsNullAndRelatedValueIsNull_Succeeds()
        {
            var p = new Prediction(new ValuePredictsOtherValueNullness(), "someColumn");
            var otherCols = new object[] { null };
            var otherColsNames = new string[] { "someColumn" };
            Assert.IsNull(p.Validate(null, otherCols, otherColsNames));
        }

    }
}
