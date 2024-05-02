// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Validation.Constraints.Secondary.Predictor;

/// <summary>
///     Models a validation rule in which the value in one column predicts the value in another based on arbitrary code
/// </summary>
public abstract class PredictionRule
{
    /// <summary>
    ///     Validates the <paramref name="targetValue" /> against the <paramref name="value" /> and returns a
    ///     <see cref="ValidationFailure" /> if
    ///     the values do not match expectations
    /// </summary>
    /// <param name="parent">The constraint in which the rule is selected</param>
    /// <param name="value">The value upon which to make a prediction</param>
    /// <param name="targetValue">The value that should match the prediction</param>
    /// <returns>
    ///     null if the predicted value matches the <paramref name="targetValue" /> otherwise a
    ///     <see cref="ValidationFailure" />
    /// </returns>
    public abstract ValidationFailure Predict(IConstraint parent, object value, object targetValue);
}