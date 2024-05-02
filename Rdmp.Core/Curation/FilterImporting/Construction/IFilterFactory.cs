// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Curation.FilterImporting.Construction;

/// <summary>
///     Facilitates the creation of IFilter (lines of WHERE Sql) and ISqlParameter (sql parameters - DECLARE @bob as
///     varchar(10)) instances.
/// </summary>
public interface IFilterFactory
{
    /// <summary>
    ///     Creates a new blank <see cref="IFilter" /> with the provided <paramref name="name" />.  Each implementation of this
    ///     method may return a
    ///     different Type of filter but should be consistent with a given implementation.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IFilter CreateNewFilter(string name);

    /// <summary>
    ///     Creates a new <see cref="ISqlParameter" /> with the provided <paramref name="parameterSQL" /> for use with the
    ///     provided <paramref name="filter" />.
    ///     Each implementation of this method may return a different Type of <see cref="ISqlParameter" /> but should be
    ///     consistent with a given implementation.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="parameterSQL"></param>
    /// <returns></returns>
    ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL);

    /// <summary>
    ///     The object Type which owns the root container e.g. if the IFilter is AggregateFilter then the IContainer Type is
    ///     AggregateFilterContainers and the
    ///     Root Owner Type is AggregateConfiguration
    /// </summary>
    /// <returns></returns>
    Type GetRootOwnerType();

    /// <summary>
    ///     If the IFilter Type is designed to be held in IContainers then this method should return the Type of IContainer
    ///     e.g. AggregateFilters belong in
    ///     AggregateFilterContainers
    /// </summary>
    /// <returns></returns>
    Type GetIContainerTypeIfAny();

    /// <summary>
    ///     Creates a new filter container of the appropriate Type
    /// </summary>
    /// <returns></returns>
    IContainer CreateNewContainer();
}