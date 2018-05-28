using System.Collections.Generic;
using CachingEngine.Locking;
using CachingEngine.PipelineExecution;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;

namespace CachingEngineTests.Unit
{
    public class PipelineExecutionTests
    {
        [Test]
        public void TestSerialPipelineExecution()
        {
            // set up two engines, one with a locked cache progress/load schedule
            // run the serial execution and ensure that only one engine had its 'ExecutePipeline' method called
            var engine1 = MockRepository.GenerateMock<IDataFlowPipelineEngine>();
            var engine2 = MockRepository.GenerateMock<IDataFlowPipelineEngine>();
            var tokenSource = new GracefulCancellationTokenSource();
            var listener = new ThrowImmediatelyDataLoadEventListener();

            // set up the engine map
            var loadProgress1 = MockRepository.GenerateStub<ILoadProgress>();
            var loadProgress2 = MockRepository.GenerateStub<ILoadProgress>();
            
            // set up the lock provider
            var engineMap = new Dictionary<IDataFlowPipelineEngine, ILoadProgress>
            {
                {engine1, loadProgress1},
                {engine2, loadProgress2}
            };
            var lockProvider = new LoadCacheEngineLockProvider(engineMap);

            // create the execution object
            var pipelineExecutor = new SerialPipelineExecution
            {
                EngineLockProvider = lockProvider
            };

            // Act
            pipelineExecutor.Execute(new [] {engine1, engine2}, tokenSource.Token, listener);

            // Assert
            // engine1 should have been executed once
            engine1.AssertWasCalled(engine => engine.ExecutePipeline(Arg<GracefulCancellationToken>.Is.Anything));
            
            // engine2 should also have been run (locking isn't a thing anymore)
            engine2.AssertWasCalled(engine => engine.ExecutePipeline(Arg<GracefulCancellationToken>.Is.Anything));
        }

        [Test]
        public void TestRoundRobinPipelineExecution()
        {
            // set up two engines, one with a locked cache progress/load schedule
            // run the serial execution and ensure that only one engine had its 'ExecutePipeline' method called
            var engine1 = MockRepository.GenerateMock<IDataFlowPipelineEngine>();
            var engine2 = MockRepository.GenerateMock<IDataFlowPipelineEngine>();
            var tokenSource = new GracefulCancellationTokenSource();
            var listener = new ThrowImmediatelyDataLoadEventListener();

            // first time both engines return that they have more data, second time they are both complete
            engine1.Stub(engine => engine.ExecuteSinglePass(Arg<GracefulCancellationToken>.Is.Anything)).Repeat.Once().Return(true);
            engine1.Stub(engine => engine.ExecuteSinglePass(Arg<GracefulCancellationToken>.Is.Anything)).Return(false);

            engine2.Stub(engine => engine.ExecuteSinglePass(Arg<GracefulCancellationToken>.Is.Anything)).Repeat.Once().Return(true);
            engine2.Stub(engine => engine.ExecuteSinglePass(Arg<GracefulCancellationToken>.Is.Anything)).Return(false);

            // set up the engine map
            var loadProgress1 = MockRepository.GenerateStub<ILoadProgress>();
            var loadProgress2 = MockRepository.GenerateStub<ILoadProgress>();
            
            // set up the lock provider
            var engineMap = new Dictionary<IDataFlowPipelineEngine, ILoadProgress>
            {
                {engine1, loadProgress1},
                {engine2, loadProgress2}
            };
            var lockProvider = new LoadCacheEngineLockProvider(engineMap);

            // create the execution object
            var pipelineExecutor = new RoundRobinPipelineExecution()
            {
                EngineLockProvider = lockProvider
            };

            // Act
            pipelineExecutor.Execute(new[] { engine1, engine2 }, tokenSource.Token, listener);

            // Assert
            // engine1 should have been executed once
            engine1.AssertWasCalled(engine => engine.ExecuteSinglePass(Arg<GracefulCancellationToken>.Is.Anything), options => options.Repeat.Times(2));

            // engine2 should not have been executed as it is locked
            engine2.AssertWasCalled(engine => engine.ExecuteSinglePass(Arg<GracefulCancellationToken>.Is.Anything), options => options.Repeat.Times(2));
        }
    }
}