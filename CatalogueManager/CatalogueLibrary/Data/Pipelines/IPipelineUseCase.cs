using System.Collections.Generic;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Describes a specific use case for executing an IPipeline under.  This includes specifying the type T of the data flow, if there is an explicit
    /// source/destination component instance which must be used, what objects are available for PreInitialize on components (GetInitializationObjects).
    /// 
    /// <para>An instance of IPipelineUseCase is not just the general case (which is defined by IDataFlowPipelineContext) but the specific hydrated use case 
    /// e.g. 'I want to Release Project 205'.</para>
    /// </summary>
    public interface IPipelineUseCase : IHasDesignTimeMode
    {
        object[] GetInitializationObjects();
        IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines);
        IDataFlowPipelineContext GetContext();
        object ExplicitSource { get; }
        object ExplicitDestination { get; }

        IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener);
    }
}
