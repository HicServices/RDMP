// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NSubstitute;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

/// <summary>
/// Base class for all tests which test RDMP CLI command line arguments to run <see cref="BasicCommandExecution"/> derived
/// classes
/// </summary>
public abstract class CommandCliTests : UnitTests
{
    protected CommandInvoker GetInvoker()
    {
        var invoker = new CommandInvoker(new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
        {
            DisallowInput = true
        });
        invoker.CommandImpossible += static (s, c) => throw new Exception(c.Command.ReasonCommandImpossible);

        return invoker;
    }

    private readonly Lazy<IBasicActivateItems> _mockActivator;

    protected CommandCliTests()
    {
        _mockActivator = new Lazy<IBasicActivateItems>(MakeMockActivator, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private IBasicActivateItems MakeMockActivator()
    {
        var mock = Substitute.For<IBasicActivateItems>();
        mock.RepositoryLocator.Returns(RepositoryLocator);
        mock.GetDelegates().Returns(new List<CommandInvokerDelegate>());
        return mock;
    }

    protected IBasicActivateItems GetMockActivator() => _mockActivator.Value;

    /// <summary>
    /// Runs the provided string which should start after the cmd e.g. the bit after rdmp cmd
    /// </summary>
    /// <param name="command">1 string per piece following rdmp cmd.  Element 0 should be the Type of command to run</param>
    /// <returns></returns>
    protected int Run(params string[] command)
    {
        var opts = new ExecuteCommandOptions
        {
            CommandName = command[0],
            CommandArgs = command.Skip(1).ToArray()
        };

        var runner = new ExecuteCommandRunner(opts);
        return runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet,
            ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken());
    }
}