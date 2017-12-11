using System.Threading;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// Wrapper for two System.Threading.CancellationTokens.  One for Stopping (please stop when you are finished with the current job) and one for Aborting (please stop 
    /// as soon as possible).  If you like you can just specify the same token twice and nobody will be any the wiser. Remember that System.Threading.CancellationToken is
    /// for checking if cancellation is requested, in order to create and trigger cancellation you need a System.Threading.CancellationTokenSource handily you can use
    /// GracefulCancellationTokenSource to do that and then just reference .Token property.
    /// </summary>
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