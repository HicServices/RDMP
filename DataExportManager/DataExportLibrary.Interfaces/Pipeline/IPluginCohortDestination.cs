using System.Data;
using CatalogueLibrary.DataFlowPipeline;

namespace DataExportLibrary.Interfaces.Pipeline
{
    /// <summary>
    /// MEF discoverble version of ICohortPipelineDestination (See ICohortPipelineDestination).  Implement this interface if you are writing a custom cohort
    /// storage system and need to populate it with identifiers through the RDMP Cohort Creation Pipeline Processes.
    /// </summary>
    public interface IPluginCohortDestination : ICohortPipelineDestination, IPluginDataFlowComponent<DataTable>
    {

    }
}