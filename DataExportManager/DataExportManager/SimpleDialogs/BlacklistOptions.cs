using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataExportManager.SimpleDialogs
{
    /// <summary>
    /// Allows you to recover from problems accessing cohorts (e.g. if a data export cohort is unavailable to the current user or misconfigured).  The options for recover include ignoring
    /// the error, stopping sending requests to the cohort endpoint, stopping sending ANY cohort requests and finally you can tell RDMP to disable all Data Export functionality for you.
    /// </summary>
    public partial class BlacklistOptions : Form
    {
        public BlacklistOptions(string problem)
        {
            InitializeComponent();
            lblProblem.Text = problem;
        }

        public BlacklistResponse Response = BlacklistResponse.Ignore;

        private void btnUnsetDataExportRepositoryLocation_Click(object sender, EventArgs e)
        {
            CloseWith(BlacklistResponse.Ignore);
        }

        private void btnBlacklistAllSources_Click(object sender, EventArgs e)
        {
            CloseWith(BlacklistResponse.BlacklistAll);
        }

        private void btnBlacklistSource_Click(object sender, EventArgs e)
        {
            CloseWith(BlacklistResponse.BlacklistSource);
        }

        private void btnIgnoreError_Click(object sender, EventArgs e)
        {
            CloseWith(BlacklistResponse.Ignore);

        }

        private void CloseWith(BlacklistResponse response)
        {
            Response = response;
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    public enum BlacklistResponse
    {
        Ignore,
        BlacklistSource,
        BlacklistAll,
        UnsetDataExport
    }
}
