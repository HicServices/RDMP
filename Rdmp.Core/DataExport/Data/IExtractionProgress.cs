// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Progress;
using System;

namespace Rdmp.Core.DataExport.Data
{
    /// <summary>
    /// Records how far through a batch extraction a <see cref="SelectedDataSets"/> is.  Also tracks which column is being
    /// used for the batch splitting.
    /// </summary>
    public interface IExtractionProgress : IMapsDirectlyToDatabaseTable, ISaveable, INamed
    {
        /// <summary>
        /// The absolute origin date of the dataset being extracted.  This is the first day of the first batch
        /// that is extracted when running a batch extraction
        /// </summary>
        DateTime? StartDate { get; set; }

        /// <summary>
        /// The absolute end date of the dataset after which there is assumed to be no data.  If null then 
        /// the current datetime is expected
        /// </summary>
        DateTime? EndDate { get; set; }

        /// <summary>
        /// The column or transform which provides the date component of the extraction upon which
        /// <see cref="StartDate"/>, <see cref="EndDate"/> etc relate
        /// </summary>
        int ExtractionInformation_ID { get; set; }

        /// <summary>
        /// When running a batch extraction this is the number of days to fetch/extract at a time
        /// </summary>
        int NumberOfDaysPerBatch { get; set; }

        /// <summary>
        /// The inclusive day at which the next batch should start at
        /// </summary>
        DateTime? ProgressDate { get; set; }

        /// <summary>
        /// The dataset as it is selected in an <see cref="ExtractionConfiguration"/> for which this class
        /// documents the progress of it's batch extraction
        /// </summary>
        int SelectedDataSets_ID { get; set; }

        /// <inheritdoc cref="SelectedDataSets_ID"/>
        ISelectedDataSets SelectedDataSets { get; }

        /// <inheritdoc cref="ExtractionInformation_ID"/>
        ExtractionInformation ExtractionInformation { get; }

        /// <summary>
        /// When extraction fails, what is the policy on retrying
        /// </summary>
        RetryStrategy Retry { get; }

        /// <summary>
        /// Returns true if the progress is not completed yet
        /// </summary>
        /// <returns></returns>
        bool MoreToFetch();

        /// <summary>
        /// Blocks for a suitable amount of time (if any) based on the <see cref="Retry"/>
        /// strategy configured.  Then returns true to retry or false to abandon retrying
        /// </summary>
        /// <param name="token">Cancellation token for if the user wants to abandon waiting</param>
        /// <param name="listener"></param>
        /// <param name="totalFailureCount"></param>
        /// <param name="consecutiveFailureCount"></param>
        /// <returns></returns>
        bool ApplyRetryWaitStrategy(GracefulCancellationToken token, IDataLoadEventListener listener, int totalFailureCount, int consecutiveFailureCount);
    }
}