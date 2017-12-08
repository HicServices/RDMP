using System;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// A single component in an IDataFlowPipelineEngine T.  The component should do a single operation on the flowing data (e.g. if T is a System.Data.DataTable the component
    /// could delete duplicate rows) then return the results of the operation via ProcessPipelineData.
    /// 
    /// Where possible, you should implement IPluginDataFlowComponent instead of this class so that you are MEF discoverable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataFlowComponent<T>
    {
        T ProcessPipelineData(T toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);
        void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny);
        void Abort(IDataLoadEventListener listener);
    }
}