// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation;

/// <summary>
///     Interface for objects with <see cref="IContainer" /> (WHERE) logic defined for them
/// </summary>
public interface IRootFilterContainerHost : ISaveable, IMapsDirectlyToDatabaseTable, IMightBeReadOnly
{
    /// <summary>
    ///     The root AND/OR container which provides WHERE logic that should be included in the query when extracting the
    ///     dataset
    /// </summary>
    int? RootFilterContainer_ID { get; set; }

    /// <summary>
    ///     Returns the main Catalogue being queried by this object's filter containers
    /// </summary>
    /// <returns></returns>
    ICatalogue GetCatalogue();

    /// <summary>
    ///     Returns the root container specified by the <see cref="RootFilterContainer_ID" />
    /// </summary>
    /// <returns></returns>
    IContainer RootFilterContainer { get; }

    /// <summary>
    ///     Creates an appropriate filter to be the root of the hierarchy (See <see cref="RootFilterContainer" />)
    /// </summary>
    void CreateRootContainerIfNotExists();

    /// <summary>
    ///     Returns a filter factory of the appropriate Type for creating filters in the <see cref="RootFilterContainer" />
    /// </summary>
    /// <returns></returns>
    IFilterFactory GetFilterFactory();
}