// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;

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

        char sex;

        if (oGender is string s)
            sex = s.ToCharArray()[0];
        else if (oGender is char gender)
            sex = gender;
        else
            throw new ArgumentException($"Gender must be a string or char, gender value is a {oGender.GetType()}");

        if (oChi is not string sChi)
            throw new ArgumentException($"Chi was not a string (or null) object.  It was of Type {oChi.GetType()}");

        if (sChi.Length == 10)
        {
            var sexDigit = (int)char.GetNumericValue(sChi, 8);

            var isvalid = true;

            if (sex.ToString(CultureInfo.InvariantCulture).ToUpper() != "M" &&
                sex.ToString(CultureInfo.InvariantCulture).ToUpper() !=
                "F") // Pass as valid if sex is not strictly specified
                return null;

            if (sexDigit % 2 == 0 && sex == 'M')
                isvalid = false;

            if (sexDigit % 2 == 1 && sex == 'F')
                isvalid = false;

            if (!isvalid)
                return new ValidationFailure(
                    $"CHI sex indicator ({sexDigit})  did not match associated sex field ({sex})", parent);
        }

        //invalid chi, who cares
        return null;
    }
}