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
    public class ReleaseUseCase:PipelineUseCase
    {
        private readonly Project _project;
        private readonly DataFlowPipelineContext<ReleaseData> _context;
        private readonly object[] _initObjects;
        private CatalogueRepository _catalogueRepository;

        public ReleaseUseCase(Project project, IDataFlowSource<ReleaseData> explicitSource)
        {
            ExplicitSource = explicitSource;
            ExplicitDestination = null;

            _project = project;

            if(_project != null)
                _catalogueRepository = ((IDataExportRepository)project.Repository).CatalogueRepository;

            var contextFactory = new DataFlowPipelineContextFactory<ReleaseData>();
            _context = contextFactory.Create(PipelineUsage.None);
            _context.CannotHave.Add(typeof(IDataFlowSource<ReleaseData>));
            
            _context.MustHaveDestination = typeof(IDataFlowDestination<ReleaseData>);
            

            _initObjects = new object[] {_project,_catalogueRepository};
        }

        public override object[] GetInitializationObjects(ICatalogueRepository repository)
        {
            return _initObjects;
        }

        public override IDataFlowPipelineContext GetContext()
        {
            return _context;
        }
    }
}