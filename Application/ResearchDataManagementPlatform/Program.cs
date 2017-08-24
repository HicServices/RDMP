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
        static void Main()
        {
            RDMPBootStrapper<RDMPMainForm> bootStrapper = new RDMPBootStrapper<RDMPMainForm>();
            bootStrapper.Show(false);
        }
    }
}
