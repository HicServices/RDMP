using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingEngine.PipelineExecution.Sources;
using CachingEngine.Requests;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowSources
{
    public class DoNothingCacheSource:CacheSource<ICacheChunk>
    {
        public override void DoGetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            //Data is never available for download
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
}
