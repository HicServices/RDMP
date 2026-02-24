// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Runtime.InteropServices;
using CommandLine;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core.Startup;
using Rdmp.UI;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup;

namespace ResearchDataManagementPlatform;

internal static partial class Program
{
    [LibraryImport("kernel32.dll")]
    private static partial void AttachConsole(int dwProcessId);

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            AttachConsole(-1);
        }
        catch (Exception)
        {
            Console.WriteLine("Couldn't redirect console. Never mind");
        }

        Startup.PreStartup();

        UsefulStuff.GetParser()
            .ParseArguments<ResearchDataManagementPlatformOptions>(args)
            .MapResult(RunApp, _ => -1);
    }

    private static object RunApp(ResearchDataManagementPlatformOptions arg)
    {
        try
        {
            arg.PopulateConnectionStringsFromYamlIfMissing(ThrowImmediatelyCheckNotifier.Quiet);
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
            return -500;
        }
        if(UserSettings.UseLocalFileSystem && !string.IsNullOrWhiteSpace(UserSettings.LocalFileSystemLocation))
        {
            arg.Dir = UserSettings.LocalFileSystemLocation;
        }


        var bootStrapper =
            new RDMPBootStrapper(arg, locator =>
            {
                var form = new RDMPMainForm();
                form.SetRepositoryLocator(locator);
                return form;
            });

        bootStrapper.Show();
        return 0;
    }
}