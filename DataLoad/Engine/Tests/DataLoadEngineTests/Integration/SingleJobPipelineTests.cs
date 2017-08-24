using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadExecution.Components;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class SingleJobPipelineTests : DatabaseTests
    {
        public void LoadNotRequiredStopsPipelineGracefully()
        {
            var component = new NotRequiredComponent();

            var pipeline = new SingleJobExecution(new List<DataLoadComponent> {component});

            var job = MockRepository.GenerateStub<IDataLoadJob>();
            var jobTokenSource = new GracefulCancellationTokenSource();
            pipeline.Run(job, jobTokenSource.Token);
        }
    }

    internal class NotRequiredComponent : DataLoadComponent
    {
        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            return ExitCodeType.OperationNotRequired;
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postDataLoadEventListener)
        {
        }
    }

}
