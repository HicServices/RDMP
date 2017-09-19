using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class ReleaseContext:IPipelineUseCase
    {
        private readonly Project _project;
        private IDataFlowSource<ReleaseData> _explicitSource;
        private DataFlowPipelineContext<ReleaseData> _context { get; set; }

        public ReleaseContext(Project project, IDataFlowSource<ReleaseData> explicitSource)
        {
            _explicitSource = explicitSource;
            _project = project;
            var contextFactory = new DataFlowPipelineContextFactory<ReleaseData>();
            _context = contextFactory.Create(PipelineUsage.None);
            _context.CannotHave.Add(typeof(IDataFlowSource<ReleaseData>));
            
            _context.MustHaveDestination = typeof(IDataFlowDestination<ReleaseData>);
            ExplicitDestination = null;
        }

        public object[] GetInitializationObjects(ICatalogueRepository repository)
        {
            return new []{_project};
        }

        public IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines)
        {
            return pipelines.Where(_context.IsAllowable);
        }

        public IDataFlowPipelineContext GetContext()
        {
            return _context;
        }

        public object ExplicitSource { get { return _explicitSource; }}
        public object ExplicitDestination { get; private set; }

        public IDataFlowPipelineEngine GetEngine(IPipeline pipeline,IDataLoadEventListener listener)
        {
            var repo = (CatalogueRepository)pipeline.Repository;

            var factory = new DataFlowPipelineEngineFactory<ReleaseData>(repo.MEF, _context);
            factory.ExplicitSource = _explicitSource;

            var engine = factory.Create(pipeline, listener);
            
            engine.Initialize(_project);

            return engine;
        }
    }
}