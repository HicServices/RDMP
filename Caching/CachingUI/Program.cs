using System;
using CatalogueManager.TestsAndSetup;

namespace CachingUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var bootStrapper = new RDMPBootStrapper<CachingEngineUI>();
            bootStrapper.Show(false);
        }
    }
}