// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NLog;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui;

internal class ConsoleGuiRunner : IRunner
{
    private readonly ConsoleGuiOptions options;
    private ConsoleGuiActivator _activator;

    public ConsoleGuiRunner(ConsoleGuiOptions options)
    {
        this.options = options;
    }

    public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token, int? dataLoadId)
    {
        Program.DisableConsoleLogging();

        if (options.UseSystemConsole) Application.UseSystemConsole = true;

        Application.Init();

        try
        {
            _activator = new ConsoleGuiActivator(repositoryLocator, checkNotifier);
            ConsoleMainWindow.StaticActivator = _activator;

            var top = Application.Top;

            // Creates the top-level window to show
            var win = new ConsoleMainWindow(_activator);
            win.SetUp(top);
        }
        catch (Exception e)
        {
            LogManager.GetCurrentClassLogger().Error(e, "Failed to startup application");
            Application.Shutdown();
            return -2;
        }

        try
        {
            Application.Run();
        }
        catch (Exception e)
        {
            LogManager.GetCurrentClassLogger().Error(e, "Application Crashed");
            Application.Top.Running = false;
            return -1;
        }
        finally
        {
            Application.Shutdown();
        }

        return 0;
    }
}