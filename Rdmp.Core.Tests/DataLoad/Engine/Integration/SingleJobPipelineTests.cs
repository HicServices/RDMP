// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Moq;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration
{
    public class SingleJobPipelineTests : DatabaseTests
    {
        public void LoadNotRequiredStopsPipelineGracefully()
        {
            var component = new NotRequiredComponent();

            var pipeline = new SingleJobExecution(new List<IDataLoadComponent> {component});

            var job = Mock.Of<IDataLoadJob>();
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
