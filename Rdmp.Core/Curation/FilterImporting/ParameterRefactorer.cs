// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;

namespace Rdmp.Core.Curation.FilterImporting;

/// <summary>
///     Handles renaming a parameter in the WHERE SQL of its parent (if it has one).  Use this when you want the user to be
///     able to change the name of a parameter and for this
///     to be carried through to the parent without having any knowledge available to what that parent is or even if it has
///     one
/// </summary>
public class ParameterRefactorer : IParameterRefactorer
{
    public HashSet<IFilter> RefactoredFilters { get; }

    public ParameterRefactorer()
    {
        RefactoredFilters = new HashSet<IFilter>();
    }

    public bool HandleRename(ISqlParameter parameter, string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return false;

        if (string.IsNullOrWhiteSpace(oldName))
            return false;

        //they are the same name!
        if (oldName.Equals(newName))
            return false;

        if (!parameter.ParameterName.Equals(newName))
            throw new ArgumentException(
                $"Expected parameter {parameter} to have name '{newName}' but its value was {parameter.ParameterName}, this means someone was lying about the rename event");

        var owner = parameter.GetOwnerIfAny();

        if (owner is not IFilter filter || filter is SpontaneousObject)
            return false;

        //There is no WHERE SQL anyway
        if (string.IsNullOrWhiteSpace(filter.WhereSQL))
            return false;

        var before = filter.WhereSQL;
        var after = ParameterCreator.RenameParameterInSQL(before, oldName, newName);

        //no change was actually made
        if (before.Equals(after))
            return false;

        filter.WhereSQL = after;
        filter.SaveToDatabase();

        RefactoredFilters.Add(filter);

        return true;
    }
}