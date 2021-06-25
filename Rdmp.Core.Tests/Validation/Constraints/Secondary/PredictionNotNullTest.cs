// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Validation.Constraints.Secondary.Predictor;

namespace Rdmp.Core.Tests.Validation.Constraints.Secondary
{

    [Category("Unit")]
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
            StringAssert.StartsWith("Nullness did not match, when one value is null, the other mus", p.Validate(null, otherCols, otherColsNames)?.Message);
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
