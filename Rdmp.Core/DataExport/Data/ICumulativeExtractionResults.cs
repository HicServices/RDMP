// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Audit record of the final SQL generated/used to perform an extraction of any <see cref="ExtractionConfiguration" />
///     and the number of extracted rows etc.
///     This record is overwritten if you re-extract the ExtractionConfiguration again.  The record is used to ensure that
///     you cannot release an extract if there have been changes
///     to the configuration subsequent to your last extraction.  This is particularly useful if you have many large
///     datasets that you are extracting over a long period of time either
///     because they are very large, have complex filters or are unstable.  Under such circumstances you can extract half
///     of your datasets one day and
///     then adjust the others to correct issues and be confident that the system is tracking those changes to ensure that
///     the current state of the system always matches the extracted
///     files at release time.
/// </summary>
public interface ICumulativeExtractionResults : IExtractionResults, IRevertable
{
    /// <summary>
    ///     This class is the audit of a the latest extraction attempt of a given dataset in a given extraction configuration.
    ///     <para>This property is the ID of the <see cref="IExtractionConfiguration" /> being audited</para>
    /// </summary>
    int ExtractionConfiguration_ID { get; set; }


    /// <summary>
    ///     This class is the audit of a the latest extraction attempt of a given dataset in a given extraction configuration.
    ///     <para>This property is the ID of the <see cref="IExtractableDataSet" /> being audited</para>
    /// </summary>
    int ExtractableDataSet_ID { get; }

    /// <summary>
    ///     Count of the number of unique anonymous release identifiers encountered during extraction.
    /// </summary>
    int DistinctReleaseIdentifiersEncountered { get; set; }


    /// <summary>
    ///     Description of all the <see cref="IFilter" /> that existed on the dataset that was extracted (e.g. "Extract only
    ///     Tayside records").
    /// </summary>
    string FiltersUsed { get; set; }

    /// <summary>
    ///     The ID of the <see cref="IExtractableCohort" /> that was linked against the dataset during extraction.
    /// </summary>
    int CohortExtracted { get; }

    /// <inheritdoc cref="ExtractableDataSet_ID" />
    IExtractableDataSet ExtractableDataSet { get; }

    /// <summary>
    ///     If the extracted artifacts have been packaged up and released then this method returns the audit object for that
    ///     release.
    /// </summary>
    /// <returns>Release audit or null if artifacts have not been released</returns>
    IReleaseLog GetReleaseLogEntryIfAny();

    /// <summary>
    ///     If there are supplemental artifacts produced during extraction (e.g. lookup tables) then this method returns the
    ///     audit object(s) for these
    /// </summary>
    List<ISupplementalExtractionResults> SupplementalExtractionResults { get; }

    /// <summary>
    ///     Records the fact that a given supplemental artifact has been produced by the extraction process (e.g. a lookup
    ///     table)
    /// </summary>
    /// <param name="sqlExecuted"></param>
    /// <param name="extractedObject"></param>
    /// <returns></returns>
    ISupplementalExtractionResults AddSupplementalExtractionResult(string sqlExecuted,
        IMapsDirectlyToDatabaseTable extractedObject);

    /// <summary>
    ///     Returns true if the audit described by this class is for the given <see cref="ISelectedDataSets" />
    /// </summary>
    /// <param name="selectedDataSet"></param>
    /// <returns></returns>
    bool IsFor(ISelectedDataSets selectedDataSet);
}