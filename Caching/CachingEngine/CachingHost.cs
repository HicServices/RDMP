using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CachingEngine.DataRetrievers;
using CachingEngine.PipelineExecution;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;

namespace CachingEngine
{
    /// <summary>
    /// The CachingHost has two public interfaces: 'Start' and 'StartDaemon'. 
    /// 'Start' is a one-shot mode where any available CacheProgress records are cached until completion (this could be a very long 'one-shot').
    /// 'StartDaemon' continually attempt to cache available CacheProgress records until cancelled. This mode will keep the caches up-to-date.
    /// </summary>
    public class CachingHost
    {
        public List<ICacheProgress> CacheProgressList { get; set; }
        public List<IPermissionWindow> PermissionWindows { get; set; }
        public bool RetryMode { get; set; }
        // this is more because we can't retrieve CacheWindows from LoadProgresss (yet) 

        private readonly ICatalogueRepository _repository;

        private List<PermissionWindowCacheDownloader> _downloaders;
        
        public bool TerminateIfOutsidePermissionWindow { get; set; }

        public CachingHost(ICatalogueRepository repository)
        {
            _repository = repository;
            RetryMode = false;
        }

        public void Start(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (CacheProgressList.Count > 1)
                throw new InvalidOperationException(
                    "Currently this entrypoint only supports single CacheProgress retrieval, ensure CacheProgressList only has one item (it has " +
                    CacheProgressList.Count + ")");

            var cacheProgress = CacheProgressList[0];
            var permissionWindow = cacheProgress.PermissionWindow;

            _downloaders = new List<PermissionWindowCacheDownloader>
            {
                new PermissionWindowCacheDownloader(permissionWindow, CacheProgressList, _repository, new RoundRobinPipelineExecution())
            };


            RetrieveNewDataForCache(listener, cancellationToken);
        }

        public void StartDaemon(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            const int sleepInSeconds = 60;

            while (!cancellationToken.IsCancellationRequested)
            {
                RetrieveNewDataForCache(listener, cancellationToken);
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information, "Sleeping for " + sleepInSeconds + " seconds"));

                // wake up every sleepInSeconds to re-check if we can download any new data, but check more regularly to see if cancellation has been requested
                var beenAsleepFor = new Stopwatch();
                beenAsleepFor.Start();
                while (beenAsleepFor.ElapsedMilliseconds < (sleepInSeconds*1000))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Information, "Cancellation has been requested"));
                        break;
                    }

                    Task.Delay(100).Wait();
                }
            }

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Daemon has stopped"));
        }

        public void CacheUsingPermissionWindows(List<IPermissionWindow> permissionWindowList, IDataLoadEventListener listener, GracefulCancellationToken token)
        {
            _downloaders = new List<PermissionWindowCacheDownloader>();

            foreach (var permissionWindow in permissionWindowList)
            {
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        "Creating download for permission window: " + permissionWindow.Name));
                _downloaders.Add(new PermissionWindowCacheDownloader(permissionWindow,  _repository,
                    new RoundRobinPipelineExecution()));
            }

            StartDaemon(listener, token);
        }

        public void CacheUsingCacheProgresses(List<ICacheProgress> cacheProgressList, IDataLoadEventListener listener, GracefulCancellationToken token)
        {
            // organise the CacheProgress items into CacheSets, based on any PermissionWindows
            if (CacheProgressList == null)
                CacheProgressList = cacheProgressList;
            else
                throw new NotSupportedException("CacheProgressList property has already been set... thats probably a problem right? TN2016-08-25");

            _downloaders = new List<PermissionWindowCacheDownloader>();
            _downloaders.Add(new PermissionWindowCacheDownloader(null, CacheProgressList, _repository, new RoundRobinPipelineExecution()));

            StartDaemon(listener, token);
        }

        private void RetrieveNewDataForCache(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Retrieving new data"));

            var combinedToken = cancellationToken.CreateLinkedSource().Token;

            // Start a task for each cache download permission window and wait until completion
            var tasks =
                _downloaders.Select(
                    downloader =>
                        Task.Run(() => DownloadUntilFinished(downloader, listener, cancellationToken), combinedToken))
                    .ToArray();

            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException e)
            {
                var operationCanceledException = e.GetExceptionIfExists<OperationCanceledException>();
                if (operationCanceledException != null)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Operation cancelled", e));
                else
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Exception in downloader task whilst caching data", e));
                    throw;
                }
            }
        }

        private void DownloadUntilFinished(PermissionWindowCacheDownloader downloader, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var result = RetryMode ? 
                        downloader.RetryDownload(listener, cancellationToken) :
                        downloader.Download(listener, cancellationToken);

                    switch (result)
                    {
                        case RetrievalResult.NotPermitted:

                            if(TerminateIfOutsidePermissionWindow)
                            {
                                listener.OnNotify(this,
                                    new NotifyEventArgs(ProgressEventType.Information,
                                        "Download not permitted at this time so exitting"));

                                return;
                            }

                            listener.OnNotify(this,
                                new NotifyEventArgs(ProgressEventType.Information,
                                    "Download not permitted at this time, sleeping for 60 seconds"));

                            // Sleep for a while, but keep one eye open for cancellation requests
                            const int sleepTime = 60000;
                            const int cancellationCheckInterval = 1000;
                            var elapsedTime = 0;
                            while (elapsedTime < sleepTime)
                            {
                                Task.Delay(cancellationCheckInterval).Wait();
                                cancellationToken.ThrowIfCancellationRequested();
                                elapsedTime += cancellationCheckInterval;
                            }
                            break;
                        case RetrievalResult.Complete:
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Download completed successfully."));
                            return;
                        default:
                            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Download ended: " + result));
                            return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Cache download cancelled: " + downloader));
            }
        }
    }
}
