// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Record of a single component extracted as part of an <see cref="IExtractionConfiguration"/>.  This could be an anonymised dataset or bundled supporting
/// documents e.g. Lookups , pdfs etc.  This audit is used to perform release process (where all extracted artifacts are collected and sent somewhere).
/// </summary>
public interface IExtractionResults : IReferenceOtherObject, IMapsDirectlyToDatabaseTable, ISaveable
{
    /// <summary>
    /// Description of the file path, database table name etc of the extracted artifact.  This must make sense to the pipeline
    /// component which extracted the artifact (See <see cref="DestinationType"/>)
    /// </summary>
    string DestinationDescription { get; }

    /// <summary>
    /// Type of the destination (final) component in the <see cref="IPipeline"/> used for extraction.  This is required so that
    /// artifacts can be collected again e.g. for release (Releasing from a flat file destination is different from releasing
    /// from a to database extraction).
    /// </summary>
    string DestinationType { get; }

    /// <summary>
    /// Total number of records in the dataset extracted
    /// </summary>
    int RecordsExtracted { get; }

    /// <summary>
    /// When the extraction began
    /// </summary>
    DateTime DateOfExtraction { get; }

    /// <summary>
    /// Null if the extraction completed successfully.  Otherwise populated with the fatal error that caused the extraction to stop.
    /// 
    /// <para>If this is not null then the extraction is considered to have been a failure.</para>
    /// </summary>
    string Exception { get; set; }

    /// <summary>
    /// The SQL Query used to extract records.  It is important that this is accurate since it is used to detect configuration changes
    /// (e.g. when trying to release a dataset in which the extracted artifacts are stale due to configuration changes in the live system vs
    /// the audit).
    /// </summary>
    string SQLExecuted { get; }

    /// <inheritdoc cref="DestinationType"/>
    Type GetDestinationType();


    /// <summary>
    /// Finalises an ongoing extraction audit.  This should only be called once at the end of the extraction process.
    /// </summary>
    /// <param name="destinationType">Type of the destination (final) component in the <see cref="IPipeline"/> used for extraction</param>
    /// <param name="destinationDescription"></param>
    /// <param name="recordsExtracted">Total number of records in the dataset extracted</param>
    /// <param name="isBatchResume">True if the <paramref name="recordsExtracted"/> is a subset of those already extracted</param>
    /// <param name="failed">True if the extraction pipeline execution failed otherwise false</param>
    void CompleteAudit(Type destinationType, string destinationDescription, int recordsExtracted, bool isBatchResume,
        bool failed);
}