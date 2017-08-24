using System.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace DataExportLibrary.Interfaces.Pipeline
{
    public interface ICohortPipelineDestination : IDataFlowDestination<DataTable>, IPipelineRequirement<ICohortCreationRequest>
    {
    }
}
