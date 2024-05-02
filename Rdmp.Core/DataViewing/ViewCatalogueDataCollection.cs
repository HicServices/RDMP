// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing;

/// <summary>
///     Collection for generating SQL around the extractable columns of a catalogue
/// </summary>
public class ViewCatalogueDataCollection : PersistableObjectCollection, IViewSQLAndResultsCollection
{
    private QueryBuilder builder;

    public Catalogue Catalogue => DatabaseObjects.OfType<Catalogue>().FirstOrDefault();

    public IFilter[] Filters => DatabaseObjects.OfType<IFilter>().ToArray();

    public ExtractionInformation[] ExtractionInformations => DatabaseObjects.OfType<ExtractionInformation>().ToArray();

    /// <summary>
    ///     The number of records to fetch (or null to fetch all records)
    /// </summary>
    public int? TopX { get; set; }

    public ViewCatalogueDataCollection(Catalogue catalogue)
    {
        DatabaseObjects.Add(catalogue);
    }

    /// <summary>
    ///     Persistence constructor
    /// </summary>
    public ViewCatalogueDataCollection()
    {
    }

    private void BuildBuilder()
    {
        if (builder != null)
            return;

        builder = new QueryBuilder(null, null);

        if (TopX.HasValue)
            builder.TopX = TopX.Value;

        var cols = ExtractionInformations;

        // if there are no explicit columns use all
        if (!cols.Any())
            cols =
                Catalogue.GetAllExtractionInformation(ExtractionCategory.Core)
                    .Union(Catalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific))
                    .ToArray();

        builder.AddColumnRange(cols);

        builder.RootFilterContainer = new SpontaneouslyInventedFilterContainer(new MemoryCatalogueRepository(), null,
            Filters, FilterContainerOperation.AND);
        builder.RegenerateSQL();
    }

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
        BuildBuilder();

        foreach (var t in builder.TablesUsedInQuery)
            autoComplete.Add(t);
    }

    public IDataAccessPoint GetDataAccessPoint()
    {
        BuildBuilder();
        return builder.TablesUsedInQuery.FirstOrDefault();
    }

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        BuildBuilder();
        return builder.QuerySyntaxHelper;
    }

    public string GetSql()
    {
        BuildBuilder();
        return builder.SQL;
    }

    public string GetTabName()
    {
        return Catalogue.Name;
    }

    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        yield return Catalogue;
    }
}