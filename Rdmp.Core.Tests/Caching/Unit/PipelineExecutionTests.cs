// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.Caching.Unit;

[Category("Unit")]
public class PipelineExecutionTests
{
    [Ignore("Tests locking we don't actually have")]
    public static void TestSerialPipelineExecution()
    {
        // set SetUp two engines, one with a locked cache progress/load schedule
        // run the serial execution and ensure that only one engine had its 'ExecutePipeline' method called
        var engine1 = Substitute.For<IDataFlowPipelineEngine>();


        var engine2 = Substitute.For<IDataFlowPipelineEngine>();

        var tokenSource = new GracefulCancellationTokenSource();
        var listener = new ThrowImmediatelyDataLoadEventListener();

        // set SetUp the engine map
        // set SetUp the lock provider

        // create the execution object
        var pipelineExecutor = new SerialPipelineExecution();

        // Act
        pipelineExecutor.Execute(new[] { engine1.Object, engine2.Object }, tokenSource.Token, listener);

        // engine1 should have been executed once
        engine1.Received(1).ExecutePipeline(Arg.Any<GracefulCancellationToken>());

        // engine2 should also have been run (locking isn't a thing any more)
        engine2.Received(1).ExecutePipeline(Arg.Any<GracefulCancellationToken>());
    }

    [Ignore("Tests locking we don't actually have")]
    public static void TestRoundRobinPipelineExecution()
    {
        // set SetUp two engines, one with a locked cache progress/load schedule
        // run the serial execution and ensure that only one engine had its 'ExecutePipeline' method called
        var engine1 = Substitute.For<IDataFlowPipelineEngine>();
        var engine2 = Substitute.For<IDataFlowPipelineEngine>();
        var tokenSource = new GracefulCancellationTokenSource();
        var listener = new ThrowImmediatelyDataLoadEventListener();

        // first time both engines return that they have more data, second time they are both complete
        engine1.SetupSequence(engine => engine.ExecuteSinglePass(Arg.Any<GracefulCancellationToken>()))
            .Returns(true)
            .Returns(false)
            .Throws<InvalidOperationException>();

        engine2.SetupSequence(engine => engine.ExecuteSinglePass(Arg.Any<GracefulCancellationToken>()))
            .Returns(true)
            .Returns(false)
            .Throws<InvalidOperationException>();

        // create the execution object
        var pipelineExecutor = new RoundRobinPipelineExecution();

        // Act
        pipelineExecutor.Execute(new[] { engine1.Object, engine2.Object }, tokenSource.Token, listener);

        // Assert
        // engine1 should have been executed once
        engine1.Verify();

        // engine2 should not have been executed as it is locked, but we don't actually have locks.
        engine1.Verify();
    }
}