using System;
using System.Linq;
using System.Threading.Tasks;
using CachingEngine;
using CachingService.Properties;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;

namespace CachingService
{
    public class CachingServiceProvider
    {
        private readonly CatalogueRepository _repository;
        private GracefulCancellationTokenSource _cancellationTokenSource;
        public Task Task { get; private set; }

        public CachingServiceProvider(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public void Start(string[] args, IDataLoadEventListener listener)
        {
            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Starting the caching provider"));
            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Using database connection '" + _repository.ConnectionString));

            var settings = Settings.Default;

            // Now driven by list of permission windows, which are effectively lists of cache progresses that must be processed together
            var permissionWindowList =
                settings.PermissionWindowIDList.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Convert.ToInt32(s)) // get integer IDs from the string
                    .Select(id => _repository.GetObjectByID<PermissionWindow>(id)) // load the cache progress for each ID
                    .Cast<IPermissionWindow>()
                    .ToList();
            var cachingHost = new CachingHost( _repository);

            _cancellationTokenSource = new GracefulCancellationTokenSource();
            try
            {
                Task = Task.Run(() => cachingHost.CacheUsingPermissionWindows(permissionWindowList, listener, _cancellationTokenSource.Token));
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, ExceptionHelper.ExceptionToListOfInnerMessages(e, true), e));
            }
        }

        public void Stop(IDataLoadEventListener listener)
        {
            _cancellationTokenSource.Abort();

            try
            {
                Task.Wait();
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error when attempting to abort"));
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, ExceptionHelper.ExceptionToListOfInnerMessages(e, true), e));
            }            
        }
    }
}