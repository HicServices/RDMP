using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.Potential;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.UserPicks;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Progress;

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
        FixedReleaseSource<ReleaseAudit> GetReleaseSource(CatalogueRepository catalogueRepository);
        GlobalReleasePotential GetGlobalReleasabilityEvaluator(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck);
    }
}
