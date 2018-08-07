using System;
using System.Linq;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Potential;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class ReleaseUseCase : PipelineUseCase
    {
        public ReleaseUseCase(IProject project, ReleaseData releaseData, ICatalogueRepository catalogueRepository)
        {
            ExplicitDestination = null;
            
            var releasePotentials = releaseData.ConfigurationsForRelease.Values.SelectMany(x => x).ToList();
            var releaseTypes = releasePotentials.Select(rp => rp.GetType()).Distinct().ToList();

            if (releaseTypes.Count == 0)
                throw new Exception("How did you manage to have multiple ZERO types in the extraction?");

            if (releaseTypes.Count(t => t != typeof (NoReleasePotential)) > 1)
                throw new Exception(
                    "You cannot release multiple configurations which have been extracted in multiple ways; e.g. " +
                    "one to DB and one to disk.  Your datasets have been extracted in the following ways:" +
                    Environment.NewLine + string.Join("," + Environment.NewLine, releaseTypes.Select(t => t.Name)));

            var releasePotentialWithKnownDestination =
                releasePotentials.FirstOrDefault(rp => rp.DatasetExtractionResult != null);

            if (releasePotentialWithKnownDestination == null)
                ExplicitSource = new NullReleaseSource<ReleaseAudit>();
            else
            {
                var destinationType = catalogueRepository.MEF
                    .GetTypeByNameFromAnyLoadedAssembly(
                        releasePotentialWithKnownDestination.DatasetExtractionResult.DestinationType,
                        typeof (IExecuteDatasetExtractionDestination));
                var constructor = new ObjectConstructor();
                var destinationUsedAtExtraction =
                    (IExecuteDatasetExtractionDestination)constructor.Construct(destinationType, catalogueRepository);

                FixedReleaseSource<ReleaseAudit> fixedReleaseSource =
                    destinationUsedAtExtraction.GetReleaseSource(catalogueRepository);

                ExplicitSource = fixedReleaseSource;
                    // destinationUsedAtExtraction.GetReleaseSource(); // new FixedSource<ReleaseAudit>(notifier => CheckRelease(notifier));    
            }

            AddInitializationObject(project);
            AddInitializationObject(releaseData);
            AddInitializationObject(catalogueRepository);
        }

        protected override IDataFlowPipelineContext GenerateContext()
        {
            var contextFactory = new DataFlowPipelineContextFactory<ReleaseAudit>();
            var context = contextFactory.Create(PipelineUsage.FixedSource);

            context.MustHaveDestination = typeof(IDataFlowDestination<ReleaseAudit>);

            return context;
        }

        public ReleaseUseCase():base(new []
        {
            typeof(Project),
            typeof(ReleaseData),
            typeof(CatalogueRepository)})
        {
            ExplicitSource = new NullReleaseSource<ReleaseAudit>();
        }

        public static ReleaseUseCase DesignTime()
        {
            return new ReleaseUseCase();
        }
    }
}