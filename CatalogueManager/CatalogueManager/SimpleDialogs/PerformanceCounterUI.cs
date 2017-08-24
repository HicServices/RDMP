using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using ReusableLibraryCode;
using ReusableLibraryCode.Performance;
using ReusableUIComponents;

using ReusableUIComponents.Performance;

namespace CatalogueManager.SimpleDialogs
{
    /// <summary>
    /// This form is mainly used for diagnostic purposes and lets you track every SQL query sent to the RDMP Data Catalogue and Data Export Manager databases.  This is useful for diagnosing
    /// the problem with sluggish user interfaces.  Once you select 'Start Command Auditing' it will record each unique SQL query sent to either database and the number of times it is sent
    /// including a StackTrace for the location in the RMDP software which the query was issued from.
    /// </summary>
    public partial class PerformanceCounterUI : Form
    {
        public PerformanceCounterUI()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DatabaseCommandHelper.PerformanceCounter == null)
            {

                lblCommandsAudited.Text = "Commands Audited:0";
                progressBar1.Value = 0;
                lblCacheUtilization.Text = "Cache Utilisation:";
            }
            else
            {
                int timesSeen = DatabaseCommandHelper.PerformanceCounter.DictionaryOfQueries.Seconds.Sum(s=>s.TimesSeen);

                lblCommandsAudited.Text = "Commands Audited:" + timesSeen + " (" + DatabaseCommandHelper.PerformanceCounter.DictionaryOfQueries.Firsts.Count + " distinct)";

                int hits = DatabaseCommandHelper.PerformanceCounter.CacheHits;
                int misses = DatabaseCommandHelper.PerformanceCounter.CacheMisses;

                progressBar1.Maximum = hits + misses;
                progressBar1.Value = hits;
                

                lblCacheUtilization.Text = "Cache Utilisation:" + hits + "/" + (hits + misses);
            }
        }

        private void btnToggleCommandAuditing_Click(object sender, EventArgs e)
        {

            if(DatabaseCommandHelper.PerformanceCounter == null)
            {
                DatabaseCommandHelper.PerformanceCounter = new ComprehensiveQueryPerformanceCounter();
                btnToggleCommandAuditing.Text = "Stop Command Auditing";
                btnViewPerformanceResults.Enabled = true;
            }
            else
            {
                DatabaseCommandHelper.PerformanceCounter = null;
                btnToggleCommandAuditing.Text = "Start Command Auditing";
                btnViewPerformanceResults.Enabled = false;
            }
            

        }
        
        private void CatalogueLibraryPerformanceCounterUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            //clear it before closing always
            DatabaseCommandHelper.PerformanceCounter = null;
        }

        private void btnViewPerformanceResults_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            var ui = new PerformanceCounterResultsUI();
            ui.Dock = DockStyle.Fill;

            //remove the current counter while this UI is running (the UI is designed to be a snapshot not a realtime view
            var performanceCounter = DatabaseCommandHelper.PerformanceCounter;
            DatabaseCommandHelper.PerformanceCounter = null;

            ui.LoadState(performanceCounter);
            f.WindowState = FormWindowState.Maximized;
            f.Controls.Add(ui);

            this.TopMost = false;
            f.ShowDialog();
            this.TopMost = true;

            //now the viewer has been closed we can reinstantiate the performance counter
            DatabaseCommandHelper.PerformanceCounter = performanceCounter;
        }
    }
}
