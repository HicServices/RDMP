using System.Data;
using CatalogueLibrary.DataFlowPipeline;

namespace DataExportLibrary.Interfaces.Pipeline
{
    public interface IPluginCohortDestination : ICohortPipelineDestination, IPluginDataFlowComponent<DataTable>
    {

    }
}