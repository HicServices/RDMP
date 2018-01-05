using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingEngine.BasicCache;
using CachingEngine.Layouts;
using CachingEngine.PipelineExecution.Destinations;
using CachingEngine.Requests;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowDestinations
{
    /// <summary>
    /// Cache destination component which does nothing.  Can be used by user to build a caching pipeline even when there is nothing to do.  Basically wraps
    /// BasicCacheLayout so that you can read from the cache even though you have no valid pipeline for writing to it.  Use this destination only if you have
    /// some bespoke process for populating / updating the cache progress and you only want a caching pipeline to exist for validation reasons not to actually
    /// run it.
    /// </summary>
    public class DoNothingCacheDestination : CacheFilesystemDestination
    {
        public override ICacheChunk ProcessPipelineData(ICacheChunk toProcess, IDataLoadEventListener listener,GracefulCancellationToken cancellationToken)
        {
            if(toProcess != null)
                throw new NotSupportedException("Expected only to be passed null chunks or never to get called, this destination is not valid for use when sources are actually sending/reading data");

            return null;
        }

        public override ICacheLayout CreateCacheLayout()
        {
            return new BasicCacheLayout(CacheDirectory);
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            
        }
    }
}
