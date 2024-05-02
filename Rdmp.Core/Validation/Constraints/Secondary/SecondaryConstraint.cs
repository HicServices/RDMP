// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using Rdmp.Core.Validation.UIAttributes;

namespace Rdmp.Core.Validation.Constraints.Secondary;

public abstract class SecondaryConstraint : ISecondaryConstraint
{
    /// <inheritdoc />
    public Consequence? Consequence { get; set; }

    [Description("Optional, Allows you to record why you have set this rule as a future reminder")]
    [ExpectsLotsOfText]
    public string Rationale { get; set; }

    public abstract void RenameColumn(string originalName, string newName);
    public abstract string GetHumanReadableDescriptionOfValidation();
    public abstract ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames);
}