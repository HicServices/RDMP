using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Reports;
using CatalogueManager.AggregationUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataQualityEngine;
using ReusableLibraryCode.Checks;

namespace CatalogueManager.SimpleDialogs.Reports
{
    /// <summary>
    /// This dialog is the preferred way of extracting per dataset documentation for users.  It will generate a report for each (or a single) dataset (Catalogue) including:
    /// 
    /// <para>- The dataset description</para>
    /// 
    /// <para>- Descriptions of all extractable columns / extraction transforms</para>
    /// 
    /// <para>- Counts of the number of records and unique patient identifiers (See ExtractionInformationUI and the IsExtractionIdentifier flag)</para>
    /// 
    /// <para>- Complete extract of the lookup tables configured for the dataset (See ConfigureLookups)</para>
    /// 
    /// <para>- Graphs of each IsExtractable aggregate on the dataset (See AggregateGraph)</para>
    /// 
    /// <para>You can untick any of the above options if desired.  If any aspect times out then you can either fix the underlying problem (maybe you need an index that helps an 
    /// Aggregate run faster) or just increase the Query Timeout (default is 30s).</para>
    /// </summary>
    public partial class ConfigureMetadataReport : RDMPForm
    {
        private readonly IActivateItems _activator;

        public ConfigureMetadataReport(IActivateItems activator)
        {
            _activator = activator;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;

            _extractableCatalogues = RepositoryLocator.CatalogueRepository.GetAllCataloguesWithAtLeastOneExtractableItem().ToArray();
            cbxCatalogues.Items.AddRange(_extractableCatalogues);
        }

        MetadataReport report;
        private Catalogue[] _extractableCatalogues;

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            if(rbAllCatalogues.Checked)
            {
                IEnumerable<Catalogue> toReportOn = _extractableCatalogues;

                if (!cbIncludeDeprecated.Checked)
                    toReportOn = toReportOn.Where(c => !c.IsDeprecated);

                if (!cbIncludeInternal.Checked)
                    toReportOn = toReportOn.Where(c => !c.IsInternalDataset && !c.IsColdStorageDataset);
                
                report = new MetadataReport(RepositoryLocator.CatalogueRepository,toReportOn,_timeout,cbIncludeRowCounts.Checked,cbIncludeDistinctIdentifiers.Checked,!cbIncludeGraphs.Checked,new DatasetTimespanCalculator());
            }
            else
                if (cbxCatalogues.SelectedItem == null)
                return;
            else
                    report = new MetadataReport(RepositoryLocator.CatalogueRepository, new[] { (Catalogue)cbxCatalogues.SelectedItem }, _timeout, cbIncludeRowCounts.Checked, cbIncludeDistinctIdentifiers.Checked, !cbIncludeGraphs.Checked, new DatasetTimespanCalculator());


            report.CatalogueCompleted+=report_CatalogueCompleted;
            report.RequestCatalogueImages += report_RequestCatalogueImages;
            report.MaxLookupRows = (int)nMaxLookupRows.Value;
            report.GenerateWordFileAsync(checksUI1);

            btnGenerateReport.Enabled = false;
            btnStop.Enabled = true;
        }

        public Bitmap[] report_RequestCatalogueImages(Catalogue catalogue)
        {
            //cross thread
            if (InvokeRequired)
                return (Bitmap[])Invoke(new RequestCatalogueImagesHandler(report_RequestCatalogueImages), catalogue);

            List<Bitmap> toReturn = new List<Bitmap>();
                
            
            aggregateGraph1.Width = (int) report.PageWidthInPixels;
            aggregateGraph1.Visible = true;

            bool firstTime = true;

            //only graph extractable aggregates
            foreach (AggregateConfiguration aggregate in catalogue.AggregateConfigurations.Where(config=>config.IsExtractable))
            {
                lblCurrentCatalogue.Text = "Calculating AggregateGraphConfiguration ID=" + aggregate.ID + ")...";

                if (firstTime)
                {
                    aggregateGraph1.SetDatabaseObject(_activator, aggregate);
                    firstTime = false;
                }
                else
                    aggregateGraph1.SetAggregate(aggregate);

                aggregateGraph1.LoadGraphAsync();
                
                while(aggregateGraph1.Done == false && aggregateGraph1.Crashed == false)
                {
                    Thread.Sleep(100);
                    Application.DoEvents();
                }

                if (aggregateGraph1.Crashed)
                {
                    checksUI1.OnCheckPerformed(new CheckEventArgs("Aggregate with ID " + aggregate.ID + " crashed", CheckResult.Fail,aggregateGraph1.Exception));
                    continue;
                }
                toReturn.AddRange(aggregateGraph1.GetImages());
            }

            aggregateGraph1.Visible = false;

            return toReturn.ToArray();

        }

        void report_CatalogueCompleted(int progress, int target, Catalogue currentCatalogue)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => report_CatalogueCompleted(progress, target, currentCatalogue)));
                return;
            }

            progressBar1.Value = progress;
            progressBar1.Maximum = target;
            lblCurrentCatalogue.Text = currentCatalogue.Name;
            lblCurrentCatalogue.Update();

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            report.Abort();
            aggregateGraph1.AbortLoadGraph();

            btnStop.Enabled = false;
            btnGenerateReport.Enabled = true;
        }

        private void ConfigureMetadataReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(report != null)
                report.Abort();

            aggregateGraph1.AbortLoadGraph();
        }

        private void rbAllCatalogues_CheckedChanged(object sender, EventArgs e)
        {
            cbxCatalogues.Enabled = false;

            cbIncludeDeprecated.Enabled = true;
            cbIncludeInternal.Enabled = true;
        }

        private void rbSpecificCatalogue_CheckedChanged(object sender, EventArgs e)
        {
            cbxCatalogues.Enabled = true;

            cbIncludeDeprecated.Enabled = false;
            cbIncludeInternal.Enabled = false;
        }


        private int _timeout = 30;
        private void tbTimeout_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _timeout = int.Parse(tbTimeout.Text);
                aggregateGraph1.Timeout = _timeout;
                tbTimeout.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbTimeout.ForeColor = Color.Red;
            }
        }

        private void cbIncludeDistinctIdentifiers_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
