using System.Collections.Generic;
using System.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.UserPicks;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations
{
    public interface IExecuteDatasetExtractionDestination : IPluginDataFlowComponent<DataTable>, IDataFlowDestination<DataTable>, IPipelineRequirement<IExtractCommand>, IPipelineRequirement<DataLoadInfo>
    {
        TableLoadInfo TableLoadInfo { get; }
        string GetDestinationDescription();

        void ExtractGlobals(Project project, ExtractionConfiguration configuration, GlobalsBundle globalsToExtract, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo);
    }
}
