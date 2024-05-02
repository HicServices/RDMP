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
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.Aggregation;

/// <summary>
///     Sometimes you want to restrict the data that is Aggregated as part of an AggregateConfiguration.  E.g. you might
///     want to only aggregate records loaded
///     in the last 6 months.  To do this you would need to set a root AggregateFilterContainer on the
///     AggregateConfiguration and then put in an appropriate
///     AggregateFilter.  Each AggregateFilter can be associated with a given ColumnInfo this will ensure that it is
///     included when it comes to JoinInfo time
///     in QueryBuilding even if it is not a selected dimension (this allows you to for example aggregate the drug codes
///     but filter by drug prescribed date even
///     when the two fields are in different tables - that will be joined at Query Time).
///     <para>
///         Each AggregateFilter can have a collection of AggregateFilterParameters which store SQL parameter values (along
///         with descriptions for the user) that let you
///         paramaterise (for the user) your AggregateFilter
///     </para>
/// </summary>
public class AggregateFilter : ConcreteFilter, IDisableable
{
    #region Database Properties

    private int? _filterContainerID;
    private int? _clonedFromExtractionFilterID;
    private int? _associatedColumnInfoID;
    private bool _isDisabled;

    /// <inheritdoc />
    public override int? ClonedFromExtractionFilter_ID
    {
        get => _clonedFromExtractionFilterID;
        set => SetField(ref _clonedFromExtractionFilterID, value);
    }

    /// <inheritdoc />
    [Relationship(typeof(AggregateFilterContainer), RelationshipType.SharedObject)]
    public override int? FilterContainer_ID
    {
        get => _filterContainerID;
        set => SetField(ref _filterContainerID, value);
    }


    /// <summary>
    ///     The column associated with the filter (most likely null).  This exists for future proofing and
    ///     for compatibility with interface <see cref="IFilter.GetColumnInfoIfExists" />
    /// </summary>
    public int? AssociatedColumnInfo_ID
    {
        get => _associatedColumnInfoID;
        set => SetField(ref _associatedColumnInfoID, value);
    }

    /// <inheritdoc />
    public bool IsDisabled
    {
        get => _isDisabled;
        set => SetField(ref _isDisabled, value);
    }

    #endregion

    /// <inheritdoc cref="GetAllParameters" />

    #region Relationships

    [NoMappingToDatabase]
    public IEnumerable<AggregateFilterParameter> AggregateFilterParameters
    {
        get { return Repository.GetAllObjectsWithParent<AggregateFilterParameter>(this); }
    }

    /// <inheritdoc />
    public override ISqlParameter[] GetAllParameters()
    {
        return AggregateFilterParameters.ToArray();
    }

    /// <inheritdoc />
    [NoMappingToDatabase]
    public override IContainer FilterContainer => FilterContainer_ID.HasValue
        ? Repository.GetObjectByID<AggregateFilterContainer>(FilterContainer_ID.Value)
        : null;

    #endregion

    public AggregateFilter()
    {
    }

    /// <summary>
    ///     Defines a new filter (line of WHERE SQL) in the specified AggregateFilterContainer (AND / OR).  Calling this
    ///     constructor creates a new object in the database
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    /// <param name="container"></param>
    public AggregateFilter(ICatalogueRepository repository, string name = null,
        AggregateFilterContainer container = null)
    {
        name ??= $"New AggregateFilter{Guid.NewGuid()}";

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "FilterContainer_ID", container != null ? container.ID : DBNull.Value }
        });
    }

    internal AggregateFilter(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        WhereSQL = r["WhereSQL"] as string;
        Description = r["Description"] as string;
        Name = r["Name"] as string;
        IsMandatory = (bool)r["IsMandatory"];
        ClonedFromExtractionFilter_ID = ObjectToNullableInt(r["ClonedFromExtractionFilter_ID"]);

        var associatedColumnInfo_ID = r["AssociatedColumnInfo_ID"];
        if (associatedColumnInfo_ID != DBNull.Value)
            AssociatedColumnInfo_ID = int.Parse(associatedColumnInfo_ID.ToString());

        if (r["FilterContainer_ID"] != null && !string.IsNullOrWhiteSpace(r["FilterContainer_ID"].ToString()))
            FilterContainer_ID = int.Parse(r["FilterContainer_ID"].ToString());
        else
            FilterContainer_ID = null;

        IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <inheritdoc />
    public override ColumnInfo GetColumnInfoIfExists()
    {
        if (AssociatedColumnInfo_ID != null)
            try
            {
                return Repository.GetObjectByID<ColumnInfo>((int)AssociatedColumnInfo_ID);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }

        return null;
    }

    /// <inheritdoc />
    public override IFilterFactory GetFilterFactory()
    {
        return new AggregateFilterFactory((ICatalogueRepository)Repository);
    }

    /// <inheritdoc />
    public override Catalogue GetCatalogue()
    {
        var agg = GetAggregate() ?? throw new Exception(
            $"Cannot determine the Catalogue for AggregateFilter {this} because GetAggregate returned null, possibly the Filter does not belong to any AggregateFilterContainer (i.e. it is an orphan?)");
        return agg.Catalogue;
    }

    /// <inheritdoc cref="ClonedFilterChecker" />
    public override void Check(ICheckNotifier notifier)
    {
        base.Check(notifier);

        var checker = new ClonedFilterChecker(this, ClonedFromExtractionFilter_ID, Repository);
        checker.Check(notifier);
    }

    /// <summary>
    ///     Removes the AggregateFilter from any AggregateFilterContainer (AND/OR) that it might be a part of
    ///     effectively turning it into a disconnected orphan.
    /// </summary>
    public void MakeIntoAnOrphan()
    {
        FilterContainer_ID = null;
        SaveToDatabase();
    }

    /// <summary>
    ///     Gets the parent <see cref="AggregateConfiguration" /> that this AggregateFilter is part of by ascending its
    ///     AggregateFilterContainer hierarchy.
    ///     If the AggregateFilter is an orphan or one of the parental containers is an orphan then null will be returned.
    /// </summary>
    /// <returns></returns>
    public AggregateConfiguration GetAggregate()
    {
        if (FilterContainer_ID == null)
            return null;

        var container = Repository.GetObjectByID<AggregateFilterContainer>(FilterContainer_ID.Value);
        return container.GetAggregate();
    }

    public AggregateFilter ShallowClone(AggregateFilterContainer into)
    {
        var clone = new AggregateFilter(CatalogueRepository, Name, into);
        CopyShallowValuesTo(clone);
        return clone;
    }
}