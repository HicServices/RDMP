using System.Collections.Generic;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Interface for components (of PipelineComponents or ProcessTasks) which have [DemandsInitialization] properties of type Pipeline.  This lets you have a pipeline 
    /// component which requires the user select another Pipeline as one of it's arguments.  You might want to do this for example if you have a standard pipeline for
    /// reading records and you want to use it in many places (in many other pipelines).  You must define the Context and any Fixed components.  Note that you can even
    /// set yourself (this) to the FixedDestination to effectively join two IPipelines together.  
    /// 
    /// The user will only be able to select IPipelines which are compatible with the Context you provide (so it won't for example override source/destination etc).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDemandToUseAPipeline<T>
    {
        DataFlowPipelineContext<T> GetContext();
        IDataFlowSource<T> GetFixedSourceIfAny();
        IDataFlowDestination<T> GetFixedDestinationIfAny();

        List<object> GetInputObjectsForPreviewPipeline();
    }
}