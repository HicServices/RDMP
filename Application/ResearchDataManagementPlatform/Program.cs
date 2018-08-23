using System;
using CatalogueManager.TestsAndSetup;

namespace ResearchDataManagementPlatform
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            RDMPBootStrapper<RDMPMainForm> bootStrapper;
            if (args.Length == 2)
                bootStrapper = new RDMPBootStrapper<RDMPMainForm>(args[0], args[1]);
            else
                bootStrapper = new RDMPBootStrapper<RDMPMainForm>(null, null);

            bootStrapper.Show(false);
        }
    }
}
