// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Validation.Constraints;

namespace Rdmp.Core.Validation.UIAttributes;

/// <summary>
///     Attribute for <see cref="IConstraint" /> properties which should not be visible in user interfaces
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class HideOnValidationUI : Attribute
{
}

/// <summary>
///     Attribute for <see cref="IConstraint" /> properties which should store a column name
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExpectsColumnNameAsInput : Attribute
{
}

/// <summary>
///     Attribute for <see cref="IConstraint" /> properties which should a large body of user entered text
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExpectsLotsOfText : Attribute
{
}