using System;
using System.Threading;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// First component in an IDataFlowPipelineEngine, responsible for producing objects of type T via GetChunk until there are no new Ts available (e.g. reading from a
    /// csv file 50,000 lines at a time and generating System.Data.Table(s) until the file is exhausted).
    /// 
    /// Where possible, you should implement IPluginDataFlowSource instead of this class so that you are MEF discoverable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataFlowSource<out T>
    {
        T GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);

        void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny);
        void Abort(IDataLoadEventListener listener);
        T TryGetPreview();
    }
    
    
}
