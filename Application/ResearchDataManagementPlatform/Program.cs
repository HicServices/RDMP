// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Runtime.InteropServices;
using CommandLine;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Startup;
using Rdmp.UI;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup;

namespace ResearchDataManagementPlatform
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AttachConsole([MarshalAs(UnmanagedType.U4)] int dwProcessId);
  
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // if user has the command line built and runnable from the windows
            // client then don't load the dlls (or we end up with 2 copies!).
            SafeDirectoryCatalog.IgnoreDll = (f) => Path.GetFileName(f.DirectoryName)?.Equals("cli")==true;

            try
            {
                AttachConsole(-1);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't redirect console. Nevermind");
            }

            Startup.PreStartup();

            UsefulStuff.GetParser()
                       .ParseArguments<ResearchDataManagementPlatformOptions>(args)
                       .MapResult(RunApp, err => -1);
        }

        private static object RunApp(ResearchDataManagementPlatformOptions arg)
        {
            try
            {
                arg.PopulateConnectionStringsFromYamlIfMissing(new ThrowImmediatelyCheckNotifier());
            }
            catch(Exception ex)
            {
                ExceptionViewer.Show(ex);
                return -500;
            }

            RDMPBootStrapper<RDMPMainForm> bootStrapper =
                new RDMPBootStrapper<RDMPMainForm>(
                    new EnvironmentInfo(PluginFolders.Main | PluginFolders.Windows),
                    arg);

            bootStrapper.Show(false);
            return 0;
        }
    }
}
