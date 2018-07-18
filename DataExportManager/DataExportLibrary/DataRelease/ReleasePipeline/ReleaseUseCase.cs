using System;
using System.Linq;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class ReleaseUseCase : PipelineUseCase
    {
        private readonly IProject _project;
        private readonly ReleaseData _releaseData;
        private readonly DataFlowPipelineContext<ReleaseAudit> _context;
        private readonly object[] _initObjects;
        private CatalogueRepository _catalogueRepository;

        public ReleaseUseCase(IProject project, ReleaseData releaseData)
        {
            ExplicitDestination = null;

            _project = project;
            _releaseData = releaseData;

            if(_project != null)
                _catalogueRepository = ((IDataExportRepository)project.Repository).CatalogueRepository;

            if (releaseData.IsDesignTime)
            {
                ExplicitSource = new NullReleaseSource<ReleaseAudit>();
            }
            else
            {
                var releasePotentials = releaseData.ConfigurationsForRelease.Values.SelectMany(x => x).ToList();
                var releaseTypes = releasePotentials.Select(rp => rp.GetType().FullName).Distinct().ToList();
                if (releaseTypes.Count() != 1)
                    throw new Exception("How did you manage to have multiple (or zero) types in the extraction?");

                var releasePotentialWithKnownDestination = releasePotentials.FirstOrDefault(rp => rp.DatasetExtractionResult != null);

                if(releasePotentialWithKnownDestination == null)
                    ExplicitSource = new NullReleaseSource<ReleaseAudit>();
                else
                {
                    var destinationType = _catalogueRepository.MEF
                                                .GetTypeByNameFromAnyLoadedAssembly(releasePotentialWithKnownDestination.DatasetExtractionResult.DestinationType, 
                                                                                    typeof(IExecuteDatasetExtractionDestination));
                    var constructor = new ObjectConstructor();
                    var destinationUsedAtExtraction = (IExecuteDatasetExtractionDestination)constructor.Construct(destinationType, _catalogueRepository);

                    FixedReleaseSource<ReleaseAudit> fixedReleaseSource = destinationUsedAtExtraction.GetReleaseSource(_catalogueRepository);

                    ExplicitSource = fixedReleaseSource;// destinationUsedAtExtraction.GetReleaseSource(); // new FixedSource<ReleaseAudit>(notifier => CheckRelease(notifier));    
                }
            }
            
            var contextFactory = new DataFlowPipelineContextFactory<ReleaseAudit>();
            _context = contextFactory.Create(PipelineUsage.FixedSource);

            _context.MustHaveDestination = typeof(IDataFlowDestination<ReleaseAudit>);

            _initObjects = new object[] { _project, _releaseData, _catalogueRepository };
        }

        public override object[] GetInitializationObjects()
        {
            return _initObjects;
        }

        public override IDataFlowPipelineContext GetContext()
        {
            return _context;
        }
    }
}