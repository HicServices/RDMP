using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Abstract base IPipelineUseCase. Provides basic implementations for filtering compatible pipelines and translating
    /// a selected IPipeline into an actual executable engine instance via DataFlowPipelineEngineFactory.  Set ExplicitSource / 
    /// ExplicitDestination / PreInitialize objects etc as needed for your use case.
    /// </summary>
    public abstract class PipelineUseCase : IPipelineUseCase
    {
        public abstract object[] GetInitializationObjects();
        public abstract IDataFlowPipelineContext GetContext();

        public object ExplicitSource { get; protected set; }
        public object ExplicitDestination { get; protected set; }
        
        public virtual IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines)
        {
            var context = GetContext();
            return pipelines.Where(context.IsAllowable);
        }
        
        public virtual IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener)
        {
            var engine = new DataFlowPipelineEngineFactory(this).Create(pipeline, listener);
            engine.Initialize(GetInitializationObjects().ToArray());

            return engine;
        }
    }
}