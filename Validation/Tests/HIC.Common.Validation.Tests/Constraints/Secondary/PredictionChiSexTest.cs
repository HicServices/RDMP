using System;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Secondary.Predictor;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Secondary
{
    
    class PredictionChiSexTest
    {
        private readonly DateTime _wrongType = DateTime.Now;
        
        [Test]
        public void Validate_IncompatibleChiType_ThrowsException()
        {
            var p = new Prediction(new ChiSexPredictor(),"gender");
            Assert.Throws<ArgumentException>(()=>p.Validate(_wrongType, new[] { "M" }, new[] { "gender" }));
        }

        [Test]
        public void Validate_IncompatibleGenderType_ThrowsException()
        {
            var p = new Prediction(new ChiSexPredictor(), "gender");
            Assert.Throws<ArgumentException>(()=>p.Validate(TestConstants._VALID_CHI, new object[] { _wrongType }, new string[] { "gender" }));
        }

        [Test]
        public void Validate_NullChiAndGender_IsIgnored()
        {
            var p = new Prediction(new ChiSexPredictor(), "gender");
            Assert.Throws<ArgumentException>(()=>p.Validate(TestConstants._VALID_CHI, null, null));
        }

        [Test]
        public void Validate_TargetFieldNotPresent_ThrowsException()
        {
            var p = new Prediction(new ChiSexPredictor(), "gender");
            var otherCols = new object[] {"M"};
            var otherColsNames = new string[] {"amagad"};
            Assert.Throws<MissingFieldException>(()=>p.Validate(TestConstants._VALID_CHI, otherCols, otherColsNames));
        }

        [Test]
        public void Validate_ConsistentChiAndSex_String_Succeeds()
        {
            var p = new Prediction(new ChiSexPredictor(), "gender");
            var otherCols = new object[] { "M" };
            var otherColsNames = new string[] { "gender" };
            p.Validate(TestConstants._VALID_CHI, otherCols, otherColsNames);
        }
        [Test]
        public void Validate_ConsistentChiAndSex_Char_Succeeds()
        {
            var p = new Prediction(new ChiSexPredictor(), "gender");
            var otherCols = new object[] { 'M' };
            var otherColsNames = new string[] { "gender" };
            p.Validate(TestConstants._VALID_CHI, otherCols, otherColsNames);
        }

        [Test]
        public void Validate_InconsistentChiAndSex_ThrowsException()
        {
            var p = new Prediction(new ChiSexPredictor(), "gender");
            var otherCols = new object[] { "F" };
            var otherColsNames = new string[] { "gender" };
            Assert.NotNull(p.Validate(TestConstants._VALID_CHI, otherCols, otherColsNames));
        }

        [Test]
        public void Validate_ChiAndUnspecifiedGender_Ignored()
        {
            var p = new Prediction(new ChiSexPredictor(), "gender");
            var otherCols = new object[] { "U" };
            var otherColsNames = new string[] { "gender" };
            p.Validate(TestConstants._VALID_CHI, otherCols, otherColsNames);
        }

        [Test]
        public void Validate_ChiAndNullGender_Ignored()
        {
            var p = new Prediction(new ChiSexPredictor(), "gender");
            var otherCols = new object[] { null };
            var otherColsNames = new string[] { "gender" };
            p.Validate(TestConstants._VALID_CHI, otherCols, otherColsNames);
        }
    }
}
