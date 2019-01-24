using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;


namespace CatalogueManager.MainFormUITabs
{
    /// <summary>
    /// Allows you to modify the descriptive data stored in the RDMP database about the selected Catalogue (dataset).  Pressing Ctrl+S will save any changes.  You should make sure that you
    /// provide as much background about your datasets as possible since this is the information that will be given to researchers when you extract the dataset (as well as being a great 
    /// reference for when you find a dataset and you're not quite sure about what it contains or how it got there or who supplied it etc).
    /// 
    /// <para>The collection of fields for documentation were chosen by committee and based on the 'Dublin Core'.  Realistically though just entering all the information into 'Resource 
    /// Description' is probably a more realistic goal.  Documentation may be boring but it is absolutely vital for handling providence of research datasets especially if you frequently
    /// get given small datasets from researchers (e.g. questionnaire data they have collected) for use in cohort generation etc).</para>
    /// 
    /// <para>There is also a box for storing a ticket number, this will let you reference a ticket in your ticketing system (e.g. Jira, Fogbugz etc).  This requires selecting/writing a compatible
    /// plugin for your ticketing system and configuring it (see TicketingSystemConfigurationUI)</para>
    /// </summary>
    public partial class CatalogueTab : CatalogueTab_Design, ISaveableUI
    {
        private bool _loadingUI;
        
        public CatalogueTab()
        {
            InitializeComponent();

            ticketingControl1.TicketTextChanged += ticketingControl1_TicketTextChanged;
            
            AssociatedCollection = RDMPCollection.Catalogue;
        }

        private Catalogue _catalogue;
        
        void ticketingControl1_TicketTextChanged(object sender, EventArgs e)
        {
            if (_catalogue != null)
                _catalogue.Ticket = ticketingControl1.TicketText;
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
            if (_catalogue != null)
                if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    var catalogueItems = _catalogue.CatalogueItems.ToArray();
                    foreach (var catalogueItem in catalogueItems)
                    {
                        catalogueItem.Periodicity = _catalogue.Periodicity;
                        catalogueItem.Topic = _catalogue.Search_keywords;
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
                _catalogue.Explicit_consent = true;
                
            if (ddExplicitConsent.Text.Equals("No"))
                _catalogue.Explicit_consent = false;

            if (string.IsNullOrWhiteSpace(ddExplicitConsent.Text))
                _catalogue.Explicit_consent = null;

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
                if (_catalogue != null)
                {
                    DateTime answer = DateTime.Parse(senderAsTextBox.Text);

                    _catalogue.Last_revision_date = answer;
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

            _catalogue.Type = (Catalogue.CatalogueType)c_ddType.SelectedItem;
        }

        private void ddPeriodicity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            _catalogue.Periodicity = (Catalogue.CataloguePeriodicity)c_ddPeriodicity.SelectedItem;

        }

        private void c_ddGranularity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            _catalogue.Granularity = (Catalogue.CatalogueGranularity)c_ddGranularity.SelectedItem;

        }
        #endregion

        private void SetUriPropertyOnCatalogue(TextBox tb, string property)
        {
            if (_loadingUI)
                return;
            
            if (_catalogue != null)
            {
                try
                {
                    Uri u = new Uri(tb.Text);

                    tb.ForeColor = Color.Black;

                    PropertyInfo target = _catalogue.GetType().GetProperty(property);
                    FieldInfo targetMaxLength = _catalogue.GetType().GetField(property + "_MaxLength");

                    if (target == null || targetMaxLength == null)
                        throw new Exception("Could not find property " + property + " or it did not have a specified _MaxLength");

                    if (tb.TextLength > (int)targetMaxLength.GetValue(_catalogue))
                        throw new UriFormatException("Uri is too long to fit in database");

                    target.SetValue(_catalogue, new Uri(tb.Text), null);
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
            SetStringProperty(tb, property, _catalogue);
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
                if (_catalogue == null)
                    return;

                if (string.IsNullOrWhiteSpace(tbDatasetStartDate.Text))
                {
                    _catalogue.DatasetStartDate = null;
                    return;
                }

                DateTime dateTime = DateTime.Parse(tbDatasetStartDate.Text);
                _catalogue.DatasetStartDate = dateTime;

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
                _catalogue.Folder = new CatalogueFolder(_catalogue, tbFolder.Text);
                tbFolder.ForeColor = Color.Black;
            }
            catch (Exception )
            {
                tbFolder.ForeColor = Color.Red;
            }
        }

        public void ClearIfNoLongerExists()
        {
            if (_catalogue != null && _catalogue.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted)
                _catalogue = null;
        }
        
        protected override void OnRepositoryLocatorAvailable()
        {
            base.OnRepositoryLocatorAvailable();

            RefreshUIFromDatabase();
        }

        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _catalogue = databaseObject;
            
            objectSaverButton1.SetupFor(databaseObject,_activator.RefreshBus);
            RefreshUIFromDatabase();
        }

        protected override void SetRules(RuleBasedErrorProvider rules, Catalogue databaseObject)
        {
            base.SetRules(rules,databaseObject);

            rules.EnsureAcronymUnique(c_tbAcronym, databaseObject);
            rules.EnsureNameUnique(c_tbName, databaseObject);

            c_tbAcronym.DataBindings.Clear();
            c_tbAcronym.DataBindings.Add("Text", databaseObject, "Acronym",false,DataSourceUpdateMode.OnPropertyChanged);
        }

        private void RefreshUIFromDatabase()
        {
            try
            {
                ticketingControl1.ReCheckTicketingSystemInCatalogue();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }

            _loadingUI = true;

            if (_catalogue == null)
            {
                ClearFormComponents();
                splitContainer1.Enabled = false;
                return;
            }

            splitContainer1.Enabled = true;
            ticketingControl1.Enabled = true;

            cbColdStorage.Checked = _catalogue.IsColdStorageDataset;
            cbDeprecated.Checked = _catalogue.IsDeprecated;
            cbInternal.Checked = _catalogue.IsInternalDataset;

            c_tbID.Text = _catalogue.ID.ToString();
            ticketingControl1.TicketText = _catalogue.Ticket;
            c_tbName.Text = _catalogue.Name;
            c_tbDescription.Text = _catalogue.Description;
            c_tbDetailPageURL.Text = _catalogue.Detail_Page_URL != null ? _catalogue.Detail_Page_URL.ToString() : "";

            c_ddType.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueType));
            c_ddType.SelectedItem = _catalogue.Type;

            c_ddPeriodicity.DataSource = Enum.GetValues(typeof(Catalogue.CataloguePeriodicity));
            c_ddPeriodicity.SelectedItem = _catalogue.Periodicity;

            c_ddGranularity.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueGranularity));
            c_ddGranularity.SelectedItem = _catalogue.Granularity;

            c_tbGeographicalCoverage.Text = _catalogue.Geographical_coverage;
            c_tbBackgroundSummary.Text = _catalogue.Background_summary;
            c_tbTopics.Text = _catalogue.Search_keywords;
            c_tbUpdateFrequency.Text = _catalogue.Update_freq;
            c_tbUpdateSchedule.Text = _catalogue.Update_sched;
            c_tbTimeCoverage.Text = _catalogue.Time_coverage;
            c_tbLastRevisionDate.Text = _catalogue.Last_revision_date.ToString();
            tbAdministrativeContactName.Text = _catalogue.Contact_details;
            c_tbResourceOwner.Text = _catalogue.Resource_owner;
            c_tbAttributionCitation.Text = _catalogue.Attribution_citation;
            c_tbAccessOptions.Text = _catalogue.Access_options;

            c_tbAPIAccessURL.Text = _catalogue.API_access_URL != null ? _catalogue.API_access_URL.ToString() : "";
            c_tbBrowseUrl.Text = _catalogue.Browse_URL != null ? _catalogue.Browse_URL.ToString() : "";
            c_tbBulkDownloadUrl.Text = _catalogue.Bulk_Download_URL != null ? _catalogue.Bulk_Download_URL.ToString() : "";
            c_tbQueryToolUrl.Text = _catalogue.Query_tool_URL != null ? _catalogue.Query_tool_URL.ToString() : "";
            c_tbSourceUrl.Text = _catalogue.Source_URL != null ? _catalogue.Source_URL.ToString() : "";

            tbCountryOfOrigin.Text = _catalogue.Country_of_origin ?? "";
            tbDataStandards.Text = _catalogue.Data_standards ?? "";
            tbAdministrativeContactName.Text = _catalogue.Administrative_contact_name ?? "";
            tbAdministrativeContactEmail.Text = _catalogue.Administrative_contact_email ?? "";
            tbAdministrativeContactTelephone.Text = _catalogue.Administrative_contact_telephone ?? "";
            tbAdministrativeContactAddress.Text = _catalogue.Administrative_contact_address ?? "";
            tbEthicsApprover.Text = _catalogue.Ethics_approver ?? "";
            tbSourceOfDataCollection.Text = _catalogue.Source_of_data_collection ?? "";
            c_tbSubjectNumbers.Text = _catalogue.SubjectNumbers ?? "";

            tbFolder.Text = _catalogue.Folder.Path;

            if (_catalogue.Explicit_consent == null)
                ddExplicitConsent.Text = "";
            else if (_catalogue.Explicit_consent == true)
                ddExplicitConsent.Text = "Yes";
            else
                ddExplicitConsent.Text = "No";

            tbDatasetStartDate.Text = _catalogue.DatasetStartDate.ToString();

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
                _catalogue.IsDeprecated = cbDeprecated.Checked;
            if (sender == cbColdStorage)
                _catalogue.IsColdStorageDataset = cbColdStorage.Checked;
            if (sender == cbInternal)
                _catalogue.IsInternalDataset = cbInternal.Checked;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueTab_Design, UserControl>))]
    public abstract class CatalogueTab_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
        
    }
}
