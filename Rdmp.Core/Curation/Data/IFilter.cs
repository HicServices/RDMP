// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// A line of WHERE sql which can be combined in IContainers.  IFilters can be either ConcreteFilter (there is persisted user defined database object that makes
/// up the IFilter) or SpontaneouslyInventedFilter.
/// </summary>
public interface IFilter : ICollectSqlParameters, INamed, IHasQuerySyntaxHelper, ICheckable, IMightBeReadOnly
{
    /// <summary>
    /// Single line of WHERE Sql for use in query generation.  Does not include the WHERE keyword.
    /// </summary>
    [Sql]
    string WhereSQL { get; set; }

    /// <summary>
    /// Human readable description of what the WHERE logic is supposed to achieve (e.g. Tayside records only)
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// True if the <see cref="IFilter"/> should always be used with the <see cref="Catalogue"/> it is associated with if any (See <see cref="GetCatalogue"/>).
    /// 
    /// <para>This results in the filter being added to <see cref="CohortIdentificationConfiguration"/> , extarctions etc by default when the <see cref="Catalogue"/> is added</para>
    /// </summary>
    bool IsMandatory { get; set; }

    /// <summary>
    /// Only applicable for non Catalogue level filters (<see cref="ExtractionFilter"/>) i.e. derived/deployed filters.  This is the ID of the original master <see cref="ExtractionFilter"/>
    /// that the current filter was cloned as a copy of.
    /// 
    /// <para>Null if a master filter (<see cref="ExtractionFilter"/>) or a deployed filter that was written from scratch </para>
    /// </summary>
    int? ClonedFromExtractionFilter_ID { get; set; }

    /// <summary>
    /// An IFilter is a line of WHERE SQL.  To be used by a query builder it must be in an AND/OR <see cref="IContainer"/>.  The container will determine which operator is used
    /// to separate the lines of SQL when combined.  Obviously if there is only one IFilter in an <see cref="IContainer"/> then no separation operator will be in the resulting
    /// query.  This property is the ID of the current container.
    /// </summary>
    int? FilterContainer_ID { get; set; }

    /// <inheritdoc cref="FilterContainer_ID"/>
    [NoMappingToDatabase]
    IContainer FilterContainer { get; }

    /// <summary>
    /// Fetches the underlying <see cref="ColumnInfo"/> for the column (e.g. <see cref="CatalogueItem"/>) which this <see cref="IFilter"/> is ultimately associated with).
    /// 
    /// <para>This mostly applies to master top level filters <see cref="ExtractionFilter"/> and cloned copies and helps identify which tables to join to during query building</para>
    /// </summary>
    /// <returns></returns>
    ColumnInfo GetColumnInfoIfExists();

    /// <summary>
    /// Gets an appropriate <see cref="IFilterFactory"/> for creating arguments and other filters of the Type compatible the derived class (e.g. if the <see cref="IFilter"/> is an
    /// <see cref="ExtractionFilter"/> then an <see cref="ExtractionFilterFactory"/>  would be returned).
    /// </summary>
    /// <returns></returns>
    IFilterFactory GetFilterFactory();

    /// <summary>
    /// Gets the <see cref="Catalogue"/> that this <see cref="IFilter"/> is designed to be run on.  This should return a value regardless of the Type of <see cref="IFilter"/> e.g.
    /// master level or deployed as part of project extractions / cohort identification configurations.
    /// </summary>
    /// <returns></returns>
    Catalogue GetCatalogue();
}