using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using CatalogueManager.TestsAndSetup.ServicePropogation;

namespace CatalogueManager.SimpleDialogs.Reports
{
    /// <summary>
    /// Allows you to generate a report of how big in MB and records each database in your live data repository is.  The tool will evaluate every database on each server for which you
    /// have a TableInfo (See TableInfoTab).  The report will include sizes/row counts of all databases/tables on these servers (not just those managed by the RDMP).
    /// </summary>
    public partial class DatabaseSizeReportUI : RDMPForm
    {
        public DatabaseSizeReportUI()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var repo = RepositoryLocator.CatalogueRepository;

            throw new Exception("Comming soon to a world near you");
            //checksUI1.StartChecking(new DatabaseSizeReport(repo.GetAllObjects<TableInfo>().ToArray(),repo));
        }
    }
}
