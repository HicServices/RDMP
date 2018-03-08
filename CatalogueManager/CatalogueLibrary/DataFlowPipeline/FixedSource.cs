using System;
using System.Linq;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// Use this every time you need a Fixed Source that has no pipeline initialization requirements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedSource<T> : ICheckable, IDataFlowSource<T> where T : class, new()
    {
        private readonly Action<ICheckNotifier> checkAction;
        private readonly T flowData;
        private bool firstTime = true;
        
        public FixedSource(Action<ICheckNotifier> checkAction = null, T flowData = null)
        {
            this.checkAction = checkAction ?? (cn => { });
            this.flowData = flowData;
        }

        public T GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            // TODO: Use an injectable strategy maybe?
            if (firstTime)
            {
                firstTime = false;
                return flowData ?? new T();
            }

            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            firstTime = true;
        }

        public void Abort(IDataLoadEventListener listener)
        {
            firstTime = true;
        }

        public T TryGetPreview()
        {
            return null;
        }

        public void Check(ICheckNotifier notifier)
        {
            checkAction(notifier);
        }
    }
}
