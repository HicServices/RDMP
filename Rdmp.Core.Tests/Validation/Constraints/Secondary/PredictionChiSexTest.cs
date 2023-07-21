// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Validation.Constraints.Secondary.Predictor;

namespace Rdmp.Core.Tests.Validation.Constraints.Secondary;

[Category("Unit")]
internal class PredictionChiSexTest
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