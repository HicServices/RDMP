using System;
using System.Threading;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// Source for creating a GracefulCancellationToken.  See GracefulCancellationToken for description of this two level Cancellation strategy.
    /// </summary>
    public class GracefulCancellationTokenSource
    {
        private readonly CancellationTokenSource _stopTokenSource;
        private readonly CancellationTokenSource _abortTokenSource;

        /// <summary>
        /// The object for checking whether stop / abort have been triggered
        /// </summary>
        public GracefulCancellationToken Token { get; private set; }

        /// <summary>
        /// Creates a new source for issuing a <see cref="GracefulCancellationToken"/> and triggering stop/abort later on.
        /// </summary>
        public GracefulCancellationTokenSource()
        {
            _stopTokenSource = new CancellationTokenSource();
            _abortTokenSource = new CancellationTokenSource();
            Token = new GracefulCancellationToken(_stopTokenSource.Token, _abortTokenSource.Token);
        }

        /// <summary>
        /// Triggers the stop flag of <see cref="Token"/> (<see cref="GracefulCancellationToken.StopToken"/>
        /// </summary>
        public void Stop()
        {
            _stopTokenSource.Cancel();
        }

        /// <summary>
        /// Triggers the abort flag of <see cref="Token"/> (<see cref="GracefulCancellationToken.AbortToken"/>
        /// </summary>
        public void Abort()
        {
            _abortTokenSource.Cancel();
        }
    }
}
