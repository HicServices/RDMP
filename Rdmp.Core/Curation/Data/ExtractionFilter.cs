// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Defines as a single line SQL Where statement, a way of reducing the scope of a data extraction / aggregation etc.
///     For example,
///     'Only prescriptions for diabetes medications'.  An ExtractionFilter can have 0 or more ExtractionFilterParameters
///     which allows
///     you to define a more versatile filter e.g. 'Only prescriptions for drug @bnfCode'
///     <para>
///         Typically an ExtractionFilter is cloned out as either a DeployedExtractionFilter or an AggregateFilter and
///         either used as is or
///         customised in its new state (where its parameters might have values populated into them).
///     </para>
///     <para>
///         It is not uncommon for an extraction to involve multiple customised copies of the same Extraction filter for
///         example a user might
///         take the filter 'Prescriptions of drug @Drugname' and make 3 copies in a given project in DataExportManager
///         (this would result in
///         3 DeployedExtractionFilters) and set the value of the first to 'Paracetamol' the second to 'Aspirin' and the
///         third to 'Ibuprofen'
///         and then put them all in a single AND container.
///     </para>
///     <para>
///         At query building time QueryBuilder rationalizes all the various containers, subcontainers, filters and
///         parameters into one extraction
///         SQL query (including whatever columns/transforms it was setup with).
///     </para>
/// </summary>
public class ExtractionFilter : ConcreteFilter, IHasDependencies, IInjectKnown<ExtractionFilterParameterSet[]>
{
    #region Database Properties

    private int _extractionInformationID;
    private Lazy<ExtractionFilterParameterSet[]> _knownExtractionFilterParameterSets;

    /// <summary>
    ///     The column in the <see cref="Catalogue" /> which is best/most associated with this filter.  A filter can query any
    ///     column in any of the table(s) under
    ///     the <see cref="Catalogue" /> but must always be associated with only one specific extractable column (
    ///     <see cref="ExtractionInformation" />)
    /// </summary>
    [Relationship(typeof(ExtractionInformation), RelationshipType.LocalReference)]
    public int ExtractionInformation_ID
    {
        get => _extractionInformationID;
        set => SetField(ref _extractionInformationID, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc />
    [NoMappingToDatabase]
    public override IContainer FilterContainer => null;

    #endregion

    /// <inheritdoc />
    [NoMappingToDatabase]
    public override int? FilterContainer_ID
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    [NoMappingToDatabase]
    public ExtractionFilterParameterSet[] ExtractionFilterParameterSets => _knownExtractionFilterParameterSets.Value;

    /// <inheritdoc />
    public override ColumnInfo GetColumnInfoIfExists()
    {
        return ExtractionInformation.ColumnInfo;
    }

    /// <inheritdoc />
    public override IFilterFactory GetFilterFactory()
    {
        return new ExtractionFilterFactory(ExtractionInformation);
    }

    /// <inheritdoc />
    public override Catalogue GetCatalogue()
    {
        return ExtractionInformation.CatalogueItem.Catalogue;
    }

    /// <inheritdoc />
    public override ISqlParameter[] GetAllParameters()
    {
        return ExtractionFilterParameters.ToArray();
    }

    #region Relationships

    /// <inheritdoc cref="ExtractionInformation_ID" />
    [NoMappingToDatabase]
    public ExtractionInformation ExtractionInformation =>
        Repository.GetObjectByID<ExtractionInformation>(ExtractionInformation_ID);

    /// <inheritdoc cref="ConcreteFilter.GetAllParameters" />
    [NoMappingToDatabase]
    public IEnumerable<ExtractionFilterParameter> ExtractionFilterParameters =>
        Repository.GetAllObjectsWithParent<ExtractionFilterParameter>(this);

    #endregion

    public ExtractionFilter()
    {
        ClearAllInjections();
    }

    /// <summary>
    ///     Creates a new WHERE SQL block for reuse with the <see cref="Catalogue" /> in which the <paramref name="parent" />
    ///     resides.  This is a top level master filter and can be
    ///     copied out in <see cref="CohortIdentificationConfiguration" />, ExtractionConfiguration etc.  This ensures a single
    ///     curated block of
    ///     logic that everyone shares.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    public ExtractionFilter(ICatalogueRepository repository, string name, ExtractionInformation parent)
    {
        name ??= $"New Filter {Guid.NewGuid()}";

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "ExtractionInformation_ID", parent.ID }
        });

        ClearAllInjections();
    }

    internal ExtractionFilter(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        ExtractionInformation_ID = int.Parse(r["ExtractionInformation_ID"].ToString());
        WhereSQL = r["WhereSQL"] as string;
        Description = r["Description"] as string;
        Name = r["Name"] as string;
        IsMandatory = (bool)r["IsMandatory"];

        ClearAllInjections();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    //we are an extraction filter ourselves! so obviously we werent cloned from one! (this is for aggregate and data export filters and satisfies IFilter).  Actually we can
    //be cloned via the publishing (elevation) from a custom filter defined at Aggregate level for example.  But in this case we don't need to know the ID anyway since we
    //become the new master anyway since we are at the highest level for filters

    /// <summary>
    ///     Returns null, <see cref="ExtractionFilter" /> are master level filters and therefore never cloned from another
    ///     filter
    /// </summary>
    [NoMappingToDatabase]
    public override int? ClonedFromExtractionFilter_ID
    {
        get => null;
        set => throw new NotSupportedException(
            "ClonedFromExtractionFilter_ID is only supported on lower level filters e.g. DeployedExtractionFilter and AggregateFilter");
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return new IHasDependencies[] { ExtractionInformation };
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        return ExtractionFilterParameters.ToArray();
    }

    public void InjectKnown(ExtractionFilterParameterSet[] instance)
    {
        _knownExtractionFilterParameterSets = new Lazy<ExtractionFilterParameterSet[]>(instance);
    }

    public void ClearAllInjections()
    {
        _knownExtractionFilterParameterSets = new Lazy<ExtractionFilterParameterSet[]>(() =>
            CatalogueRepository.GetAllObjectsWithParent<ExtractionFilterParameterSet>(this));
    }

    public override void DeleteInDatabase()
    {
        if (ExtractionFilterParameterSets.Any())
            throw new Exception(
                $"Cannot delete '{this}' because there are one or more {nameof(ExtractionFilterParameterSet)} declared on it");

        base.DeleteInDatabase();
    }
}