using System;
using System.Threading;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    public interface IDataFlowSource<out T>
    {
        T GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);

        void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny);
        void Abort(IDataLoadEventListener listener);
        T TryGetPreview();
    }
    
    
}
