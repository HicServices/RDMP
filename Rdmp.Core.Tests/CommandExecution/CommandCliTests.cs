// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    /// <summary>
    /// Base class for all tests which test RDMP CLI command line arguments to run <see cref="BasicCommandExecution"/> derrived
    /// classes
    /// </summary>
    public abstract class CommandCliTests : UnitTests
    {
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            SetupMEF();
        }

        protected CommandInvoker GetInvoker()
        {
            var invoker = new CommandInvoker(new ConsoleInputManager(RepositoryLocator,new ThrowImmediatelyCheckNotifier())
            {
                DisallowInput = true
            });
            invoker.CommandImpossible +=(s,c)=> throw new Exception(c.Command.ReasonCommandImpossible);

            return invoker;
        }
        
        protected Mock<IBasicActivateItems> GetMockActivator()
        {
            var mock = new Mock<IBasicActivateItems>();
            mock.Setup(m => m.RepositoryLocator).Returns(RepositoryLocator);
            mock.Setup(m => m.GetDelegates()).Returns(new List<CommandInvokerDelegate>());
            mock.Setup(m => m.Show(It.IsAny<string>()));
            return mock;
        }

        /// <summary>
        /// Runs the provided string which should start after the cmd e.g. the bit after rdmp cmd
        /// </summary>
        /// <param name="command">1 string per piece following rdmp cmd.  Element 0 should be the Type of command to run</param>
        /// <returns></returns>
        protected int Run(params string[] command)
        {
            var opts = new ExecuteCommandOptions();
            opts.CommandName = command[0];
            opts.CommandArgs = command.Skip(1).ToArray();
            
            var runner = new ExecuteCommandRunner(opts);
            return runner.Run(RepositoryLocator, new ThrowImmediatelyDataLoadEventListener(),
                new ThrowImmediatelyCheckNotifier(), new GracefulCancellationToken());
        }
    }
}