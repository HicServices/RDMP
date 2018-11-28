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
        /// <summary>
        /// CancellationToken for stopping where convenient (e.g. at the end of a data load)
        /// </summary>
        public CancellationToken StopToken { get; private set; }

        /// <summary>
        /// CancellationToken for stopping as soon as possible
        /// </summary>
        public CancellationToken AbortToken { get; private set; }

        /// <summary>
        /// Creates a new <see cref="GracefulCancellationToken"/> that will never be cancelled
        /// </summary>
        public GracefulCancellationToken() : this(default(CancellationToken), default(CancellationToken))
        {
        }
         
        /// <summary>
        /// Creates a new <see cref="GracefulCancellationToken"/> using the provided stop and abort tokens.  You can pass
        /// the same CancellationToken for both parameters if you only want to support abort.
        /// </summary>
        /// <param name="stopToken"></param>
        /// <param name="abortToken"></param>
        public GracefulCancellationToken(CancellationToken stopToken, CancellationToken abortToken)
        {
            AbortToken = abortToken;
            StopToken = stopToken;
        }

        /// <summary>
        /// True if <see cref="AbortToken"/> has been set
        /// </summary>
        public bool IsAbortRequested 
        {
            get { return AbortToken.IsCancellationRequested; }
        }
        
        /// <summary>
        /// True if <see cref="StopToken"/> has been set
        /// </summary>
        public bool IsStopRequested
        {
            get { return StopToken.IsCancellationRequested; }
        }

        /// <summary>
        /// True if either <see cref="AbortToken"/> or <see cref="StopToken"/> has been set
        /// </summary>
        public bool IsCancellationRequested
        {
            get { return IsAbortRequested || IsStopRequested; }
        }

        /// <summary>
        /// Throws OperationCanceledException if either <see cref="AbortToken"/> or <see cref="StopToken"/> has been set
        /// </summary>
        public void ThrowIfCancellationRequested()
        {
            ThrowIfAbortRequested();
            ThrowIfStopRequested();
        }
        /// <summary>
        /// Throws OperationCanceledException if <see cref="AbortToken"/> has been set
        /// </summary>
        public void ThrowIfAbortRequested()
        {
            AbortToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Throws OperationCanceledException if <see cref="StopToken"/> has been set
        /// </summary>
        public void ThrowIfStopRequested()
        {
            StopToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Creates a single CancellationTokenSource which will be triggered if either <see cref="StopToken"/> or <see cref="AbortToken"/> is set
        /// </summary>
        /// <returns></returns>
        public CancellationTokenSource CreateLinkedSource()
        {
            return CancellationTokenSource.CreateLinkedTokenSource(StopToken, AbortToken);
        }
    }
}