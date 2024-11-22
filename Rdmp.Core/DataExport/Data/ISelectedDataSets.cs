// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
/// Represents the desire to extract a given dataset for a given <see cref="ExtractableDataSet"/> for a given <see cref="ExtractionConfiguration"/>.
/// </summary>
public interface ISelectedDataSets : IRevertable, IRootFilterContainerHost
{
    /// <summary>
    /// <see cref="IExtractionConfiguration"/> in which the <see cref="ExtractableDataSet_ID"/> is selected
    /// </summary>
    int ExtractionConfiguration_ID { get; set; }

    /// <summary>
    /// <see cref="IExtractableDataSet"/> that has been selected for extraction in the <see cref="ExtractionConfiguration_ID"/>
    /// </summary>
    int ExtractableDataSet_ID { get; set; }


    /// <inheritdoc cref="ExtractionConfiguration_ID"/>
    IExtractionConfiguration ExtractionConfiguration { get; }

    /// <inheritdoc cref="ExtractableDataSet_ID"/>
    IExtractableDataSet ExtractableDataSet { get; }

    /// <summary>
    /// If date based batch extraction is being used for this dataset then this
    /// describes what range should be retrieved and in what incremental amount
    /// </summary>
    IExtractionProgress ExtractionProgressIfAny { get; }

    /// <summary>
    /// Returns all tables which should be force joined against when extracting the <see cref="ISelectedDataSets"/> (regardless of extracted columns).
    /// This does not include implicitly joined <see cref="ITableInfo"/> (i.e. if you are extracting a column from that table).
    /// </summary>
    ISelectedDataSetsForcedJoin[] SelectedDataSetsForcedJoins { get; }

    /// <summary>
    /// If this dataset has been extracted in the past this will return the last extract audit record.  Otherwise it will return null
    /// </summary>
    /// <returns></returns>
    ICumulativeExtractionResults GetCumulativeExtractionResultsIfAny();
}