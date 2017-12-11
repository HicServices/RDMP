using System.Collections.Generic;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Interface for the Generic IDataFlowPipelineEngine T.  An IDataFlowPipelineEngine is a collection of IDataFlowComponents starting with an IDataFlowSource and
    /// ending with an IDataFlowDestination with any number of IDataFlowComponents in the middle.  Each component must operate on the class that flows through which is
    /// of type T (see the Generic implementation).  
    /// 
    /// Before running the IDataFlowPipelineEngine you should call Initialize with the objects that are available for IPipelineRequirement on components.
    /// 
    /// See also Pipeline
    /// </summary>
    public interface IDataFlowPipelineEngine : ICheckable
    {
        /// <summary>
        /// Runs all components from source to destination repeatedly until the source returs null;
        /// </summary>
        /// <param name="cancellationToken"></param>
        void ExecutePipeline(GracefulCancellationToken cancellationToken);

        /// <summary>
        /// Runs the source GetChunk once and passes it down through the other components to the destination
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        bool ExecuteSinglePass(GracefulCancellationToken cancellationToken);

        /// <summary>
        /// Components can declare IPipelineRequirement, calling this method will PreInitialize all components with compatible IPipelineRequirements with the values
        /// provided.  This is used to for example tell an ExecuteDatasetExtractionSource what IExtractCommand it is supposed to be running.
        /// </summary>
        /// <param name="initializationObjects"></param>
        void Initialize(params object[] initializationObjects);

        /// <summary>
        /// All middle IDataFlowComponents in the pipeline (except the source / destination)
        /// </summary>
        List<object> ComponentObjects { get; }

        /// <summary>
        /// The IDataFlowDestination component at the end of the pipeline
        /// </summary>
        object DestinationObject { get; }

        /// <summary>
        /// The IDataFlowSource component at the start of the pipeline
        /// </summary>
        object SourceObject { get; }
        
    }
}