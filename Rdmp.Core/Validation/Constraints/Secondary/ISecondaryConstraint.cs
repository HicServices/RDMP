// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Validation.UIAttributes;

namespace Rdmp.Core.Validation.Constraints.Secondary;

/// <summary>
///     Each column can have as many SecondaryConstraints as you want.  Each SecondaryConstraint is a general rule about
///     the data that the column is allowed to
///     contain.  This can include Regexes, NotNull requirements etc.
/// </summary>
public interface ISecondaryConstraint : IConstraint
{
    /// <summary>
    ///     Inherit this method to perform validation operations unique to your Class.  Column value could be DateTime, string
    ///     or numerical.  Part of validation is
    ///     ensuring it is of the appropriate type.  otherColumns can be used for example in the case that you intend to
    ///     predict something such as Gender from Title.
    ///     If your validation fails you should return a ValidationFailure.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="otherColumns"></param>
    /// <param name="otherColumnNames"></param>
    ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames);

    /// <summary>
    ///     User supplied rational for the validation rule being applied (helps understanding complicated rules e.g. regex
    ///     based rules).
    /// </summary>
    [ExpectsLotsOfText]
    string Rationale { get; set; }
}