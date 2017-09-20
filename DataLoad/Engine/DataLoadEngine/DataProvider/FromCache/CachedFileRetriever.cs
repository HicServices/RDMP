using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using CachingEngine.Factories;
using CachingEngine.Layouts;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.Job.Scheduling;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataProvider.FromCache
{
    [Description("Fetches all the ILoadProgresss in the ILoadMetadata, it then selects the first scheduled task which has work to be done (e.g. data is cached but not yet loaded).  Cached data is unzipped to the forLoading directory.  The Dispose method (which should be called after the entire DataLoad has completed successfully) will clear out the cached file(s) that were loaded and update the schedule to indicate the successful loading of data")]
    public abstract class CachedFileRetriever : ICachedDataProvider
    {
        [DemandsInitialization("Whether to unarchive the files into the ForLoading folder, or just copy them as is")]
        public bool ExtractFilesFromArchive { get; set; }

        public abstract void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo);
        public abstract ExitCodeType Fetch(IDataLoadJob dataLoadJob, GracefulCancellationToken cancellationToken);
        
        #region Events
        public event CacheFileNotFoundHandler CacheFileNotFound;
        protected virtual void OnCacheFileNotFound(string message, Exception ex)
        {
            CacheFileNotFoundHandler handler = CacheFileNotFound;
            if (handler != null) handler(this, message, ex);
        }
        #endregion

        protected virtual ICacheLayout CreateCacheLayout(ScheduledDataLoadJob job)
        {
            var cacheProgress = job.LoadProgress.GetCacheProgress();

            if(cacheProgress == null)
                throw new NullReferenceException("cacheProgress cannot be null");

            var pipelineFactory = new CachingPipelineUseCase(cacheProgress);
            var destination = pipelineFactory.CreateDestinationOnly(job);
            return destination.CreateCacheLayout();
        }

        protected static ScheduledDataLoadJob ConvertToScheduledJob(IDataLoadJob dataLoadJob)
        {
            var scheduledJob = dataLoadJob as ScheduledDataLoadJob;

            if (scheduledJob == null)
                throw new Exception("CachedFileRetriever can only be used in conjunction with a ScheduledDataLoadJob");

            return scheduledJob;
        }

        protected Dictionary<DateTime, FileInfo> GetDataLoadWorkload(ScheduledDataLoadJob job)
        {
            var cacheLayout = CreateCacheLayout(job);
            cacheLayout.CheckCacheFilesAvailability();

            _workload = new Dictionary<DateTime, FileInfo>();
            foreach (var date in job.DatesToRetrieve)
            {
                var fileInfo = cacheLayout.GetArchiveFileInfoForDate(date);
                
                if (fileInfo == null)
                    OnCacheFileNotFound("Could not find cached file for date '" + date + "' for CacheLayout.ArchiveType " + cacheLayout.ArchiveType + " in cache at " + job.HICProjectDirectory.Cache.FullName, null);
                else
                if (!fileInfo.Exists)
                    OnCacheFileNotFound("Could not find cached file '" + fileInfo.FullName + "' for date " + date + " in cache at " + job.HICProjectDirectory.Cache.FullName, null);

                _workload.Add(date, fileInfo);
            }

            return _workload;
        }

        private Dictionary<DateTime, FileInfo> _workload;

        private string[] GetPathsRelativeToDirectory(FileInfo[] absoluteFilePaths, DirectoryInfo directory)
        {
            var relativeFilePaths = new List<string>();
            foreach (var path in absoluteFilePaths)
            {
                if (!path.FullName.StartsWith(directory.FullName))
                    throw new InvalidOperationException("The file must be within " + directory.FullName + " (or a subdirectory thereof)");

                relativeFilePaths.Add(path.FullName.Replace(directory.FullName, ""));
            }

            return relativeFilePaths.ToArray();
        }

        private bool FilesInForLoadingMatchWorkload(IHICProjectDirectory hicProjectDirectory)
        {
            var filesInForLoading = GetPathsRelativeToDirectory(hicProjectDirectory.ForLoading.EnumerateFiles("*", SearchOption.AllDirectories).ToArray(), hicProjectDirectory.ForLoading);
            var filesFromCache = GetPathsRelativeToDirectory(_workload.Values.ToArray(), hicProjectDirectory.Cache);

            return filesInForLoading.OrderBy(t => t).SequenceEqual(filesFromCache.OrderBy(t => t));
        }

        protected void ExtractJobs(IDataLoadJob dataLoadJob)
        {
            // check to see if forLoading has anything in it and bail if it does
            if (dataLoadJob.HICProjectDirectory.ForLoading.EnumerateFileSystemInfos().Any())
            {
                // RDMPDEV-185
                // There are files in ForLoading, but do they match what we would expect to find? Need to make sure that they aren't from a different dataset and/or there is the expected number of files
                // We should already have a _workload
                if (_workload == null)
                    throw new InvalidOperationException("The workload has not been initialised, don't know what files are to be retrieved from the cache");

                if (!FilesInForLoadingMatchWorkload(dataLoadJob.HICProjectDirectory))
                    throw new InvalidOperationException("The files in ForLoading do not match what this job expects to be loading from the cache. Please delete the files in ForLoading before re-attempting the data load.");

                dataLoadJob.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "ForLoading already has files, skipping extraction"));
                return;
            }
            
            var layout = CreateCacheLayout((ScheduledDataLoadJob)dataLoadJob);

            //extract all the jobs into the forLoading directory
            foreach (KeyValuePair<DateTime, FileInfo> job in _workload)
            {
                if (job.Value == null)
                    continue;

                if (ExtractFilesFromArchive)
                {
                    var extractor = CreateExtractor(layout.ArchiveType);
                    extractor.Extract(job, dataLoadJob.HICProjectDirectory.ForLoading, dataLoadJob);
                }
                else
                {
                    dataLoadJob.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Archive identified:" + job.Value.FullName));

                    // just copy the archives across
                    var relativePath = GetPathRelativeToCacheRoot(dataLoadJob.HICProjectDirectory.Cache, job.Value);
                    var absolutePath = Path.Combine(dataLoadJob.HICProjectDirectory.ForLoading.FullName, relativePath);
                    if (!Directory.Exists(absolutePath))
                        Directory.CreateDirectory(absolutePath);

                    var destFileName = Path.Combine(absolutePath, job.Value.Name);
                    job.Value.CopyTo(destFileName);
                }
            }
        }

        private string GetPathRelativeToCacheRoot(DirectoryInfo cacheRoot, FileInfo fileInCache)
        {
            return fileInCache.Directory.FullName.Replace(cacheRoot.FullName, "").TrimStart(Path.DirectorySeparatorChar);
        }

        private IArchivedFileExtractor CreateExtractor(CacheArchiveType cacheArchiveType)
        {
            switch (cacheArchiveType)
            {
                case CacheArchiveType.None:
                    throw new Exception("At this stage a cache archive type must be specified");
                case CacheArchiveType.Zip:
                    return new ZipExtractor();
                case CacheArchiveType.Tar:
                    return new TarExtractor();
                default:
                    throw new ArgumentOutOfRangeException("cacheArchiveType");
            }
        }

        public bool Validate(IHICProjectDirectory destination)
        {
            if (destination.Cache == null)
                throw new NullReferenceException("Destination " + destination.RootPath.FullName + " does not have a 'Cache' folder");

            if (!destination.Cache.Exists)
                throw new DirectoryNotFoundException(destination.Cache.FullName);

            return true;
        }

        

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
        }

        public void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Runtime checks are dependent on which CacheProgress if any is executed so cannot run any checks at this time",CheckResult.Warning));
        }

    }

    public delegate void CacheFileNotFoundHandler(object sender, string message, Exception ex);
}