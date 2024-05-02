// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Checks;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Sometimes it is necessary to restrict which records are extracted for a given ExtractionConfiguration beyond the
///     linkage against a cohort.  For example you might want to extract
///     'only paracetamol prescriptions' for your cohort rather than the entire Prescribing dataset.  This is achieved by
///     using a DeployedExtractionFilter.  DeployedExtractionFilters are
///     curated pieces of WHERE SQL with a name and description.  These can either be written bespoke for your extract or
///     copied from a master ExtractionFilter in the Catalogue database.
///     In general if a filter concept is reusable and useful across multiple projects / over time then you should create
///     it in the Catalogue database as an ExtractionFilter and then
///     import a copy into your ExtractionConfiguration each time you need it (or mark it as IsMandatory if it should
///     always be used in data extraction of that Catalogue).
///     <para>
///         When you import a master filter into your ExtractionConfiguration a copy of the WHERE SQL, any
///         parameters and the name and description will be made as a DeployedExtractionFilter which will also contain a
///         reference back to the original (ClonedFromExtractionFilter_ID).  This
///         allows you to ensure consistency over time and gives you a central location (the ExtractionFilter) to fix
///         errors in the Filter implementation etc.
///     </para>
/// </summary>
public class DeployedExtractionFilter : ConcreteFilter
{
    #region Database Properties

    private int? _clonedFromExtractionFilterID;
    private int? _filterContainerID;

    /// <inheritdoc />
    public override int? ClonedFromExtractionFilter_ID
    {
        get => _clonedFromExtractionFilterID;
        set => SetField(ref _clonedFromExtractionFilterID, value);
    }

    /// <inheritdoc />
    [Relationship(typeof(FilterContainer), RelationshipType.SharedObject)]
    public override int? FilterContainer_ID
    {
        get => _filterContainerID;
        set => SetField(ref _filterContainerID, value);
    }

    #endregion

    #region Relationships

    /// <summary>
    ///     Returns all parameters declared against this filter (does not include other parameters in scope e.g. globals)
    /// </summary>
    [NoMappingToDatabase]
    public DeployedExtractionFilterParameter[] ExtractionFilterParameters => Repository
        .GetAllObjectsWhere<DeployedExtractionFilterParameter>("ExtractionFilter_ID", ID).ToArray();

    /// <inheritdoc />
    [NoMappingToDatabase]
    public override IContainer FilterContainer => FilterContainer_ID.HasValue
        ? Repository.GetObjectByID<FilterContainer>(FilterContainer_ID.Value)
        : null;

    #endregion

    /// <inheritdoc />
    public override ColumnInfo GetColumnInfoIfExists()
    {
        return null;
    }

    /// <inheritdoc />
    public override IFilterFactory GetFilterFactory()
    {
        return new DeployedExtractionFilterFactory((IDataExportRepository)Repository);
    }

    /// <inheritdoc />
    public override Catalogue GetCatalogue()
    {
        var ds = GetDataset().ExtractableDataSet;
        try
        {
            return (Catalogue)ds.Catalogue;
        }
        catch (Exception)
        {
            //could be that the catalogue has been deleted
            return null;
        }
    }

    /// <inheritdoc />
    public override ISqlParameter[] GetAllParameters()
    {
        return ExtractionFilterParameters.Cast<ISqlParameter>().ToArray();
    }

    public DeployedExtractionFilter()
    {
    }

    /// <summary>
    ///     Creates a new empty WHERE filter in the given <paramref name="container" /> that will be used when
    ///     extracting the dataset.
    ///     <para>This object is created into the data export metadata database</para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    /// <param name="container"></param>
    public DeployedExtractionFilter(IDataExportRepository repository, string name, FilterContainer container)
    {
        Repository = repository;
        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name != null ? name : DBNull.Value },
            { "FilterContainer_ID", container != null ? container.ID : DBNull.Value }
        });
    }

    /// <summary>
    ///     Read an existing WHERE filter out of the database
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal DeployedExtractionFilter(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        WhereSQL = r["WhereSQL"] as string;
        Description = r["Description"] as string;
        Name = r["Name"] as string;
        IsMandatory = (bool)r["IsMandatory"];

        if (r["FilterContainer_ID"] != null && !string.IsNullOrWhiteSpace(r["FilterContainer_ID"].ToString()))
            FilterContainer_ID = int.Parse(r["FilterContainer_ID"].ToString());
        else
            FilterContainer_ID = null;

        ClonedFromExtractionFilter_ID = ObjectToNullableInt(r["ClonedFromExtractionFilter_ID"]);
    }

    /// <summary>
    ///     Returns Name of filters
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Name;
    }


    /// <summary>
    ///     Checks the filter is properly defined (e.g. not blank).
    /// </summary>
    /// <param name="notifier"></param>
    public override void Check(ICheckNotifier notifier)
    {
        base.Check(notifier);

        var checker = new ClonedFilterChecker(this, ClonedFromExtractionFilter_ID,
            ((IDataExportRepository)Repository).CatalogueRepository);
        checker.Check(notifier);
    }

    /// <summary>
    ///     Returns the configuration and dataset (<see cref="ISelectedDataSets" />) in which the filter is declared.  This
    ///     involves traversing
    ///     up any nested <see cref="FilterContainer" />s to the root.
    ///     <para>Returns null if the filter is an orphan (not in a container or part of an orphan container tree)</para>
    /// </summary>
    /// <returns></returns>
    public SelectedDataSets GetDataset()
    {
        if (FilterContainer_ID == null)
            return null;

        var container = Repository.GetObjectByID<FilterContainer>(FilterContainer_ID.Value);
        return container.GetSelectedDataSetsRecursively();
    }

    public DeployedExtractionFilter ShallowClone(FilterContainer into)
    {
        var clone = new DeployedExtractionFilter(DataExportRepository, Name, into);
        CopyShallowValuesTo(clone);
        return clone;
    }
}