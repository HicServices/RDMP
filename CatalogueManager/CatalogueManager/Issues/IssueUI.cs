using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode;
using ReusableUIComponents;

using ReusableUIComponents.ScintillaHelper;
using ReusableUIComponents.SingleControlForms;
using ScintillaNET;
using Color = System.Drawing.Color;

namespace CatalogueManager.Issues
{
    /// <summary>
    /// RDMP includes a basic issue tracking system for users who do not already have a more advanced purpose built issue tracking/bug reporting tool.  This control lets you edit
    /// an existing Issue.  Issues are associated with CatalogueItems (columns in your dataset) and are intended to document problems identified by data analysts or researchers 
    /// with the dataset.  For example you could have an issue 'It is not clear what the difference between 0 and null is in the 'PatientDeleted' of column 'Demographics' dataset.
    /// 
    /// Issues have statuses (Outstanding / Blocked etc) and severity (Red/Amber/Green) as well as a Reporter and Owner.  You can document the issue and the Actions taken to try to 
    /// resolve it.  Finally you should try to write some SQL that will highlight the issue (e.g. Select PatientDeleted, count(*) from Demographics Group By PatientDeleted).  
    /// 
    /// You can also provide an Excel sheet as an attachment which goes into more detail.  
    /// 
    /// IMPORTANT: Be careful of your language in Description and Notes to Researcher fields because depending on settings Issues can be reported in the data extraction metadata 
    /// supplied to researchers who receive extracts of the dataset.
    /// </summary>
    public partial class IssueUI : IssueUI_Design,ISaveableUI
    {
        private CatalogueItemIssue _catalogueItemIssue;
        private bool bloading;
        
        private Scintilla QueryPreview { get; set; }
        
        public CatalogueItemIssue CatalogueItemIssue
        {
            get { return _catalogueItemIssue; }
            set
            {
                bloading = true;
                
                if(QueryPreview == null)
                    return;
                
                _catalogueItemIssue = value;
                if (value == null)
                {
                    tbID.Text = "";

                    tbDescription.Text = "";
                    tbDescription.Enabled = false;

                    tbName.Text = "";
                    tbName.Enabled = false;

                    ticketingControl1.TicketText = "";
                    ticketingControl1.Enabled = false;

                    QueryPreview.Text = "";
                    QueryPreview.ReadOnly = true;

                    tbDescription.Text = "";
                    tbDescription.Enabled = false;
                    tbDescription.ForeColor = Color.Black;
                    lbError.Text = "";

                    ddStatus.SelectedItem = null;
                    ddStatus.Enabled = false;

                    ddSeverity.SelectedItem = null;
                    ddSeverity.Enabled = false;

                    tbAction.Text = "";
                    tbAction.Enabled = false;

                    tbReportedOnDate.Text = "";
                    tbReportedOnDate.Enabled = false;

                    tbNotesToResearcher.Text = "";
                    tbNotesToResearcher.Enabled = false;

                    tbOwner.Text =  "";
                    tbReportedBy.Text = "";

                    tbDateCreated.Text = "";
                    tbDateOfLastStatusChange.Text = "";
                    tbUserWhoCreated.Text = "";
                    tbUserWhoLastChangedStatus.Text = "";

                    tbPathToExcelSheetWithAdditionalInformation.Text = "";
                    tbPathToExcelSheetWithAdditionalInformation.Enabled = false;
                    btnBrowse.Enabled = false;
                    btnOpenFile.Enabled = false;
                    btnOpenFolder.Enabled = false;


                }
                else
                {
                    tbID.Text = value.ID.ToString();

                    tbDescription.Text =  value.Description;
                    tbDescription.Enabled = true;

                    tbName.Text = value.Name;
                    tbName.Enabled = true;

                    ticketingControl1.TicketText = value.Ticket;
                    ticketingControl1.Enabled = true;

                    QueryPreview.ReadOnly = false;
                    QueryPreview.Text = value.SQL;

                    tbDescription.Text = value.Description;
                    tbDescription.Enabled = true;
                    tbDescription.ForeColor = Color.Black;
                    lbError.Text = "";

                    ddStatus.SelectedItem = value.Status;
                    ddStatus.Enabled = true;

                    ddSeverity.SelectedItem = value.Severity;
                    ddSeverity.Enabled = true;

                    tbAction.Text = value.Action;
                    tbAction.Enabled = true;

                    tbReportedOnDate.Text = value.ReportedOnDate.ToString();
                    tbReportedOnDate.Enabled = true;

                    tbNotesToResearcher.Text = value.NotesToResearcher;
                    tbNotesToResearcher.Enabled = true;

                    tbOwner.Text = value.Owner_ID != null ? value.Owner.Name : "";
                    tbReportedBy.Text = value.ReportedBy_ID != null ? value.ReportedBy.Name : "";
                    
                    tbDateCreated.Text = value.DateCreated.ToString();
                    tbDateOfLastStatusChange.Text = value.DateOfLastStatusChange.ToString();
                    tbUserWhoCreated.Text = value.UserWhoCreated;
                    tbUserWhoLastChangedStatus.Text = value.UserWhoLastChangedStatus;


                    tbPathToExcelSheetWithAdditionalInformation.Text = value.PathToExcelSheetWithAdditionalInformation;
                    tbPathToExcelSheetWithAdditionalInformation.Enabled = true;
                    btnBrowse.Enabled = true;
                    btnOpenFile.Enabled = true;
                    btnOpenFolder.Enabled = true;
                }

                bloading = false;
            }
        }
        
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

            if (CatalogueItemIssue != null)
                CatalogueItemIssue.Ticket = ticketingControl1.TicketText;
        }


        public override void SetDatabaseObject(IActivateItems activator, CatalogueItemIssue databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            CatalogueItemIssue = databaseObject;
            objectSaverButton1.SetupFor(databaseObject,activator.RefreshBus);
        }

        protected override bool ProcessKeyPreview(ref Message m)
        {

            PreviewKey p = new PreviewKey(ref m, ModifierKeys);

            if (p.IsKeyDownMessage && p.e.KeyCode == Keys.S && p.e.Control)
            {
                btnSave_Click(null, null);
                p.Trap(this);
            }

            return base.ProcessKeyPreview(ref m);
        }

        private void QueryPreview_TextChanged(object sender, EventArgs e)
        {
            if(bloading)
                return;

            if (CatalogueItemIssue != null)
                CatalogueItemIssue.SQL = QueryPreview.Text;
        }


        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
              if(bloading)
                return;

            if (CatalogueItemIssue != null)
            {
                try
                {
                    CatalogueItemIssue.Description = tbDescription.Text;
                    tbDescription.ForeColor = Color.Black;
                    lbError.Text = "";
                }
                catch (Exception exception)
                {
                    lbError.Text = ExceptionHelper.ExceptionToListOfInnerMessages(exception);
                    tbDescription.ForeColor = Color.Red;
                }
            }
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        { 
            if(bloading)
                return;

            if (string.IsNullOrWhiteSpace(tbName.Text))
            {

                tbName.Text = "No Name";
                tbName.SelectAll();
            }

            CatalogueItemIssue.Name = tbName.Text;
            

        }


        private void tbNotesToResearcher_TextChanged(object sender, EventArgs e)
        {
            if (bloading)
                return;

            if (CatalogueItemIssue != null)
                CatalogueItemIssue.NotesToResearcher = tbNotesToResearcher.Text;
        }

        private void tbAction_TextChanged(object sender, EventArgs e)
        {
            if (bloading)
                return;

            if (CatalogueItemIssue != null)
                CatalogueItemIssue.Action = tbAction.Text;
        }

        private void ddSeverity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (bloading)
                return;

            if (CatalogueItemIssue != null && ddSeverity.SelectedItem is IssueSeverity)
                CatalogueItemIssue.Severity = (IssueSeverity) ddSeverity.SelectedItem;
        }
        
        private void ddStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(bloading)
                return;

            if(ddStatus.SelectedItem == null)
                return;

            if (CatalogueItemIssue != null)
            {
                CatalogueItemIssue.Status = (IssueStatus) ddStatus.SelectedItem;
                CatalogueItemIssue.UserWhoLastChangedStatus = Environment.UserName;
                CatalogueItemIssue.DateOfLastStatusChange = Truncate(DateTime.Now,TimeSpan.FromMinutes(1));
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(CatalogueItemIssue != null)
            {
                CatalogueItemIssue.SaveToDatabase();
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(CatalogueItemIssue));
            }
        }

        private void tbReportedOnDate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (CatalogueItemIssue != null)
                    CatalogueItemIssue.ReportedOnDate = DateTime.Parse(tbReportedOnDate.Text);

                tbReportedOnDate.ForeColor = Color.Black;
            }
            catch (FormatException)
            {
                tbReportedOnDate.ForeColor = Color.Red;
            }

        }

        private void btnChangeReportedBy_Click(object sender, EventArgs e)
        {
            if (bloading || CatalogueItemIssue == null)
                return;

            var dialog = new SelectIssueSystemUser();
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.ShowDialog();

            if (dialog.SelectedUser != null)
            {
                CatalogueItemIssue.ReportedBy_ID = dialog.SelectedUser.ID;
                tbReportedBy.Text = dialog.SelectedUser.Name;
            }
        }

        private void btnChangeOwner_Click(object sender, EventArgs e)
        {
            if(bloading || CatalogueItemIssue == null)
                return;


            var dialog = new SelectIssueSystemUser();
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.ShowDialog();

            if (dialog.SelectedUser != null)
            {
                CatalogueItemIssue.Owner_ID = dialog.SelectedUser.ID;
                tbOwner.Text = dialog.SelectedUser.Name;
            }

        }

        private void tbPathToExcelSheetWithAdditionalInformation_TextChanged(object sender, EventArgs e)
        {
            if (CatalogueItemIssue != null)
            {
                CatalogueItemIssue.PathToExcelSheetWithAdditionalInformation =
                    tbPathToExcelSheetWithAdditionalInformation.Text;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Workbook (*.xls, *.xlsx)|*.xls;*.xlsx";

            if (ofd.ShowDialog(this) == DialogResult.OK)
                tbPathToExcelSheetWithAdditionalInformation.Text = ofd.FileName;


        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(tbPathToExcelSheetWithAdditionalInformation.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            FileInfo f = new FileInfo(tbPathToExcelSheetWithAdditionalInformation.Text);

            try
            {
                Process.Start(f.DirectoryName);

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

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<IssueUI_Design, UserControl>))]
    public abstract class IssueUI_Design:RDMPSingleDatabaseObjectControl<CatalogueItemIssue>
    {
    }
}
