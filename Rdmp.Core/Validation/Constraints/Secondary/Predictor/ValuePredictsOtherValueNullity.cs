// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Xml.Serialization;

namespace Rdmp.Core.Validation.Constraints.Secondary.Predictor;

/// <summary>
///     Validation rule for use with a Prediction Constraint.  Indicates that the 'nullity' of the columns must match (i.e.
///     if one is null the other must be too)
/// </summary>
// ReSharper disable once StringLiteralTypo
[XmlType(TypeName = "ValuePredictsOtherValueNullness")]
public class ValuePredictsOtherValueNullity : PredictionRule
{
    public override ValidationFailure Predict(IConstraint parent, object value, object targetValue)
    {
        return value == null != (targetValue == null)
            ? new ValidationFailure(
                $"Nullity did not match, when one value is null, the other must be null.  When one value has a value the other must also have a value.  Nullity of ConstrainedColumn:{value == null}. Nullity of TargetColumn:{targetValue == null}",
                parent)
            : null;
    }
}