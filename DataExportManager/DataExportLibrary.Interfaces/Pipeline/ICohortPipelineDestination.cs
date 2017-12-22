using System.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace DataExportLibrary.Interfaces.Pipeline
{
    /// <summary>
    /// Destination component interface for Cohort Creation Pipelines.  Must fulfill the ICohortCreationRequest and populate the Cohort Source with the 
    /// identifiers supplied in the DataTable.
    /// </summary>
    public interface ICohortPipelineDestination : IDataFlowDestination<DataTable>, IPipelineRequirement<ICohortCreationRequest>
    {
    }
}
