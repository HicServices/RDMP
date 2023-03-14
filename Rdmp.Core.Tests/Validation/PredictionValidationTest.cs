// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints.Primary;
using Rdmp.Core.Validation.Constraints.Secondary;
using Rdmp.Core.Validation.Constraints.Secondary.Predictor;

namespace Rdmp.Core.Tests.Validation
{

    [Category("Unit")]
    class PredictionValidationTest
    {

        #region Test arguments

        [TestCase("UNKNOWN")]
        [TestCase("Gender")]
        public void Validate_NullTargetField_GeneratesException(string targetField)
        {
            var prediction = new Prediction(new ChiSexPredictor(), targetField);
            var v = CreateInitialisedValidator(prediction);

            var ex = Assert.Throws<InvalidOperationException>(()=>v.Validate(TestConstants.ValidChiAndInconsistentSex));
            Assert.IsInstanceOf<MissingFieldException>(ex?.InnerException);
        }

        [Test]
        public void Validate_NullRule_GeneratesException()
        {
            var ex = Assert.Throws<ArgumentException>(()=>_=new Prediction(null, "gender"));
            StringAssert.Contains("You must specify a PredictionRule to follow",ex?.Message);
        }

        [Test]
        public void Validate_Uninitialized_GeneratesException()
        {
            var prediction = new Prediction();
            var v = CreateInitialisedValidator(prediction);

            var ex = Assert.Throws<InvalidOperationException>(()=>v.Validate(TestConstants.ValidChiAndInconsistentSex));
        }

        [Test]
        public void Validate_UninitializedTarget_GeneratesException()
        {
            var prediction = new Prediction
            {
                Rule = new ChiSexPredictor()
            };
            var v = CreateInitialisedValidator(prediction);
            var ex = Assert.Throws<InvalidOperationException>(()=>v.Validate(TestConstants.ValidChiAndInconsistentSex));
            Assert.IsInstanceOf<InvalidOperationException>(ex?.InnerException);
        }

        [Test]
        public void Validate_UninitializedRule_GeneratesException()
        {
            var prediction = new Prediction
            {
                TargetColumn = "chi"
            };
            var v = CreateInitialisedValidator(prediction);
            var ex = Assert.Throws<InvalidOperationException>(()=>v.Validate(TestConstants.ValidChiAndInconsistentSex));
        }
        #endregion

        #region Test CHI - with primary constraint & secondary constraint

        [Test]
        public void Validate_ChiHasConsistentSexIndicator_Valid()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidator(prediction);

            Assert.IsNull(v.Validate(TestConstants.ValidChiAndConsistentSex));
        }
        
        [Test]
        public void Validate_ChiIsNull_Valid()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidator(prediction);

            Assert.IsNull(v.Validate(TestConstants.NullChiAndValidSex));
        }

        [Test]
        public void Validate_SexIsNull_Valid()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidator(prediction);

            Assert.IsNull(v.Validate(TestConstants.NullChiAndNullSex));
        }

        [Test]
        public void Validate_CHIHasInconsistentSexIndicator_Invalid()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidator(prediction);

            Assert.NotNull(v.Validate(TestConstants.ValidChiAndInconsistentSex));
        }

        [Test]
        public void Validate_ChiIsInvalid_Invalid()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidator(prediction);

            Assert.NotNull(v.Validate(TestConstants.InvalidChiAndValidSex));
        }

        #endregion

        #region Test CHI - with primary constraint & secondary constraint

        [Test]
        public void Validate_NoPrimaryConstraintChiHasConsistentSexIndicator_Valid()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidatorWithNoPrimaryConstraint(prediction);

            Assert.IsNull(v.Validate(TestConstants.ValidChiAndConsistentSex));
        }

        [Test]
        public void Validate_NoPrimaryConstraintChiIsNull_Valid()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidatorWithNoPrimaryConstraint(prediction);

            Assert.IsNull(v.Validate(TestConstants.NullChiAndNullSex));
        }

        [Test]
        public void Validate_NoPrimaryConstraintCHIHasInconsistentSexIndicator_Invalid()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidatorWithNoPrimaryConstraint(prediction);

            Assert.NotNull(v.Validate(TestConstants.ValidChiAndInconsistentSex));
        }

        [Test]
        public void Validate_NoPrimaryConstraintChiIsInvalid_ValidBecauseWhoCaresIfChiIsInvalid_IfYouDoCareUseAChiPrimaryConstraintInstead()
        {
            var prediction = new Prediction(new ChiSexPredictor(), "gender");
            var v = CreateInitialisedValidatorWithNoPrimaryConstraint(prediction);

            Assert.Null(v.Validate(TestConstants.InvalidChiAndValidSex));
        }

        #endregion
        
        private static Validator CreateInitialisedValidator(SecondaryConstraint prediction)
        {
            var i = new ItemValidator
            {
                PrimaryConstraint = new Chi()
            };
            i.SecondaryConstraints.Add(prediction);

            var v = new Validator();
            v.AddItemValidator(i, "chi", typeof(string));
            return v;
        }

        private static Validator CreateInitialisedValidatorWithNoPrimaryConstraint(SecondaryConstraint prediction)
        {
            var i = new ItemValidator();
            i.SecondaryConstraints.Add(prediction);
            
            var v = new Validator();
            v.AddItemValidator(i, "chi", typeof(string));
            return v;
        }
    }
}
