using System;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class FixedDataReleaseSource : IPluginDataFlowSource<ReleaseData>
    {
        private bool firstTime = true;

        public ReleaseData CurrentRelease { get; set; }

        public FixedDataReleaseSource()
        {
            CurrentRelease = new ReleaseData();
        }

        public ReleaseData GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            CheckForCumulativeExtractionResults(CurrentRelease.ConfigurationsForRelease.SelectMany(c => c.Value).ToArray());

            if(firstTime)
            {
                firstTime = false;
                return CurrentRelease;
            }

            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            firstTime = true;
        }

        public void Abort(IDataLoadEventListener listener)
        {   
        }

        public ReleaseData TryGetPreview()
        {
            return null;
        }

        public void Check(ICheckNotifier notifier)
        {
        }

        private void CheckForCumulativeExtractionResults(ReleasePotential[] datasetReleasePotentials)
        {
            var staleDatasets = datasetReleasePotentials.Where(
                p => p.ExtractionResults.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted).ToArray();

            if (staleDatasets.Any())
                throw new Exception(
                    "The following ReleasePotentials relate to expired (stale) extractions, you or someone else has executed another data extraction since you added this dataset to the release.  Offending datasets were (" +
                    string.Join(",", staleDatasets.Select(ds => ds.ToString())) + ").  You can probably fix this problem by reloading/refreshing the Releaseability window.  If you have already added them to a planned Release you will need to add the newly recalculated one instead.");
        }
    }
}
