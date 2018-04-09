using System;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// A single component in an IDataFlowPipelineEngine T.  The component should do a single operation on the flowing data (e.g. if T is a System.Data.DataTable the component
    /// could delete duplicate rows) then return the results of the operation via ProcessPipelineData.
    /// 
    /// <para>Where possible, you should implement IPluginDataFlowComponent instead of this class so that you are MEF discoverable</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataFlowComponent<T>
    {
        /// <summary>
        /// Contains the code that will be executed to modify the T object passing through the component.  E.g. ColumnRenamer component would take each DataTable (T is a 
        /// DataTable for this example) and rename the column it is configured for (at Design Time).  This method will be called once for each T served by the IDataFlowSource.
        /// 
        /// <para>Do not keep references to toProcess since it will interfere with garbage collection.</para>
        /// </summary>
        /// <param name="toProcess"></param>
        /// <param name="listener"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        T ProcessPipelineData(T toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);
        
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
    }
}
