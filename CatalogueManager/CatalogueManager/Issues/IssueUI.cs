using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Copying;
using ReusableLibraryCode;
using ReusableUIComponents;

using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;
using Color = System.Drawing.Color;

namespace CatalogueManager.Issues
{
    /// <summary>
    /// RDMP includes a basic issue tracking system for users who do not already have a more advanced purpose built issue tracking/bug reporting tool.  This control lets you edit
    /// an existing Issue.  Issues are associated with CatalogueItems (columns in your dataset) and are intended to document problems identified by data analysts or researchers 
    /// with the dataset.  For example you could have an issue 'It is not clear what the difference between 0 and null is in the 'PatientDeleted' of column 'Demographics' dataset.
    /// 
    /// <para>Issues have statuses (Outstanding / Blocked etc) and severity (Red/Amber/Green) as well as a Reporter and Owner.  You can document the issue and the Actions taken to try to 
    /// resolve it.  Finally you should try to write some SQL that will highlight the issue (e.g. Select PatientDeleted, count(*) from Demographics Group By PatientDeleted).  </para>
    /// 
    /// <para>You can also provide an Excel sheet as an attachment which goes into more detail.  </para>
    /// 
    /// <para>IMPORTANT: Be careful of your language in Description and Notes to Researcher fields because depending on settings Issues can be reported in the data extraction metadata 
    /// supplied to researchers who receive extracts of the dataset.</para>
    /// </summary>
    public partial class IssueUI : IssueUI_Design,ISaveableUI
    {
        private CatalogueItemIssue _catalogueItemIssue;
        private bool bloading;
        
        private Scintilla QueryPreview { get; set; }
        
        
        public IssueUI()
        {
            InitializeComponent();

            ddStatus.DataSource = Enum.GetValues(typeof(IssueStatus));
            ddSeverity.DataSource = Enum.GetValues(typeof(IssueSeverity));
            lbError.Text = "";

            #region Query Editor setup
            if (VisualStudioDesignMode)
                return;

            QueryPreview = new ScintillaTextEditorFactory().Create(new RDMPCommandFactory());
            QueryPreview.ReadOnly = false;
            QueryPreview.TextChanged += new EventHandler(QueryPreview_TextChanged);

            ticketingControl1.TicketTextChanged += ticketingControl1_TicketTextChanged;

            this.pSQL.Controls.Add(QueryPreview);
            #endregion


            AssociatedCollection = RDMPCollection.Catalogue;
            

        }

        void ticketingControl1_TicketTextChanged(object sender, EventArgs e)
        {
            if (bloading)
                return;

            if (_catalogueItemIssue != null)
                _catalogueItemIssue.Ticket = ticketingControl1.TicketText;
        }


        public override void SetDatabaseObject(IActivateItems activator, CatalogueItemIssue databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _catalogueItemIssue = databaseObject;

            bloading = true;

            if (QueryPreview == null)
                return;

            ticketingControl1.TicketText = _catalogueItemIssue.Ticket;
            QueryPreview.Text = _catalogueItemIssue.SQL;
            
            tbReportedOnDate.Text = _catalogueItemIssue.ReportedOnDate.ToString();
            tbOwner.Text = _catalogueItemIssue.Owner_ID != null ? _catalogueItemIssue.Owner.Name : "";
            tbReportedBy.Text = _catalogueItemIssue.ReportedBy_ID != null ? _catalogueItemIssue.ReportedBy.Name : "";

            tbDateCreated.Text = _catalogueItemIssue.DateCreated.ToString();
            tbDateOfLastStatusChange.Text = _catalogueItemIssue.DateOfLastStatusChange.ToString();
            tbUserWhoLastChangedStatus.Text = _catalogueItemIssue.UserWhoLastChangedStatus;

            bloading = false;
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, CatalogueItemIssue databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID,"Text","ID",i=>i.ID);
            Bind(tbName, "Text", "Name", i => i.Name);
            Bind(tbDescription, "Text", "Description", i => i.Description);
            
            Bind(ddStatus, "SelectedItem", "Status", i => i.Status);
            Bind(ddSeverity, "SelectedItem", "Severity", i => i.Severity);
            
            Bind(tbAction, "Text", "Action", i => i.Action);
            Bind(tbNotesToResearcher, "Text", "NotesToResearcher", i => i.NotesToResearcher);
            Bind(tbUserWhoCreated, "Text", "UserWhoCreated", i => i.UserWhoCreated);
            Bind(tbPathToExcelSheetWithAdditionalInformation, "Text", "PathToExcelSheetWithAdditionalInformation", i => i.PathToExcelSheetWithAdditionalInformation);
        }

        private void QueryPreview_TextChanged(object sender, EventArgs e)
        {
            if(bloading)
                return;

            _catalogueItemIssue.SQL = QueryPreview.Text;
        }

        private void ddStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(bloading)
                return;

            if(ddStatus.SelectedItem == null)
                return;

            _catalogueItemIssue.UserWhoLastChangedStatus = Environment.UserName;
            _catalogueItemIssue.DateOfLastStatusChange = Truncate(DateTime.Now, TimeSpan.FromMinutes(1));
        }
        
        private void tbReportedOnDate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _catalogueItemIssue.ReportedOnDate = DateTime.Parse(tbReportedOnDate.Text);

                tbReportedOnDate.ForeColor = Color.Black;
            }
            catch (FormatException)
            {
                tbReportedOnDate.ForeColor = Color.Red;
            }

        }

        private void btnChangeReportedBy_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIssueSystemUser();
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.ShowDialog();

            if (dialog.SelectedUser != null)
            {
                _catalogueItemIssue.ReportedBy_ID = dialog.SelectedUser.ID;
                tbReportedBy.Text = dialog.SelectedUser.Name;
            }
        }

        private void btnChangeOwner_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIssueSystemUser();
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.ShowDialog();

            if (dialog.SelectedUser != null)
            {
                _catalogueItemIssue.Owner_ID = dialog.SelectedUser.ID;
                tbOwner.Text = dialog.SelectedUser.Name;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Workbook (*.xls, *.xlsx)|*.xls;*.xlsx";

            if (ofd.ShowDialog(this) == DialogResult.OK)
                tbPathToExcelSheetWithAdditionalInformation.Text = ofd.FileName;
        }
        
        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            FileInfo f = new FileInfo(tbPathToExcelSheetWithAdditionalInformation.Text);

            try
            {
                UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(f.Directory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        

        public DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<IssueUI_Design, UserControl>))]
    public abstract class IssueUI_Design:RDMPSingleDatabaseObjectControl<CatalogueItemIssue>
    {
    }
}
