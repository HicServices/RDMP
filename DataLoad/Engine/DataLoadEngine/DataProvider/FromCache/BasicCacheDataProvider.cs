using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using FAnsi.Discovery;

namespace DataLoadEngine.DataProvider.FromCache
{
    /// <summary>
    /// Simple implementation of abstract CachedFileRetriever which unzips/copies data out of the cache into the ForLoading directory according to
    /// the current IDataLoadJob coverage dates (workload).
    /// </summary>
    public class BasicCacheDataProvider : CachedFileRetriever
    {
        public override void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public override ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            var scheduledJob = ConvertToScheduledJob(job);
            
            var workload = GetDataLoadWorkload(scheduledJob);
            ExtractJobs(scheduledJob);

            job.PushForDisposal(new DeleteCachedFilesOperation(scheduledJob, workload));
            return ExitCodeType.Success;
        }
    }
}
