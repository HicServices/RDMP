// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="ICumulativeExtractionResults"/>
public class CumulativeExtractionResults : DatabaseEntity, ICumulativeExtractionResults,
    IInjectKnown<IExtractableDataSet>
{
    #region Database Properties

    private int _extractionConfiguration_ID;
    private int _extractableDataSet_ID;
    private DateTime _dateOfExtraction;
    private string _destinationType;
    private string _destinationDescription;
    private int _recordsExtracted;
    private int _distinctReleaseIdentifiersEncountered;
    private string _filtersUsed;
    private string _exception;
    private string _sQLExecuted;
    private int _cohortExtracted;
    private Lazy<IExtractableDataSet> _knownExtractableDataSet;

    /// <inheritdoc/>
    public int ExtractionConfiguration_ID
    {
        get => _extractionConfiguration_ID;
        set => SetField(ref _extractionConfiguration_ID, value);
    }

    /// <inheritdoc/>
    public int ExtractableDataSet_ID
    {
        get => _extractableDataSet_ID;
        set => SetField(ref _extractableDataSet_ID, value);
    }

    /// <inheritdoc/>
    public DateTime DateOfExtraction
    {
        get => _dateOfExtraction;
        private set => SetField(ref _dateOfExtraction, value);
    }

    /// <inheritdoc/>
    public string DestinationType
    {
        get => _destinationType;
        private set => SetField(ref _destinationType, value);
    }

    /// <inheritdoc/>
    public string DestinationDescription
    {
        get => _destinationDescription;
        private set => SetField(ref _destinationDescription, value);
    }

    /// <inheritdoc/>
    public int RecordsExtracted
    {
        get => _recordsExtracted;
        set => SetField(ref _recordsExtracted, value);
    }

    /// <inheritdoc/>
    public int DistinctReleaseIdentifiersEncountered
    {
        get => _distinctReleaseIdentifiersEncountered;
        set => SetField(ref _distinctReleaseIdentifiersEncountered, value);
    }

    /// <inheritdoc/>
    public string FiltersUsed
    {
        get => _filtersUsed;
        set => SetField(ref _filtersUsed, value);
    }

    /// <inheritdoc/>
    public string Exception
    {
        get => _exception;
        set => SetField(ref _exception, value);
    }

    /// <inheritdoc/>
    public string SQLExecuted
    {
        get => _sQLExecuted;
        private set => SetField(ref _sQLExecuted, value);
    }

    /// <inheritdoc/>
    public int CohortExtracted
    {
        get => _cohortExtracted;
        private set => SetField(ref _cohortExtracted, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IExtractableDataSet ExtractableDataSet => _knownExtractableDataSet.Value;

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public List<ISupplementalExtractionResults> SupplementalExtractionResults =>
        new(
            Repository.GetAllObjectsWithParent<SupplementalExtractionResults>(this));

    /// <inheritdoc/>
    public ISupplementalExtractionResults AddSupplementalExtractionResult(string sqlExecuted,
        IMapsDirectlyToDatabaseTable extractedObject)
    {
        var result = new SupplementalExtractionResults(DataExportRepository, this, sqlExecuted, extractedObject);
        SupplementalExtractionResults.Add(result);
        return result;
    }

    /// <inheritdoc/>
    public bool IsFor(ISelectedDataSets selectedDataSet) =>
        selectedDataSet.ExtractableDataSet_ID == ExtractableDataSet_ID &&
        selectedDataSet.ExtractionConfiguration_ID == ExtractionConfiguration_ID;

    #endregion


    public CumulativeExtractionResults()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Creates a new audit record in the data export database for describing an extraction attempt of the given <paramref name="dataset"/> in the
    /// extraction <paramref name="configuration"/>.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="configuration"></param>
    /// <param name="dataset"></param>
    /// <param name="sql"></param>
    public CumulativeExtractionResults(IDataExportRepository repository, IExtractionConfiguration configuration,
        IExtractableDataSet dataset, string sql)
    {
        Repository = repository;
        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ExtractionConfiguration_ID", configuration.ID },
            { "ExtractableDataSet_ID", dataset.ID },
            { "SQLExecuted", sql },
            { "CohortExtracted", configuration.Cohort_ID }
        });

        ClearAllInjections();
    }

    /// <summary>
    /// Reads an existing audit record out of the data export database
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal CumulativeExtractionResults(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        ExtractionConfiguration_ID = int.Parse(r["ExtractionConfiguration_ID"].ToString());
        ExtractableDataSet_ID = int.Parse(r["ExtractableDataSet_ID"].ToString());
        DateOfExtraction = (DateTime)r["DateOfExtraction"];
        RecordsExtracted = int.Parse(r["RecordsExtracted"].ToString());
        DistinctReleaseIdentifiersEncountered = int.Parse(r["DistinctReleaseIdentifiersEncountered"].ToString());
        Exception = r["Exception"] as string;
        FiltersUsed = r["FiltersUsed"] as string;
        DestinationType = r["DestinationType"] as string;
        DestinationDescription = r["DestinationDescription"] as string;
        SQLExecuted = r["SQLExecuted"] as string;
        CohortExtracted = int.Parse(r["CohortExtracted"].ToString());

        ClearAllInjections();
    }

    /// <inheritdoc/>
    public IReleaseLog GetReleaseLogEntryIfAny()
    {
        var repo = (IDataExportRepository)Repository;

        return repo.GetReleaseLogEntryIfAny(this);
    }

    /// <inheritdoc/>
    public Type GetDestinationType() => MEF.GetType(_destinationType);

    /// <inheritdoc/>
    public bool IsReferenceTo(Type t) => t == typeof(ExtractableDataSet);

    /// <inheritdoc/>
    public bool IsReferenceTo(IMapsDirectlyToDatabaseTable o) =>
        o is ExtractableDataSet eds && eds.ID == ExtractionConfiguration_ID;

    /// <inheritdoc/>
    public void CompleteAudit(Type destinationType, string destinationDescription, int recordsExtracted,
        bool isBatchResume, bool failed)
    {
        DestinationType = destinationType.FullName;
        DestinationDescription = destinationDescription;

        if (isBatchResume)
        {
            if (!failed) RecordsExtracted += recordsExtracted;
        }
        else
        {
            RecordsExtracted = recordsExtracted;
        }

        SaveToDatabase();
    }

    /// <summary>
    /// Returns the name of the dataset for which this extraction is an audit of
    /// </summary>
    /// <returns></returns>
    public override string ToString() => ExtractableDataSet.Catalogue.Name;

    /// <inheritdoc/>
    public void InjectKnown(IExtractableDataSet instance)
    {
        _knownExtractableDataSet = new Lazy<IExtractableDataSet>(instance);
    }

    /// <inheritdoc/>
    public void ClearAllInjections()
    {
        _knownExtractableDataSet =
            new Lazy<IExtractableDataSet>(() => Repository.GetObjectByID<ExtractableDataSet>(ExtractableDataSet_ID));
    }
}