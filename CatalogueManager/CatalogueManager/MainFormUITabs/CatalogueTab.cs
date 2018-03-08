using System;
using System.CodeDom;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableUIComponents;


namespace CatalogueManager.MainFormUITabs
{
    /// <summary>
    /// Allows you to modify the descriptive data stored in the RDMP database about the selected Catalogue (dataset).  Pressing Ctrl+S will save any changes.  You should make sure that you
    /// provide as much background about your datasets as possible since this is the information that will be given to researchers when you extract the dataset (as well as being a great 
    /// reference for when you find a dataset and you're not quite sure about what it contains or how it got there or who supplied it etc).
    /// 
    /// The collection of fields for documentation were chosen by committee and based on the 'Dublin Core'.  Realistically though just entering all the information into 'Resource 
    /// Description' is probably a more realistic goal.  Documentation may be boring but it is absolutely vital for handling providence of research datasets especially if you frequently
    /// get given small datasets from researchers (e.g. questionnaire data they have collected) for use in cohort generation etc).
    /// 
    /// There is also a box for storing a ticket number, this will let you reference a ticket in your ticketing system (e.g. Jira, Fogbugz etc).  This requires selecting/writing a compatible
    /// plugin for your ticketing system and configuring it (see TicketingSystemConfigurationUI)
    /// </summary>
    public partial class CatalogueTab : CatalogueTab_Design, ISaveableUI
    {
        private bool _loadingUI;
        
        public CatalogueTab()
        {
            InitializeComponent();

            ticketingControl1.TicketTextChanged += ticketingControl1_TicketTextChanged;
        }

        public Catalogue Catalogue { get; private set; }
        
        void ticketingControl1_TicketTextChanged(object sender, EventArgs e)
        {
            if (Catalogue != null)
                Catalogue.Ticket = ticketingControl1.TicketText;
        }

        private void ClearFormComponents()
        {
            _loadingUI = true;

            try
            {
                foreach (Control c in splitContainer1.Panel1.Controls.Cast<Control>().Union(splitContainer1.Panel2.Controls.Cast<Control>()))
                    if (c is TextBox)
                        c.Text = "";
                    else if (c is ComboBox)
                        c.Text = "";
            }
            finally
            {
                _loadingUI = false;
            }
        }
        
        private void c_ddOverrideChildren_Click(object sender, EventArgs e)
        {
            if (Catalogue != null)
                if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    var catalogueItems = Catalogue.CatalogueItems.ToArray();
                    foreach (var catalogueItem in catalogueItems)
                    {
                        catalogueItem.Periodicity = Catalogue.Periodicity;
                        catalogueItem.Topic = Catalogue.Search_keywords;
                        catalogueItem.SaveToDatabase();
                    }
                }
        }
        
        #region Getters and setters


        private void tbDetailPageURL_TextChanged(object sender, EventArgs e)
        {
            SetUriPropertyOnCatalogue((TextBox)sender, "Detail_Page_URL");
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {

            string reasonInvalid;
            if (!Catalogue.IsAcceptableName(c_tbName.Text,out reasonInvalid))
            {
                lblReasonNameIsUnacceptable.Text = reasonInvalid;
                lblReasonNameIsUnacceptable.Visible = true;
                c_tbName.ForeColor = Color.Red;
                return;
            }
            
            c_tbName.ForeColor = Color.Black;
            lblReasonNameIsUnacceptable.Visible = false;

            SetStringPropertyOnCatalogue((TextBox)sender, "Name");
        }

        private void tbAcronym_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Acronym");
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Description");
        }

        private void tbCoverage_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Geographical_coverage");
        }

        private void tbNotes_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Background_summary");
        }

        private void tbTopics_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Search_keywords");
        }

        private void tbUpdateFrequency_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Update_freq");
        }

        private void tbUpdateSchedule_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Update_sched");
        }

        private void tbTimeCoverage_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Time_coverage");
        }
        private void tbDataStandards_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Data_standards");
        }

        private void tbAdministrativeContactName_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Administrative_contact_name");
        }

        private void tbAdministrativeContactEmail_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Administrative_contact_email");
        }

        private void tbAdministrativeContactTelephone_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Administrative_contact_telephone");
        }

        private void tbAdministrativeContactAddress_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Administrative_contact_address");
        }
        private void c_tbSubjectNumbers_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "SubjectNumbers");
        }
        private void ddExplicitConsent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;
            
            
            if (ddExplicitConsent.Text.Equals("Yes"))
                Catalogue.Explicit_consent = true;
                
            if (ddExplicitConsent.Text.Equals("No"))
                Catalogue.Explicit_consent = false;

            if (string.IsNullOrWhiteSpace(ddExplicitConsent.Text))
                Catalogue.Explicit_consent = null;

        }

        private void tbCountryOfOrigin_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Country_of_origin");
        }

        private void tbEthicsApprover_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Ethics_approver");
        }

        private void tbSourceOfDataCollection_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Source_of_data_collection");
        }
        private void c_tbResourceOwner_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Resource_owner");
        }

        private void tbAttributionCitation_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Attribution_citation");
        }

        private void tbAccessOptions_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogue((TextBox)sender, "Access_options");
        }

        private void tbAPIAccessURL_TextChanged(object sender, EventArgs e)
        {
            SetUriPropertyOnCatalogue((TextBox)sender, "API_access_URL");
        }

        private void tbBrowseUrl_TextChanged(object sender, EventArgs e)
        {
            SetUriPropertyOnCatalogue((TextBox)sender, "Browse_URL");
        }

        private void tbBulkDownloadUrl_TextChanged(object sender, EventArgs e)
        {
            SetUriPropertyOnCatalogue((TextBox)sender, "Bulk_Download_URL");
        }

        private void tbQueryToolUrl_TextChanged(object sender, EventArgs e)
        {
            SetUriPropertyOnCatalogue((TextBox)sender, "Query_tool_URL");
        }

        private void tbSourceUrl_TextChanged(object sender, EventArgs e)
        {
            SetUriPropertyOnCatalogue((TextBox)sender, "Source_URL");
        }
        private void tbLastRevisionDate_TextChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            TextBox senderAsTextBox = (TextBox)sender;
            try
            {
                if (Catalogue != null)
                {
                    DateTime answer = DateTime.Parse(senderAsTextBox.Text);

                    Catalogue.Last_revision_date = answer;
                    senderAsTextBox.ForeColor = Color.Black;
                }
            }
            catch (FormatException)
            {
                senderAsTextBox.ForeColor = Color.Red;
            }
        }

        private void ddType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            Catalogue.Type = (Catalogue.CatalogueType)c_ddType.SelectedItem;
        }

        private void ddPeriodicity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            Catalogue.Periodicity = (Catalogue.CataloguePeriodicity)c_ddPeriodicity.SelectedItem;

        }

        private void c_ddGranularity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            Catalogue.Granularity = (Catalogue.CatalogueGranularity)c_ddGranularity.SelectedItem;

        }
        #endregion

        private void SetUriPropertyOnCatalogue(TextBox tb, string property)
        {
            if (_loadingUI)
                return;
            
            if (Catalogue != null)
            {
                try
                {
                    Uri u = new Uri(tb.Text);

                    tb.ForeColor = Color.Black;

                    PropertyInfo target = Catalogue.GetType().GetProperty(property);
                    FieldInfo targetMaxLength = Catalogue.GetType().GetField(property + "_MaxLength");

                    if (target == null || targetMaxLength == null)
                        throw new Exception("Could not find property " + property + " or it did not have a specified _MaxLength");

                    if (tb.TextLength > (int)targetMaxLength.GetValue(Catalogue))
                        throw new UriFormatException("Uri is too long to fit in database");

                    target.SetValue(Catalogue, new Uri(tb.Text), null);
                    tb.ForeColor = Color.Black;

                }
                catch (UriFormatException)
                {
                    tb.ForeColor = Color.Red;
                }
            }
        }

        private void SetStringPropertyOnCatalogue(TextBox tb, string property)
        {
            if (_loadingUI)
                return;
            SetStringProperty(tb, property, Catalogue);
        }

        private void SetStringProperty(TextBox tb, string property, object toSetOn)
        {
            if (_loadingUI)
                return;

            if (toSetOn != null)
            {
                PropertyInfo target = toSetOn.GetType().GetProperty(property);
                FieldInfo targetMaxLength = toSetOn.GetType().GetField(property + "_MaxLength");


                if (target == null || targetMaxLength == null)
                    throw new Exception("Could not find property " + property + " or it did not have a specified _MaxLength");

                if (tb.TextLength > (int)targetMaxLength.GetValue(toSetOn))
                    tb.ForeColor = Color.Red;
                else
                {
                    target.SetValue(toSetOn, tb.Text, null);
                    tb.ForeColor = Color.Black;
                }
            }
        }

        
        private void tbDatasetStartDate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Catalogue == null)
                    return;

                if (string.IsNullOrWhiteSpace(tbDatasetStartDate.Text))
                {
                    Catalogue.DatasetStartDate = null;
                    return;
                }

                DateTime dateTime = DateTime.Parse(tbDatasetStartDate.Text);
                Catalogue.DatasetStartDate = dateTime;

                tbDatasetStartDate.ForeColor = Color.Black;
                
            }
            catch (Exception)
            {
                tbDatasetStartDate.ForeColor = Color.Red;
            }
        }
        
        private void tbFolder_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Catalogue.Folder = new CatalogueFolder(Catalogue, tbFolder.Text);
                tbFolder.ForeColor = Color.Black;
            }
            catch (Exception )
            {
                tbFolder.ForeColor = Color.Red;
            }
        }

        public void ClearIfNoLongerExists()
        {
            if (Catalogue != null && Catalogue.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted)
                Catalogue = null;
        }
        
        protected override void OnRepositoryLocatorAvailable()
        {
            base.OnRepositoryLocatorAvailable();

            RefreshUIFromDatabase();
        }

        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            Catalogue = databaseObject;
            
            objectSaverButton1.SetupFor(databaseObject,_activator.RefreshBus);
            RefreshUIFromDatabase();

        }

        private void RefreshUIFromDatabase()
        {
            if(RepositoryLocator == null)
                return;

            
            try
            {
                ticketingControl1.ReCheckTicketingSystemInCatalogue();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }

            _loadingUI = true;

            if (Catalogue == null)
            {
                ClearFormComponents();
                splitContainer1.Enabled = false;
                return;
            }

            splitContainer1.Enabled = true;
            ticketingControl1.Enabled = true;

            cbColdStorage.Checked = Catalogue.IsColdStorageDataset;
            cbDeprecated.Checked = Catalogue.IsDeprecated;
            cbInternal.Checked = Catalogue.IsInternalDataset;

            c_tbID.Text = Catalogue.ID.ToString();
            ticketingControl1.TicketText = Catalogue.Ticket;
            c_tbName.Text = Catalogue.Name;
            c_tbAcronym.Text = Catalogue.Acronym;
            c_tbDescription.Text = Catalogue.Description;
            c_tbDetailPageURL.Text = Catalogue.Detail_Page_URL != null ? Catalogue.Detail_Page_URL.ToString() : "";

            c_ddType.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueType));
            c_ddType.SelectedItem = Catalogue.Type;

            c_ddPeriodicity.DataSource = Enum.GetValues(typeof(Catalogue.CataloguePeriodicity));
            c_ddPeriodicity.SelectedItem = Catalogue.Periodicity;

            c_ddGranularity.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueGranularity));
            c_ddGranularity.SelectedItem = Catalogue.Granularity;

            c_tbGeographicalCoverage.Text = Catalogue.Geographical_coverage;
            c_tbBackgroundSummary.Text = Catalogue.Background_summary;
            c_tbTopics.Text = Catalogue.Search_keywords;
            c_tbUpdateFrequency.Text = Catalogue.Update_freq;
            c_tbUpdateSchedule.Text = Catalogue.Update_sched;
            c_tbTimeCoverage.Text = Catalogue.Time_coverage;
            c_tbLastRevisionDate.Text = Catalogue.Last_revision_date.ToString();
            tbAdministrativeContactName.Text = Catalogue.Contact_details;
            c_tbResourceOwner.Text = Catalogue.Resource_owner;
            c_tbAttributionCitation.Text = Catalogue.Attribution_citation;
            c_tbAccessOptions.Text = Catalogue.Access_options;

            c_tbAPIAccessURL.Text = Catalogue.API_access_URL != null ? Catalogue.API_access_URL.ToString() : "";
            c_tbBrowseUrl.Text = Catalogue.Browse_URL != null ? Catalogue.Browse_URL.ToString() : "";
            c_tbBulkDownloadUrl.Text = Catalogue.Bulk_Download_URL != null ? Catalogue.Bulk_Download_URL.ToString() : "";
            c_tbQueryToolUrl.Text = Catalogue.Query_tool_URL != null ? Catalogue.Query_tool_URL.ToString() : "";
            c_tbSourceUrl.Text = Catalogue.Source_URL != null ? Catalogue.Source_URL.ToString() : "";

            tbCountryOfOrigin.Text = Catalogue.Country_of_origin ?? "";
            tbDataStandards.Text = Catalogue.Data_standards ?? "";
            tbAdministrativeContactName.Text = Catalogue.Administrative_contact_name ?? "";
            tbAdministrativeContactEmail.Text = Catalogue.Administrative_contact_email ?? "";
            tbAdministrativeContactTelephone.Text = Catalogue.Administrative_contact_telephone ?? "";
            tbAdministrativeContactAddress.Text = Catalogue.Administrative_contact_address ?? "";
            tbEthicsApprover.Text = Catalogue.Ethics_approver ?? "";
            tbSourceOfDataCollection.Text = Catalogue.Source_of_data_collection ?? "";
            c_tbSubjectNumbers.Text = Catalogue.SubjectNumbers ?? "";

            tbFolder.Text = Catalogue.Folder.Path;

            if (Catalogue.Explicit_consent == null)
                ddExplicitConsent.Text = "";
            else if (Catalogue.Explicit_consent == true)
                ddExplicitConsent.Text = "Yes";
            else
                ddExplicitConsent.Text = "No";

            tbDatasetStartDate.Text = Catalogue.DatasetStartDate.ToString();

            _loadingUI = false;
        }

        private bool _expand = true;

        private void btnExpandOrCollapse_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !_expand;
            _expand = !_expand;
            btnExpandOrCollapse.Text = _expand ? "+" : "-";
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void cbFlag_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == cbDeprecated)
                Catalogue.IsDeprecated = cbDeprecated.Checked;
            if (sender == cbColdStorage)
                Catalogue.IsColdStorageDataset = cbColdStorage.Checked;
            if (sender == cbInternal)
                Catalogue.IsInternalDataset = cbInternal.Checked;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueTab_Design, UserControl>))]
    public abstract class CatalogueTab_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {

    }
}
