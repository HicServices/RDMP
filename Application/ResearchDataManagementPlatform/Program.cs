using System;
using System.Runtime.InteropServices;
using CatalogueManager.TestsAndSetup;
using CommandLine;
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

            UsefulStuff.GetParser().ParseArguments<ResearchDataManagementPlatformOptions>(args).MapResult(RunApp,err=>-1);
        }

        private static object RunApp(ResearchDataManagementPlatformOptions arg)
        {
            RDMPBootStrapper<RDMPMainForm>.Boostrap(arg.CatalogueConnectionString, arg.DataExportConnectionString, requiresDataExportDatabaseToo: false);
            return 0;
        }
    }
}
