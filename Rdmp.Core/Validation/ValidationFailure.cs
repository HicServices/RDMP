// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Validation.Constraints;

namespace Rdmp.Core.Validation;

/// <summary>
///     A custom Validation exception, thrown when user-specified validation has failed in some way.
/// </summary>
public class ValidationFailure
{
    public ItemValidator SourceItemValidator { get; set; }
    public IConstraint SourceConstraint { get; set; }

    public string Message { get; set; }

    private readonly List<ValidationFailure> eList;

    private ValidationFailure(string message)
    {
        Message = message;
    }

    public ValidationFailure(string message, IConstraint sender) : this(message)
    {
        SourceConstraint = sender;
    }

    public ValidationFailure(string message, ItemValidator sender) : this(message)
    {
        SourceItemValidator = sender;
    }

    public ValidationFailure(string message, List<ValidationFailure> e) : this(message)
    {
        eList = e;
    }

    public List<ValidationFailure> GetExceptionList()
    {
        return eList;
    }
}