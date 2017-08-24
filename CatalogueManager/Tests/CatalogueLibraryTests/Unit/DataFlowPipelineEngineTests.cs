using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;

namespace CatalogueLibraryTests.Unit
{
    public class DataFlowPipelineEngineTests
    {
        private Task ExecutePipeline(DataFlowPipelineEngine<DataTable> engine, GracefulCancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => engine.ExecutePipeline(cancellationToken));
            
        }

        [Test]
        [Ignore("This test seems to block other tests from running properly")]
        [ExpectedException(typeof(OperationCanceledException))]
        public void TestExecutePipelineAsync()
        {
            var source = new BlockingSource();
            var context = MockRepository.GenerateMock<DataFlowPipelineContext<DataTable>>();
            var destination = MockRepository.GenerateMock<IDataFlowDestination<DataTable>>();
            var engine = new DataFlowPipelineEngine<DataTable>(context, source, destination,
                new ToConsoleDataLoadEventReceiver());
            
            engine.Initialize();
            var tokenSource = new CancellationTokenSource();
            
            var cancellationToken = new GracefulCancellationToken(tokenSource.Token, tokenSource.Token);

            var task = ExecutePipeline(engine, cancellationToken);

            // wait a bit until the pipeline has actually started (maybe add an event for this?)
            Thread.Sleep(500);

            tokenSource.Cancel();
            task.Wait(tokenSource.Token);
        }
    }

    internal class BlockingSource : IDataFlowSource<DataTable>
    {

        public BlockingSource()
        {
            HasBeenAborted = false;
        }

        public bool HasBeenAborted { get; private set; }

        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            while (true)
            {
                Thread.Sleep(100);

                cancellationToken.AbortToken.ThrowIfCancellationRequested();
            }
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public DataTable TryGetPreview()
        {
            throw new System.NotImplementedException();
        }
    }
}