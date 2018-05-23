using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingEngine.PipelineExecution.Sources;
using CachingEngine.Requests;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowSources
{
    /// <summary>
    /// Cache source component which does nothing.  Can be used by user to build a caching pipeline even when there is nothing to do.  Use this source only if
    /// you have some bespoke process for populating / updating the cache progress and you only want a caching pipeline to exist for validation reasons not to
    /// actually run it.
    /// </summary>
    public class DoNothingCacheSource:CacheSource<ICacheChunk>
    {
        private int runs;

        public override void DoGetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            //Data is never available for download
            if (runs < 10)
            {
                runs++;
                Chunk = new DoNothingCacheChunk(CatalogueRepository)
                {
                    RunIteration = runs
                };
                return;
            }
            Chunk = null;
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            
        }

        public override ICacheChunk TryGetPreview()
        {
            return null;
        }

        public override void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "This Cache Source will never find new data available since it is there only for testing purposes and so you can set up a valid Caching pipeline configuration even if it doesn't do anything (e.g. for use in a hacky manner with DoNothingCacheDestination)",
                    CheckResult.Warning));
        }
    }

    public class DoNothingCacheChunk : ICacheChunk
    {
        public DoNothingCacheChunk(ICatalogueRepository catalogueRepository)
        {
            Request = new CacheFetchRequest(catalogueRepository);
        }

        public int RunIteration { get; set; }
        public ICacheFetchRequest Request { get; private set; }
    }
}
