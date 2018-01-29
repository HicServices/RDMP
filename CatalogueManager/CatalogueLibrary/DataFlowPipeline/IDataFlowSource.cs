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
        /// <summary>
        /// Called iteratively during pipeline execution, this method should return chunks of data T which will then be processed further down the pipeline.  Do not
        /// retain references to the T object you return or that can interfere with garbage collection.  When you are unable to yield any more T objects return null
        /// to indicate that there is no more data to source
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        T GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);

        /// <summary>
        /// Called after your pipeline has been fully executed (even if it resulted in a crash).  If the pipeline crashed then the Exception will be populated.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="pipelineFailureExceptionIfAny"></param>
        void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny);

        /// <summary>
        /// Invoked when the user (or program) attempts to cancel the pipeline execution.  This is used in addition to the GracefulCancellationToken since it can be called
        /// out of order (i.e. your component might not be currently executing at the abort time.
        /// </summary>
        /// <param name="listener"></param>
        void Abort(IDataLoadEventListener listener);


        /// <summary>
        /// This method is used at Design Time to help the user building a valid pipeline.  In theory the method should return a sample DataTable (or T) which is then 
        /// available programatically for checks and stuff.
        /// 
        /// In practice just returning null will work fine (it means no preview is available) or you can return an empty object with the compatible schema.
        /// </summary>
        /// <returns></returns>
        T TryGetPreview();
    }
    
    
}
