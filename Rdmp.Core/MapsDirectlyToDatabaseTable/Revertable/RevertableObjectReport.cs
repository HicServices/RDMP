// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

/// <summary>
///     Summarises the differences (if any) between the Properties of an IRevertable object vs the corresponding currently
///     saved database record.  Changes
///     can be the result of local in memory changes the user has made but not saved yet or changes other users have made
///     and saved since the IRevertable
///     was fetched.
/// </summary>
public class RevertableObjectReport
{
    public ChangeDescription Evaluation { get; set; }
    public List<RevertablePropertyDifference> Differences { get; private set; }

    public RevertableObjectReport()
    {
        Differences = new List<RevertablePropertyDifference>();
    }

    public RevertableObjectReport(List<RevertablePropertyDifference> differences)
    {
        Differences = differences;
    }
}