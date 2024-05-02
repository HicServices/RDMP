// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Providers;

public abstract class ProblemProvider : IProblemProvider
{
    public static HashSet<Type> IgnoreBadNamesFor = new(new[]
    {
        typeof(TableInfo),
        typeof(ColumnInfo),
        typeof(IFilter),
        typeof(Pipeline)
    });

    /// <inheritdoc />
    public bool HasProblem(object o)
    {
        return DescribeProblem(o) != null;
    }

    public string DescribeProblem(object o)
    {
        return o is INamed n && !IgnoreBadNamesFor.Any(t => t.IsInstanceOfType(o)) && UsefulStuff.IsBadName(n.Name)
            ? "Name contains illegal characters"
            : DescribeProblemImpl(o);
    }

    protected abstract string DescribeProblemImpl(object o);

    public abstract void RefreshProblems(ICoreChildProvider childProvider);
}