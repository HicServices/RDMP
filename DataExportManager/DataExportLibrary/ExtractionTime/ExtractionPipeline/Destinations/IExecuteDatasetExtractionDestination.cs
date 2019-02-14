// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.IO;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.DataRelease.Potential;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations
{
    /// <summary>
    /// Destination for Extraction Pipelines.  Saves the extracted (anonymous) data contained in the DataTables received (which arrive in batches) to some location
    /// (depending on implementation).  Destinations must also support one off calls (per ExtractionConfiguration) to ExtractGlobals
    /// </summary>
    public interface IExecuteDatasetExtractionDestination : IPluginDataFlowComponent<DataTable>, IDataFlowDestination<DataTable>, IPipelineRequirement<IExtractCommand>, IPipelineRequirement<DataLoadInfo>
    {
        TableLoadInfo TableLoadInfo { get; }
        DirectoryInfo DirectoryPopulated { get; }
        bool GeneratesFiles { get; }
        string OutputFile { get; }
        int SeparatorsStrippedOut { get; }
        string DateFormat { get; }
        string GetFilename();
        string GetDestinationDescription();

        ReleasePotential GetReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISelectedDataSets selectedDataSet);
        FixedReleaseSource<ReleaseAudit> GetReleaseSource(ICatalogueRepository catalogueRepository);
        GlobalReleasePotential GetGlobalReleasabilityEvaluator(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck);
    }
}
