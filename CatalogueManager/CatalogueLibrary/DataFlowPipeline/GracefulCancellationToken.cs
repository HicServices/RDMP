using System.Threading;

namespace CatalogueLibrary.DataFlowPipeline
{
    public class GracefulCancellationToken
    {
        public CancellationToken StopToken { get; private set; }
        public CancellationToken AbortToken { get; private set; }

        public GracefulCancellationToken() : this(default(CancellationToken), default(CancellationToken))
        {
        }

        public GracefulCancellationToken(CancellationToken stopToken, CancellationToken abortToken)
        {
            AbortToken = abortToken;
            StopToken = stopToken;
        }

        public bool IsAbortRequested 
        {
            get { return AbortToken.IsCancellationRequested; }
        }

        public bool IsStopRequested
        {
            get { return StopToken.IsCancellationRequested; }
        }

        public bool IsCancellationRequested
        {
            get { return IsAbortRequested || IsStopRequested; }
        }

        public void ThrowIfCancellationRequested()
        {
            ThrowIfAbortRequested();
            ThrowIfStopRequested();
        }

        public void ThrowIfAbortRequested()
        {
            AbortToken.ThrowIfCancellationRequested();
        }

        public void ThrowIfStopRequested()
        {
            StopToken.ThrowIfCancellationRequested();
        }

        public CancellationTokenSource CreateLinkedSource()
        {
            return CancellationTokenSource.CreateLinkedTokenSource(StopToken, AbortToken);
        }
    }
}