using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.TestsAndSetup;

namespace CachingDashboard
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            RDMPBootStrapper<CachingDashboardForm> bootStrapper = new RDMPBootStrapper<CachingDashboardForm>();
            bootStrapper.Show(false);
        }
    }
}
