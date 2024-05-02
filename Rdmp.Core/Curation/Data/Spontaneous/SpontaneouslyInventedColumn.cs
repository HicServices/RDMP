// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Curation.Data.Spontaneous;

/// <summary>
///     Spontaneous (memory only) implementation of IColumn.
/// </summary>
public class SpontaneouslyInventedColumn : ConcreteColumn, IColumn
{
    public SpontaneouslyInventedColumn(MemoryRepository repo, string alias, string selectSQl)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new Exception("Column must have an alias");
        Alias = alias;
        SelectSQL = selectSQl;

        repo.InsertAndHydrate(this, new Dictionary<string, object>());
    }

    public override ColumnInfo ColumnInfo => null;
}