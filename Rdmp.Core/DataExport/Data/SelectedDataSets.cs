// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="ISelectedDataSets" />
public class SelectedDataSets : DatabaseEntity, ISelectedDataSets, IInjectKnown<IExtractableDataSet>,
    IInjectKnown<IExtractionConfiguration>, IInjectKnown<ISelectedDataSetsForcedJoin[]>, IDeletableWithCustomMessage
{
    #region Database Properties

    private int _extractionConfiguration_ID;
    private int _extractableDataSet_ID;
    private int? _rootFilterContainer_ID;

    private Lazy<IExtractableDataSet> _extractableDataSet;
    private Lazy<IExtractionConfiguration> _extractionConfiguration;
    private Lazy<ISelectedDataSetsForcedJoin[]> _selectedDatasetsForcedJoins;

    /// <inheritdoc />
    public int ExtractionConfiguration_ID
    {
        get => _extractionConfiguration_ID;
        set => SetField(ref _extractionConfiguration_ID, value);
    }

    /// <inheritdoc />
    public int ExtractableDataSet_ID
    {
        get => _extractableDataSet_ID;
        set
        {
            ClearAllInjections();
            SetField(ref _extractableDataSet_ID, value);
        }
    }

    /// <inheritdoc />
    public int? RootFilterContainer_ID
    {
        get => _rootFilterContainer_ID;
        set => SetField(ref _rootFilterContainer_ID, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="RootFilterContainer_ID" />
    [NoMappingToDatabase]
    public IContainer RootFilterContainer =>
        RootFilterContainer_ID == null
            ? null
            : Repository.GetObjectByID<FilterContainer>(RootFilterContainer_ID.Value);

    /// <inheritdoc cref="ExtractionConfiguration_ID" />
    [NoMappingToDatabase]
    public IExtractionConfiguration ExtractionConfiguration => _extractionConfiguration.Value;

    /// <inheritdoc cref="ExtractableDataSet_ID" />
    [NoMappingToDatabase]
    public IExtractableDataSet ExtractableDataSet => _extractableDataSet.Value;

    /// <inheritdoc />
    [NoMappingToDatabase]
    public ISelectedDataSetsForcedJoin[] SelectedDataSetsForcedJoins => _selectedDatasetsForcedJoins.Value;


    /// <inheritdoc />
    [NoMappingToDatabase]
    public IExtractionProgress ExtractionProgressIfAny =>
        DataExportRepository.GetAllObjectsWithParent<ExtractionProgress>(this).SingleOrDefault();

    #endregion

    public SelectedDataSets()
    {
        ClearAllInjections();
    }

    internal SelectedDataSets(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        ExtractionConfiguration_ID = Convert.ToInt32(r["ExtractionConfiguration_ID"]);
        ExtractableDataSet_ID = Convert.ToInt32(r["ExtractableDataSet_ID"]);
        RootFilterContainer_ID = ObjectToNullableInt(r["RootFilterContainer_ID"]);
    }

    /// <summary>
    ///     Declares in the <paramref name="repository" /> database that the given <paramref name="dataSet" /> should be
    ///     extracted as part of the given <paramref name="configuration" />.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="configuration"></param>
    /// <param name="dataSet"></param>
    /// <param name="rootContainerIfAny">
    ///     Adds the restriction that the extraction SQL should include the WHERE logic in this
    ///     container
    /// </param>
    public SelectedDataSets(IDataExportRepository repository, ExtractionConfiguration configuration,
        IExtractableDataSet dataSet, FilterContainer rootContainerIfAny)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ExtractionConfiguration_ID", configuration.ID },
            { "ExtractableDataSet_ID", dataSet.ID },
            { "RootFilterContainer_ID", rootContainerIfAny != null ? rootContainerIfAny.ID : DBNull.Value }
        });

        ClearAllInjections();
        InjectKnown(dataSet);
    }

    /// <summary>
    ///     Returns the <see cref="ExtractableDataSet" /> name
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ExtractableDataSet.ToString();
    }

    public bool ShouldBeReadOnly(out string reason)
    {
        return ExtractionConfiguration.ShouldBeReadOnly(out reason);
    }

    /// <inheritdoc />
    public string GetDeleteVerb()
    {
        return "Remove";
    }

    /// <inheritdoc />
    public string GetDeleteMessage()
    {
        return $"remove '{ExtractableDataSet}' from ExtractionConfiguration '{ExtractionConfiguration}'";
    }

    /// <inheritdoc />
    public void InjectKnown(IExtractableDataSet instance)
    {
        if (instance.ID != ExtractableDataSet_ID)
            throw new ArgumentException($"That is not our dataset, our dataset has ID {ExtractableDataSet_ID}",
                nameof(instance));

        _extractableDataSet = new Lazy<IExtractableDataSet>(instance);
    }

    /// <inheritdoc />
    public void InjectKnown(IExtractionConfiguration instance)
    {
        _extractionConfiguration = new Lazy<IExtractionConfiguration>(FetchExtractionConfiguration);
    }

    private IExtractionConfiguration FetchExtractionConfiguration()
    {
        return Repository.GetObjectByID<ExtractionConfiguration>(ExtractionConfiguration_ID);
    }

    /// <inheritdoc />
    public void InjectKnown(ISelectedDataSetsForcedJoin[] instances)
    {
        _selectedDatasetsForcedJoins = new Lazy<ISelectedDataSetsForcedJoin[]>(instances);
    }

    /// <inheritdoc />
    public void ClearAllInjections()
    {
        _selectedDatasetsForcedJoins = new Lazy<ISelectedDataSetsForcedJoin[]>(FetchForcedJoins);
        _extractionConfiguration = new Lazy<IExtractionConfiguration>(FetchExtractionConfiguration);
        _extractableDataSet = new Lazy<IExtractableDataSet>(FetchExtractableDataset);
    }

    private ISelectedDataSetsForcedJoin[] FetchForcedJoins()
    {
        return Repository.GetAllObjectsWithParent<SelectedDataSetsForcedJoin>(this).ToArray();
    }

    public ICatalogue GetCatalogue()
    {
        return ExtractableDataSet.Catalogue;
    }

    private IExtractableDataSet FetchExtractableDataset()
    {
        return Repository.GetObjectByID<ExtractableDataSet>(ExtractableDataSet_ID);
    }

    /// <inheritdoc />
    public ICumulativeExtractionResults GetCumulativeExtractionResultsIfAny()
    {
        return ExtractionConfiguration.CumulativeExtractionResults.SingleOrDefault(ec => ec.IsFor(this));
    }

    public void CreateRootContainerIfNotExists()
    {
        if (RootFilterContainer_ID != null) return;
        var container = new FilterContainer(DataExportRepository);
        RootFilterContainer_ID = container.ID;
        SaveToDatabase();
    }

    public IFilterFactory GetFilterFactory()
    {
        return new DeployedExtractionFilterFactory(DataExportRepository);
    }

    public override void DeleteInDatabase()
    {
        var cols = ExtractionConfiguration.GetAllExtractableColumnsFor(ExtractableDataSet);

        ExtractionProgressIfAny?.DeleteInDatabase();
        base.DeleteInDatabase();

        foreach (var col in cols)
            if (col.Exists())
                col.DeleteInDatabase();
    }
}