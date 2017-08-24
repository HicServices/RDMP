using System;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    public interface IDataFlowComponent<T>
    {
        T ProcessPipelineData(T toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken);
        void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny);
        void Abort(IDataLoadEventListener listener);
    }
}