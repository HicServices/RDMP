using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.Data.Pipelines
{
    public abstract class PipelineUseCase : IPipelineUseCase
    {
        public abstract object[] GetInitializationObjects(ICatalogueRepository repository);
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
            engine.Initialize(GetInitializationObjects((ICatalogueRepository) pipeline.Repository).ToArray());

            return engine;
        }
    }
}