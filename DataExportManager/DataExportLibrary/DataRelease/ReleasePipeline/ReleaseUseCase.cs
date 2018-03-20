using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using MapsDirectlyToDatabaseTable.Revertable;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class ReleaseUseCase : PipelineUseCase
    {
        private readonly Project _project;
        private readonly ReleaseData _releaseData;
        private readonly DataFlowPipelineContext<ReleaseAudit> _context;
        private readonly object[] _initObjects;
        private CatalogueRepository _catalogueRepository;

        public ReleaseUseCase(Project project, ReleaseData releaseData)
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
                var releaseTypes = releaseData.ConfigurationsForRelease.Values.SelectMany(x => x).Distinct().ToList();
                if (releaseTypes.Count() != 1)
                    throw new Exception("How did you manage to have multiple (or zero) types in the extraction?");

                var releaseType = releaseTypes.First();

                var destinationType = _catalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(releaseType.ExtractionResults.DestinationType, typeof(IExecuteDatasetExtractionDestination));
                ObjectConstructor constructor = new ObjectConstructor();

                var destinationUsedAtExtraction = (IExecuteDatasetExtractionDestination)constructor.Construct(destinationType, _catalogueRepository);

                FixedReleaseSource<ReleaseAudit> fixedReleaseSource = destinationUsedAtExtraction.GetReleaseSource();

                ExplicitSource = fixedReleaseSource;// destinationUsedAtExtraction.GetReleaseSource(); // new FixedSource<ReleaseAudit>(notifier => CheckRelease(notifier));    
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

        private void CheckRelease(ICheckNotifier notifier)
        {
            if (_releaseData.IsDesignTime)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Stale datasets will be checked at runtime...", CheckResult.Success));
                return;
            }

            var staleDatasets = _releaseData.ConfigurationsForRelease.SelectMany(c => c.Value).Where(
                   p => p.ExtractionResults.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted).ToArray();

            if (staleDatasets.Any())
                throw new Exception(
                    "The following ReleasePotentials relate to expired (stale) extractions, you or someone else has executed another data extraction since you added this dataset to the release.  Offending datasets were (" +
                    string.Join(",", staleDatasets.Select(ds => ds.ToString())) + ").  You can probably fix this problem by reloading/refreshing the Releaseability window.  If you have already added them to a planned Release you will need to add the newly recalculated one instead.");
        }
    }
}