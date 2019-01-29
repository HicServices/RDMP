using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
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
        public CatalogueTab()
        {
            InitializeComponent();

            ticketingControl1.TicketTextChanged += ticketingControl1_TicketTextChanged;
            
            AssociatedCollection = RDMPCollection.Catalogue;

            c_ddType.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueType));
            c_ddPeriodicity.DataSource = Enum.GetValues(typeof(Catalogue.CataloguePeriodicity));
            c_ddGranularity.DataSource = Enum.GetValues(typeof(Catalogue.CatalogueGranularity));
        }

        private Catalogue _catalogue;
        
        void ticketingControl1_TicketTextChanged(object sender, EventArgs e)
        {
            if (_catalogue != null)
                _catalogue.Ticket = ticketingControl1.TicketText;
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
        
        private void tbName_TextChanged(object sender, EventArgs e)
        {
            string reasonInvalid;
            if (!Catalogue.IsAcceptableName(c_tbName.Text, out reasonInvalid))
                errorProvider1.SetError(c_tbName, reasonInvalid);
            else
                errorProvider1.Clear();
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
        
        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _catalogue = databaseObject;
            
            RefreshUIFromDatabase();
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, Catalogue databaseObject)
        {
            base.SetBindings(rules,databaseObject);

            Bind(c_tbAcronym, "Text", "Acronym", c=>c.Acronym);
            Bind(c_tbName, "Text", "Name", c => c.Name);
            Bind(c_tbID,"Text","ID", c=>c.ID);
            Bind(c_tbDescription,"Text","Description",c=>c.Description);
            
            Bind(c_ddType,"SelectedItem","Type",c=>c.Type);
            Bind(c_ddGranularity,"SelectedItem","Granularity", c => c.Granularity);
            Bind(c_ddPeriodicity, "SelectedItem", "Periodicity", c => c.Periodicity);
            Bind(ddExplicitConsent, "SelectedItem", "Explicit_consent", c => c.Explicit_consent);

            Bind(cbColdStorage, "Checked", "IsColdStorageDataset", c => c.IsColdStorageDataset);
            Bind(cbDeprecated, "Checked", "IsDeprecated", c => c.IsDeprecated);
            Bind(cbInternal, "Checked", "IsInternalDataset", c => c.IsInternalDataset);

            Bind(c_tbGeographicalCoverage ,"Text","Geographical_coverage", c=>c.Geographical_coverage);
            Bind(c_tbBackgroundSummary ,"Text","Background_summary", c=>c.Background_summary);
            Bind(c_tbTopics ,"Text","Search_keywords", c=>c.Search_keywords);
            Bind(c_tbUpdateFrequency ,"Text","Update_freq", c=>c.Update_freq);
            Bind(c_tbUpdateSchedule ,"Text","Update_sched", c=>c.Update_sched);
            Bind(c_tbTimeCoverage ,"Text","Time_coverage", c=>c.Time_coverage);
            Bind(tbAdministrativeContactName ,"Text","Contact_details", c=>c.Contact_details);
            Bind(c_tbResourceOwner ,"Text","Resource_owner", c=>c.Resource_owner);
            Bind(c_tbAttributionCitation ,"Text","Attribution_citation", c=>c.Attribution_citation);
            Bind(c_tbAccessOptions ,"Text","Access_options", c=>c.Access_options);

            Bind(tbDataStandards,"Text", "Data_standards",c=>c.Data_standards);
            Bind(tbAdministrativeContactName,"Text", "Administrative_contact_name",c=>c.Administrative_contact_name);
            Bind(tbAdministrativeContactEmail,"Text", "Administrative_contact_email",c=>c.Administrative_contact_email);
            Bind(tbAdministrativeContactTelephone,"Text", "Administrative_contact_telephone",c=>c.Administrative_contact_telephone);
            Bind(tbAdministrativeContactAddress,"Text", "Administrative_contact_address",c=>c.Administrative_contact_address);
            Bind(tbCountryOfOrigin,"Text", "Country_of_origin",c=>c.Country_of_origin);
            Bind(tbEthicsApprover,"Text", "Ethics_approver",c=>c.Ethics_approver);
            Bind(tbSourceOfDataCollection,"Text", "Source_of_data_collection",c=>c.Source_of_data_collection);
            Bind(c_tbAttributionCitation,"Text", "Attribution_citation",c=>c.Attribution_citation);
            Bind(c_tbAccessOptions,"Text", "Access_options",c=>c.Access_options);

            Bind(c_tbDetailPageURL,"Text","Detail_Page_URL", c=>c.Detail_Page_URL);
            Bind(c_tbAPIAccessURL,"Text","API_access_URL", c=>c.API_access_URL );
            Bind(c_tbBrowseUrl,"Text","Browse_URL", c=>c.Browse_URL );
            Bind(c_tbBulkDownloadUrl,"Text","Bulk_Download_URL", c=>c.Bulk_Download_URL );
            Bind(c_tbQueryToolUrl,"Text","Query_tool_URL", c=>c.Query_tool_URL );
            Bind(c_tbSourceUrl,"Text","Source_URL",c=>c.Source_URL );
        }

        private bool _loadingUI = false;

        private void RefreshUIFromDatabase()
        {
            _loadingUI = true;
            try
            {
                ticketingControl1.ReCheckTicketingSystemInCatalogue();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
            
            splitContainer1.Enabled = true;
            ticketingControl1.Enabled = true;

            ticketingControl1.TicketText = _catalogue.Ticket;
            
            tbFolder.Text = _catalogue.Folder.Path;

            if (_catalogue.Explicit_consent == null)
                ddExplicitConsent.Text = "";
            else if (_catalogue.Explicit_consent == true)
                ddExplicitConsent.Text = "Yes";
            else
                ddExplicitConsent.Text = "No";

            c_tbLastRevisionDate.Text = _catalogue.Last_revision_date.ToString();
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

        private void c_tbLastRevisionDate_TextChanged(object sender, EventArgs e)
        {
            SetDate(c_tbLastRevisionDate, v => _catalogue.Last_revision_date = v);
        }

        private void tbDatasetStartDate_TextChanged(object sender, EventArgs e)
        {
            SetDate(tbDatasetStartDate,v=>_catalogue.DatasetStartDate = v);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueTab_Design, UserControl>))]
    public abstract class CatalogueTab_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
        
    }
}
