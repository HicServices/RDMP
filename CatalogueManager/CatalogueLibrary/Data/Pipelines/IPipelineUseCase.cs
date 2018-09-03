using System.Collections.Generic;
using CatalogueLibrary.DataFlowPipeline.Requirements;
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
        /// <summary>
        /// All the objects available for executing the Pipeline.  
        /// <para>OR: If <see cref="IHasDesignTimeMode.IsDesignTime"/> then an array of the Types of objects that should be around at runtime
        /// when performing the task described by the PipelineUseCase</para>
        /// </summary>
        /// <returns></returns>
        HashSet<object> GetInitializationObjects();

        /// <summary>
        /// Returns all <see cref="Pipeline"/> from the collection which are compatible with the <see cref="GetContext"/> and <see cref="GetInitializationObjects"/>
        /// </summary>
        /// <param name="pipelines"></param>
        /// <returns></returns>
        IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines);

        /// <summary>
        /// Returns an object describing which <see cref="Pipeline"/>s can be used to undertake the activity described by this use case (e.g. loading a flat file into the
        /// database).  This includes the flow object (T) of and whether there are fixed sources/destinations as well as any forbidden <see cref="PipelineComponent"/> types
        /// </summary>
        /// <returns></returns>
        IDataFlowPipelineContext GetContext();

        /// <summary>
        /// The fixed runtime instance of <see cref="CatalogueLibrary.DataFlowPipeline.IDataFlowSource{T}"/> that will be used instead of an <see cref="IPipelineComponent"/> when
        /// running this use case. If this is populated then <see cref="Pipeline"/>s cannot have a user configured source component.
        /// </summary>
        object ExplicitSource { get; }

        /// <summary>
        /// The fixed runtime instance of <see cref="CatalogueLibrary.DataFlowPipeline.IDataFlowDestination{T}"/> that will be used instead of an <see cref="IPipelineComponent"/> when
        /// running this use case. If this is populated then <see cref="Pipeline"/>s cannot have a user configured destination component.
        /// </summary>
        object ExplicitDestination { get; }

        /// <summary>
        /// Constructs and engine from the provided <see cref="pipeline"/> initialized by <see cref="GetInitializationObjects"/>
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="listener"></param>
        /// <returns></returns>
        IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener);
    }
}
