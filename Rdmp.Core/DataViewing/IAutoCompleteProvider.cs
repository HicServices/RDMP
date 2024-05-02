// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.DataViewing;

/// <summary>
///     Interface for autocomplete controls which gather RDMP objects and supply user with text suggestions during typing
/// </summary>
public interface IAutoCompleteProvider
{
    /// <summary>
    ///     Add autocomplete strings/controls that represent <paramref name="tableInfo" /> to the user interface
    /// </summary>
    /// <param name="tableInfo"></param>
    void Add(ITableInfo tableInfo);

    /// <summary>
    ///     Add autocomplete strings/controls that represent <paramref name="aggregateConfiguration" /> to the user interface
    /// </summary>
    /// <param name="aggregateConfiguration"></param>
    void Add(AggregateConfiguration aggregateConfiguration);

    /// <summary>
    ///     Add autocomplete strings/controls that represent <paramref name="table" /> to the user interface
    /// </summary>
    /// <param name="table"></param>
    void Add(DiscoveredTable table);

    /// <summary>
    ///     Add autocomplete strings/controls that represent <paramref name="columnInfo" /> to the user interface
    /// </summary>
    /// <param name="columnInfo"></param>
    void Add(ColumnInfo columnInfo);
}