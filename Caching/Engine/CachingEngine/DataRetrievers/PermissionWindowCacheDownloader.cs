using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CachingEngine.Factories;
using CachingEngine.Locking;
using CachingEngine.PipelineExecution;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CachingEngine.DataRetrievers
{
    /// <summary>
    /// Responsible for executing the caching pipeline(s) for a given PermissionWindow and/or set of CacheProgress items.
    /// If a PermissionWindow is set, either all CacheProgresses attached to that window can be downloaded (by calling Download) or a subset may be specified (by calling overloaded Download).
    /// If no PermissionWindow is set, either all CacheProgresses which have no PermissionWindow can be downloaded (by calling Download) or a subset may be specified (by calling overloaded Download).
    /// </summary>
    public class PermissionWindowCacheDownloader
    {
        private readonly IPermissionWindow _permissionWindow;
        private readonly List<ICacheProgress> _cacheProgressItems;
        private readonly IMultiPipelineEngineExecutionStrategy _pipelineEngineExecutionStrategy;
        private IDataLoadEventListener _listener;
        private RetrievalResult _retrievalResult;
        
        private Dictionary<IDataFlowPipelineEngine, ILoadProgress> _engineMap;
        private readonly ICatalogueRepository _repository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionWindow">May be null, then allows downloading of CacheProgress with no associated PermissionWindow</param>
        /// <param name="dynamicPipelineEngineFactory"></param>
        /// <param name="repository"></param>
        /// <param name="pipelineEngineExecutionStrategy">Multiple CacheProgresses will result in multiple PipelineEngines, this strategy determines how these should be run, e.g. Round-robin, Async</param>
        public PermissionWindowCacheDownloader(IPermissionWindow permissionWindow, ICatalogueRepository repository, IMultiPipelineEngineExecutionStrategy pipelineEngineExecutionStrategy)
        {
            _permissionWindow = permissionWindow;
            _cacheProgressItems = _permissionWindow.GetAllCacheProgresses().ToList();
            _repository = repository;
            _pipelineEngineExecutionStrategy = pipelineEngineExecutionStrategy;
        }

        /// <summary>
        /// Overload with specific cache items to download for this permission window
        /// </summary>
        /// <param name="permissionWindow"></param>
        /// <param name="cacheProgressItems"></param>
        /// <param name="dynamicPipelineEngineFactory"></param>
        /// <param name="repository"></param>
        /// <param name="pipelineEngineExecutionStrategy"></param>
        public PermissionWindowCacheDownloader(IPermissionWindow permissionWindow, List<ICacheProgress> cacheProgressItems, ICatalogueRepository repository, IMultiPipelineEngineExecutionStrategy pipelineEngineExecutionStrategy)
        {
            _permissionWindow = permissionWindow;

            CheckCacheProgressesUseCorrectPermissionWindow(cacheProgressItems);
            _cacheProgressItems = cacheProgressItems;

            _repository = repository;
            _pipelineEngineExecutionStrategy = pipelineEngineExecutionStrategy;
        }

        private bool IsDownloadPermitted()
        {
            //if there isn't a permission window then you are good to go
            if (_permissionWindow == null)
                return true;

            //return true only if the permission window is not locked and the time is currently within the window
            return
                !_permissionWindow.LockedBecauseRunning && _permissionWindow.CurrentlyWithinPermissionWindow();
        }

        /// <summary>
        /// Single-shot, will either exit immediately if not in the permission window or run until either:
        /// - the permission window expires, or 
        /// - all engines successfully complete execution.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public RetrievalResult Download(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            _listener = listener;
            
            return IsDownloadRequired(listener) ?
                RunPipelineExecutionTask(cancellationToken, CreateCachingEngine) :
                _retrievalResult;
        }

        public RetrievalResult RetryDownload(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            _listener = listener;
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Retrying download: " + (_permissionWindow == null ? "No permission window" : _permissionWindow.Name)));

            return IsDownloadRequired(listener) ? 
                RunPipelineExecutionTask(cancellationToken, CreateRetryCachingEngine) :
                _retrievalResult;
        }

        private bool IsDownloadRequired(IDataLoadEventListener listener)
        {
            if (!IsDownloadPermitted())
            {
                _retrievalResult = RetrievalResult.NotPermitted;
                return false;
            }

            if (_cacheProgressItems == null)
            {
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "No cache progress provided so nothing to download."));
                _retrievalResult = RetrievalResult.Complete;
                return false;
            }
            
            return true;
        }

        private RetrievalResult RunPipelineExecutionTask(GracefulCancellationToken cancellationToken, Func<ICacheProgress, IDataFlowPipelineEngine> createEngineFunc)
        {
            _engineMap = new Dictionary<IDataFlowPipelineEngine, ILoadProgress>();
            
            // CreateCachingEngine also populates the engineMap as it knows about both the load schedule and pipeline engine at the same time
            var cachingEngines = _cacheProgressItems.Select(createEngineFunc).ToList();

            // This particular lock provider uses the LoadProgress object to determine whether a particular engine is locked (the pipeline engine itself has no concept of locking, but the execution strategy does)
            var engineLockProvider = new LoadCacheEngineLockProvider(_engineMap);
            _pipelineEngineExecutionStrategy.EngineLockProvider = engineLockProvider;

            return RunOnce(cancellationToken, cachingEngines);
        }

        /// <summary>
        /// Blocking call which runs a set of DataFlowPipelineEngines according to the execution strategy whilst observing the PermissionWindow.
        /// Stops the task and returns if the PermissionWindow closes.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="cachingEngines"></param>
        /// <returns></returns>
        private RetrievalResult RunOnce(GracefulCancellationToken cancellationToken, List<IDataFlowPipelineEngine> cachingEngines)
        {
            Lock();

            // We will be spawning our own task which we want separate control of (to kill if we pass outside the permission window), so need our own cancellation token
            var executionCancellationTokenSource = new GracefulCancellationTokenSource();

            // We want to be able to stop the engine if we pass outside the permission window, however the execution strategy objects should not know about PermissionWindows
            var executionTask = new Task(() => 
                _pipelineEngineExecutionStrategy.Execute(cachingEngines, executionCancellationTokenSource.Token, _listener));
            
            // Block waiting on task completion or signalling of the cancellation token
            while (!executionTask.IsCompleted)
            {
                if (executionTask.Status == TaskStatus.Created)
                    executionTask.Start();

                Task.Delay(1000).Wait();

                // We need to handle stop and abort as we have used our own cancellation token with the child task
                // If someone above us in the process chain has requested abort or cancel then use our own cancellation token to pass this info on to the child task
                if (cancellationToken.IsAbortRequested)
                {
                    // Wait nicely until the child task signals its abort token (by throwing?)
                    executionCancellationTokenSource.Abort();
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Have issued Abort request to the Pipeline Execution Task. Waiting for the task to exit..."));
                    try
                    {
                        executionTask.Wait();
                    }
                    catch (AggregateException)
                    {
                        
                    }
                    Unlock();
                    return RetrievalResult.Aborted;
                }

                if (cancellationToken.IsStopRequested)
                {
                    // Wait nicely until the child task signals its stop token (by throwing?)
                    executionCancellationTokenSource.Stop();
                    _listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information,
                            "Have issued Stop request to the Pipeline Execution Task, however this may take some to complete as it will attempt to complete the current run through the pipeline."));
                    executionTask.Wait();
                    Unlock();
                    return RetrievalResult.Stopped;
                }

                // Now can check to see if we are finished (we have passed outside the permission window)
                if (_permissionWindow != null && !_permissionWindow.CurrentlyWithinPermissionWindow())
                {
                    executionCancellationTokenSource.Abort();
                    _listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information,
                            "Now outside the PermissionWindow, have issued Abort request to the Pipeline Execution Task."));
                    executionTask.Wait();
                    Unlock();
                    return RetrievalResult.NotPermitted;
                }
            }

            if (executionTask.IsFaulted)
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Task faulted, information is in the attached exception.", executionTask.Exception));
                Unlock();
                throw new InvalidOperationException("Task faulted, see inner exception for details.", executionTask.Exception);
            }

            Unlock();
            return RetrievalResult.Complete;
        }

        public void Lock()
        {
            if (_permissionWindow != null)
                _permissionWindow.Lock();
        }

        public void Unlock()
        {
            if (_permissionWindow != null)
                _permissionWindow.Unlock();
        }

        private IDataFlowPipelineEngine CreateCachingEngine(ICacheProgress cacheProgress)
        {
            var cachingPipelineEngineFactory = new CachingPipelineEngineFactory();
            var engine = cachingPipelineEngineFactory.CreateCachingPipelineEngine(cacheProgress, _repository, _listener);
            _engineMap.Add(engine, cacheProgress.GetLoadProgress());
            return engine;
        }

        private IDataFlowPipelineEngine CreateRetryCachingEngine(ICacheProgress cacheProgress)
        {
            var cachingPipelineEngineFactory = new CachingPipelineEngineFactory();
            var engine = cachingPipelineEngineFactory.CreateRetryCachingPipelineEngine(cacheProgress, _repository, _listener);
            _engineMap.Add(engine, cacheProgress.GetLoadProgress());
            return engine;
        }

        private void CheckCacheProgressesUseCorrectPermissionWindow(IEnumerable<ICacheProgress> cacheProgressList)
        {
            foreach (var cacheProgress in cacheProgressList)
            {
                if (_permissionWindow == null)
                {
                    if (cacheProgress.PermissionWindow_ID != null)
                        throw new Exception("Configuration error: This downloader has been configured for all CacheProgress items which have no PermissionWindow, but this CacheProgress (ID=" + cacheProgress.ID + ") does have a PermissionWindow.");
                }
                else if (cacheProgress.PermissionWindow_ID != _permissionWindow.ID)
                {
                    var progressPermissionWindow = cacheProgress.GetPermissionWindow();
                    throw new Exception("Configuration error, CacheProgress with ID=" + cacheProgress.ID +
                                        " does not have its permissions specified by Permission Window '" +
                                        _permissionWindow.Name +
                                        "' (ID=" + _permissionWindow.ID + "). It uses Permission Window '" +
                                        progressPermissionWindow.Name + "' (ID=" + progressPermissionWindow.ID + ")");
                }

            }
        }

        public override string ToString()
        {
            return "Downloader for " + _permissionWindow.Name;
        }
    }
}