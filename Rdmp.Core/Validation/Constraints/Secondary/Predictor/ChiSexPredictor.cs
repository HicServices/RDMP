// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Validation.Constraints.Secondary.Predictor;

/// <summary>
///     Validation rule that checks that the second from last digit in a CHI matches the patient gender.  CHI numbers
///     second last digit should be even for
///     females and odd for males.
/// </summary>
public class ChiSexPredictor : PredictionRule
{
    public override ValidationFailure Predict(IConstraint parent, object oChi, object oGender)
    {
        if (oChi == null || oGender == null) // Null is valid
            return null;

        var sex = oGender switch
        {
            string s => char.ToUpperInvariant(s[0]),
            char gender => char.ToUpperInvariant(gender),
            _ => throw new ArgumentException($"Gender must be a string or char, gender value is a {oGender.GetType()}")
        };

        var sChi = oChi as string ??
                   throw new ArgumentException(
                       $"Chi was not a string (or null) object.  It was of Type {oChi.GetType()}");
        if (sChi.Length == 10)
        {
            var sexDigit = (int)char.GetNumericValue(sChi, 8);

            if ((sex == 'M' && (sexDigit & 1) == 0) || (sex == 'F' && (sexDigit & 1) == 1))
                return new ValidationFailure(
                    $"CHI sex indicator ({sexDigit})  did not match associated sex field ({sex})", parent);
        }

        //invalid chi, who cares
        return null;
    }
}