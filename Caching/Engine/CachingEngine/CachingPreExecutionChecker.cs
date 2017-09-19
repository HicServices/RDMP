using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CachingEngine
{
    /// <summary>
    /// Determines whether a given CacheProgress can be run.  This includes checking if there is a data time period to process, whether it is Locked, whether the classes required in the Pipeline
    /// can be constructed etc.
    /// </summary>
    public class CachingPreExecutionChecker : ICheckable
    {
        private readonly CacheProgress _cacheProgress;
        private CatalogueRepository _repository;


        public CachingPreExecutionChecker(CacheProgress cacheProgress)
        {
            _cacheProgress = cacheProgress;
            _repository = (CatalogueRepository) _cacheProgress.Repository;
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                if (_cacheProgress.Pipeline_ID == null)
                    throw new Exception("CacheProgress " + _cacheProgress.ID + " doesn't have a caching pipeline!");

                IPipeline pipeline = null;
                try
                {
                    pipeline = _cacheProgress.Pipeline;
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Error when trying to load Pipeline ID = " + _cacheProgress.Pipeline_ID.Value, CheckResult.Fail, e));
                }

                if (pipeline == null)
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not run Pipeline checks due to previous errors", CheckResult.Fail));
                else
                {
                    var checker = new PipelineChecker(pipeline);
                    checker.Check(notifier);
                }
                
                if (_cacheProgress.CacheFillProgress == null && _cacheProgress.LoadProgress.OriginDate == null)
                    //if we don't know what dates to request
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Both the CacheFillProgress and the LoadProgress.OriginDate are null, this means we don't know where the cache has filled up to and we don't know when the dataset is supposed to start.  This means it is impossible to know what dates to fetch",
                            CheckResult.Fail));

                if (_cacheProgress.PermissionWindow_ID != null &&
                    !_cacheProgress.PermissionWindow.TimeIsWithinPermissionWindow(DateTime.UtcNow))
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Current time is " + DateTime.UtcNow +
                            " which is not a permitted time according to the configured PermissionWindow of the CacheProgress",
                            CheckResult.Fail));

                var shortfall = _cacheProgress.GetShortfall();
                
                if (shortfall <= TimeSpan.Zero)
                    if (_cacheProgress.CacheLagPeriod == null)
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "CacheProgress reports that it has loaded up till " + _cacheProgress.CacheFillProgress +
                                " which is in the future.  So we don't need to load this cache.", CheckResult.Fail));
                    else
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "CacheProgress reports that it has loaded up till " + _cacheProgress.CacheFillProgress +
                                " but there is a lag period of " + _cacheProgress.CacheLagPeriod +
                                " which means we are not due to load any cached data yet.", CheckResult.Fail));


                var factory = new CachingPipelineEngineFactory(_cacheProgress);
                IDataFlowPipelineEngine engine = null;
                try
                {
                    var loadMetadata = _cacheProgress.LoadProgress.GetLoadMetadata();

                    engine = factory.CreateCachingPipelineEngine(_repository, new FromCheckNotifierToDataLoadEventListener(notifier));
                    engine.Initialize(new object[]
                    {
                        _repository.MEF,
                        new HICProjectDirectory(loadMetadata.LocationOfFlatFiles, false),
                        new PermissionWindow(),
                        new CacheFetchRequestProvider(new CacheFetchRequest(_repository, DateTime.Now))
                    });
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not create IDataFlowPipelineEngine", CheckResult.Fail, e));
                }

                if (engine == null)
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not run further PipelineEngine checks due to previous errors", CheckResult.Fail));
                else
                {
                    var source = engine.SourceObject as ICheckable;
                    if (source != null)
                        source.Check(notifier);
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not check Source as it doesn't implement ICheckable", CheckResult.Warning));


                    var destination = engine.DestinationObject as ICheckable;
                    if (destination != null)
                        destination.Check(notifier);
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not check Destination as it doesn't implement ICheckable", CheckResult.Warning));

                    foreach (var component in engine.ComponentObjects)
                    {
                        var checkable = component as ICheckable;
                        if (checkable != null)
                            checkable.Check(notifier);
                        else
                            notifier.OnCheckPerformed(new CheckEventArgs("Could not check Component as it doesn't implement ICheckable", CheckResult.Warning));
                    }
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Entire checking process for cache progress " + _cacheProgress +
                        " crashed, see Exception for details", CheckResult.Fail, e));
            }
        }
    }
}
