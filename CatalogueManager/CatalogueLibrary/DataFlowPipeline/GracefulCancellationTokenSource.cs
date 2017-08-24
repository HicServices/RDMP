using System;
using System.Threading;

namespace CatalogueLibrary.DataFlowPipeline
{
    public class GracefulCancellationTokenSource
    {
        private readonly CancellationTokenSource _stopTokenSource;
        private readonly CancellationTokenSource _abortTokenSource;
        public GracefulCancellationToken Token { get; private set; }

        public GracefulCancellationTokenSource()
        {
            _stopTokenSource = new CancellationTokenSource();
            _abortTokenSource = new CancellationTokenSource();
            Token = new GracefulCancellationToken(_stopTokenSource.Token, _abortTokenSource.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="AggregateException"></exception>
        public void Stop()
        {
            _stopTokenSource.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="AggregateException"></exception>
        public void Abort()
        {
            _abortTokenSource.Cancel();
        }
    }
}