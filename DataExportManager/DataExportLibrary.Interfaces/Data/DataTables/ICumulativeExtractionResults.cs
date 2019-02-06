// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Referencing;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See CumulativeExtractionResults
    /// </summary>
    public interface ICumulativeExtractionResults : IExtractionResults, IRevertable
    {
        int ExtractionConfiguration_ID { get; set; }
        int ExtractableDataSet_ID { get; }

        int DistinctReleaseIdentifiersEncountered { get; set; }
        string FiltersUsed { get; set; }
        int CohortExtracted { get; }
        IExtractableDataSet ExtractableDataSet { get; }

        IReleaseLogEntry GetReleaseLogEntryIfAny();
        void CompleteAudit(Type destinationType, string destinationDescription, int recordsExtracted);

        List<ISupplementalExtractionResults> SupplementalExtractionResults { get; }
        ISupplementalExtractionResults AddSupplementalExtractionResult(string sqlExecuted, IMapsDirectlyToDatabaseTable extractedObject);
        bool IsFor(ISelectedDataSets selectedDataSet);

    }
}