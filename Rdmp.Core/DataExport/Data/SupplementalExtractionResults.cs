// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc cref="ISupplementalExtractionResults" />
public class SupplementalExtractionResults : ReferenceOtherObjectDatabaseEntity, ISupplementalExtractionResults
{
    #region Database Properties

    private int? _cumulativeExtractionResults_ID;
    private int? _extractionConfiguration_ID;
    private string _destinationDescription;
    private int _recordsExtracted;
    private DateTime _dateOfExtraction;
    private string _exception;
    private string _sQLExecuted;
    private string _extractedName;
    private string _destinationType;

    /// <inheritdoc />
    public int? CumulativeExtractionResults_ID
    {
        get => _cumulativeExtractionResults_ID;
        set => SetField(ref _cumulativeExtractionResults_ID, value);
    }

    /// <inheritdoc />
    public int? ExtractionConfiguration_ID
    {
        get => _extractionConfiguration_ID;
        set => SetField(ref _extractionConfiguration_ID, value);
    }

    /// <inheritdoc />
    public string DestinationDescription
    {
        get => _destinationDescription;
        set => SetField(ref _destinationDescription, value);
    }

    /// <inheritdoc />
    public int RecordsExtracted
    {
        get => _recordsExtracted;
        set => SetField(ref _recordsExtracted, value);
    }

    /// <inheritdoc />
    public DateTime DateOfExtraction
    {
        get => _dateOfExtraction;
        private set => SetField(ref _dateOfExtraction, value);
    }

    /// <inheritdoc />
    public string Exception
    {
        get => _exception;
        set => SetField(ref _exception, value);
    }

    /// <inheritdoc />
    public string SQLExecuted
    {
        get => _sQLExecuted;
        set => SetField(ref _sQLExecuted, value);
    }

    /// <inheritdoc />
    public string ExtractedName
    {
        get => _extractedName;
        set => SetField(ref _extractedName, value);
    }

    /// <inheritdoc />
    public string DestinationType
    {
        get => _destinationType;
        private set => SetField(ref _destinationType, value);
    }

    #endregion

    /// <inheritdoc />
    [NoMappingToDatabase]
    public bool IsGlobal { get; }

    public SupplementalExtractionResults()
    {
    }

    /// <summary>
    ///     Starts a new audit in the database of a supplemental artifact (<paramref name="extractedObject" />).  This is a
    ///     GLOBAL artifact that is not associated
    ///     with any one dataset but with the extraction as a whole.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="configuration">The configuration being extracted</param>
    /// <param name="sql">
    ///     The SQL executed to generate the artifact or null if not appropriate (e.g. if it is a
    ///     <see cref="SupportingDocument" />)
    /// </param>
    /// <param name="extractedObject">
    ///     The owner of the artifact being extracted (e.g. a <see cref="SupportingDocument" /> or
    ///     <see cref="SupportingSQLTable" />)
    /// </param>
    public SupplementalExtractionResults(IDataExportRepository repository, IExtractionConfiguration configuration,
        string sql, IMapsDirectlyToDatabaseTable extractedObject)
    {
        Repository = repository;
        var name = extractedObject.GetType().FullName;

        if (extractedObject is INamed named)
            name = named.Name;

        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ReferencedObjectID", extractedObject.ID },
            { "ReferencedObjectType", extractedObject.GetType().Name },
            { "ReferencedObjectRepositoryType", extractedObject.Repository.GetType().Name },
            { "ExtractionConfiguration_ID", configuration.ID },
            { "SQLExecuted", sql },
            { "ExtractedName", name }
        });

        IsGlobal = true;
    }

    /// <summary>
    ///     Starts a new audit in the database of a supplemental artifact (<paramref name="extractedObject" />).  This is a NON
    ///     GLOBAL artifact that is associated only
    ///     with the dataset being extracted.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="mainAudit">The dataset extraction audit for the dataset to which this supplemental artifact belongs</param>
    /// <param name="sql">
    ///     The SQL executed to generate the artifact or null if not appropriate (e.g. if it is a
    ///     <see cref="SupportingDocument" />)
    /// </param>
    /// <param name="extractedObject">
    ///     The owner of the artifact being extracted (e.g. a <see cref="SupportingDocument" /> or
    ///     <see cref="SupportingSQLTable" />)
    /// </param>
    public SupplementalExtractionResults(IDataExportRepository repository, ICumulativeExtractionResults mainAudit,
        string sql, IMapsDirectlyToDatabaseTable extractedObject)
    {
        Repository = repository;

        var name = extractedObject.GetType().FullName;

        if (extractedObject is INamed named)
            name = named.Name;

        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ReferencedObjectID", extractedObject.ID },
            { "ReferencedObjectType", extractedObject.GetType().Name },
            { "ReferencedObjectRepositoryType", extractedObject.Repository.GetType().Name },
            { "CumulativeExtractionResults_ID", mainAudit.ID },
            { "SQLExecuted", sql },
            { "ExtractedName", name }
        });

        IsGlobal = false;
    }

    /// <summary>
    ///     Reads an existing audit record out of the data export database
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal SupplementalExtractionResults(IRepository repository, DbDataReader r)
        : base(repository, r)
    {
        CumulativeExtractionResults_ID = ObjectToNullableInt(r["CumulativeExtractionResults_ID"]);
        ExtractionConfiguration_ID = ObjectToNullableInt(r["ExtractionConfiguration_ID"]);
        DestinationDescription = r["DestinationDescription"] as string;
        RecordsExtracted = r["RecordsExtracted"] is DBNull ? 0 : Convert.ToInt32(r["RecordsExtracted"]);
        DateOfExtraction = (DateTime)r["DateOfExtraction"];
        Exception = r["Exception"] as string;
        SQLExecuted = r["SQLExecuted"] as string;
        ExtractedName = r["ExtractedName"] as string;
        DestinationType = r["DestinationType"] as string;

        IsGlobal = CumulativeExtractionResults_ID == null && ExtractionConfiguration_ID != null;
    }

    /// <inheritdoc />
    public Type GetDestinationType()
    {
        return MEF.GetType(DestinationType);
    }

    /// <inheritdoc />
    public void CompleteAudit(Type destinationType, string destinationDescription, int distinctIdentifiers,
        bool isBatchResume, bool failed)
    {
        DestinationType = destinationType.FullName;
        DestinationDescription = destinationDescription;
        RecordsExtracted = distinctIdentifiers;

        SaveToDatabase();
    }

    /// <summary>
    ///     Returns <see cref="ExtractedName" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ExtractedName;
    }
}