// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExtractionConfiguration
    /// </summary>
    public interface IExtractionConfiguration:INamed,IHasDependencies
    {
        DateTime? dtCreated { get; set; }
        int? Cohort_ID { get; set; }
        string RequestTicket { get; set; }
        string ReleaseTicket { get; set; }
        int Project_ID { get; }
        IProject Project { get; }
        string Username { get; }
        string Separator { get; set; }
        string Description { get; set; }
        bool IsReleased { get; set; }
        int? ClonedFrom_ID { get; set; }

        IExtractableCohort Cohort { get;}
        
        int? DefaultPipeline_ID { get; set; }
        int? CohortIdentificationConfiguration_ID { get; set; }

        IExtractableCohort GetExtractableCohort();
        IProject GetProject();

        ISqlParameter[] GlobalExtractionFilterParameters { get; }
        IReleaseLogEntry[] ReleaseLogEntries { get; }
        IEnumerable<ICumulativeExtractionResults> CumulativeExtractionResults { get; }
        IEnumerable<ISupplementalExtractionResults> SupplementalExtractionResults { get; }

        ConcreteColumn[] GetAllExtractableColumnsFor(IExtractableDataSet dataset);
        IContainer GetFilterContainerFor(IExtractableDataSet dataset);
        IExtractableDataSet[] GetAllExtractableDataSets();
        ISelectedDataSets[] SelectedDataSets { get; }

        void RemoveDatasetFromConfiguration(IExtractableDataSet extractableDataSet);
        void Unfreeze();
        IMapsDirectlyToDatabaseTable[] GetGlobals();
    }
}