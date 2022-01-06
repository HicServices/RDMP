// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CommandLine;
using Rdmp.Core.Startup;
using Rdmp.UI;
using Rdmp.UI.TestsAndSetup;
using ReusableLibraryCode;

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
            try
            {
                AttachConsole(-1);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't redirect console. Nevermind");
            }

            UsefulStuff.GetParser()
                       .ParseArguments<ResearchDataManagementPlatformOptions>(args.Except(new[] { "--squirrel-firstrun" }))
                       .MapResult(RunApp, err => -1);
        }

        private static object RunApp(ResearchDataManagementPlatformOptions arg)
        {
            arg.PopulateConnectionStringsFromYamlIfMissing();

            RDMPBootStrapper<RDMPMainForm> bootStrapper =
                new RDMPBootStrapper<RDMPMainForm>(
                    new EnvironmentInfo(PluginFolders.Main | PluginFolders.Windows),
                    arg);

            bootStrapper.Show(false);
            return 0;
        }
    }
}
