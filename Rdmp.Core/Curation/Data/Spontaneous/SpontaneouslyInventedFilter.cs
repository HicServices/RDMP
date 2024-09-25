// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Spontaneous;

/// <summary>
/// Spontaneous (memory only) implementation of IFilter.  This is the preferred method of injecting lines of WHERE Sql into an ISqlQueryBuilder dynamically in code
/// (as opposed to ones the user has created).  This can be used to for example enforce additional constraints on the query e.g. 'generate this Aggregate Graph but
/// restrict the results to patients appearing in my cohort list X' (in this case the SpontaneouslyInventedFilter would be the 'patients appearing in my cohort list X'
/// 
/// <para>The other way to inject sql code into an ISqlQueryBuilder is via CustomLine but that's less precise.</para>
/// </summary>
public class SpontaneouslyInventedFilter : ConcreteFilter
{
    private readonly MemoryCatalogueRepository _repo;
    private readonly ISqlParameter[] _filterParametersIfAny;
    private int _order =0;

    /// <summary>
    /// Creates a new temporary (unsaveable) filter in the given memory <paramref name="repo"/>
    /// </summary>
    /// <param name="repo">The repository to store the temporary object in</param>
    /// <param name="notionalParent"></param>
    /// <param name="whereSql"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="filterParametersIfAny"></param>
    public SpontaneouslyInventedFilter(MemoryCatalogueRepository repo, IContainer notionalParent, string whereSql,
        string name, string description, ISqlParameter[] filterParametersIfAny)
    {
        _repo = repo;
        _filterParametersIfAny = filterParametersIfAny;
        WhereSQL = whereSql;
        Name = name;
        Description = description;

        repo.InsertAndHydrate(this, new Dictionary<string, object>());

        if (notionalParent != null)
        {
            repo.AddChild(notionalParent, this);
            FilterContainer_ID = notionalParent.ID;
        }
    }

    /// <summary>
    /// Constructs a new filter by copying out the values from the supplied IFilter
    /// </summary>
    /// <param name="repo">The repository to store the temporary object in</param>
    /// <param name="copyFrom"></param>
    public SpontaneouslyInventedFilter(MemoryCatalogueRepository repo, IFilter copyFrom) : this(repo, null,
        copyFrom.WhereSQL, copyFrom.Name, copyFrom.Description, copyFrom.GetAllParameters())
    {
    }

    public override int? ClonedFromExtractionFilter_ID { get; set; }
    public override int? FilterContainer_ID { get; set; }
    public override ISqlParameter[] GetAllParameters() => _filterParametersIfAny ?? Array.Empty<ISqlParameter>();

    public override IContainer FilterContainer => FilterContainer_ID.HasValue
        ? _repo.GetObjectByID<IContainer>(FilterContainer_ID.Value)
        : null;

    public override int Order { get => _order; set => SetField(ref _order, value); }

    public override ColumnInfo GetColumnInfoIfExists() => null;

    public override IFilterFactory GetFilterFactory() => null;

    public override Catalogue GetCatalogue() => null;
}