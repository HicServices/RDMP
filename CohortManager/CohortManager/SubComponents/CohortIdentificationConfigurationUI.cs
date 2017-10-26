using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CohortManager.SubComponents
{
    /// <summary>
    /// Allows you to view/edit a CohortIdentificationConfiguration (See CohortManagerMainForm for a description of what one of these is).  This includes giving it a sensible name
    /// e.g. 'Project 132 Cases - Deaths caused by diabetic medication' and AS FULL A DESCRIPTION AS POSSIBLE e.g. 'All patients in Tayside and Fife who are over 16 at the time of 
    /// their first prescription of a diabetic medication (BNF chapter 6.1) and died within 6 months of the first prescribed date of the diabetic medication'.  The better the 
    /// description the more likely it is that you and the researcher will be on the same page about what you are providing.
    /// 
    /// If you have a large data repository or plan to use lots of different datasets or complex filters in your CohortIdentificationCriteria you should configure a caching database
    /// (See QueryCachingServerSelector) from the dropdown.
    /// 
    /// Next you should add datasets and set operation containers to generate your cohort by dragging datasets from the Catalogue list on the right into the CohortCompilerUI
    /// list box (See CohortCompilerUI for configuring filters on the datasets added).
    /// 
    /// In the above example you might have 
    /// 
    /// Set 1 - Prescribing
    /// 
    ///     Filter 1 - Prescription is for a diabetic medication
    /// 
    ///     Filter 2 - Prescription is the first prescription of it's type for the patient
    /// 
    ///     Filter 3 - Patient died within 6 months of prescription
    /// 
    /// INTERSECT
    /// 
    /// Set 2 - Demography
    ///     
    ///     Filter 1 - Latest known healthboard is Tayside or Fife
    /// 
    ///     Filter 2 - Date of Death - Date of Birth > 16 years
    ///  
    /// </summary>
    public partial class CohortIdentificationConfigurationUI : CohortIdentificationConfigurationUI_Design, ISaveableUI
    {
        private CohortIdentificationConfiguration _configuration;
        
        public CohortIdentificationConfigurationUI()
        {
            InitializeComponent();
            queryCachingServerSelector.SelectedServerChanged += queryCachingServerSelector_SelectedServerChanged;
        }

        void queryCachingServerSelector_SelectedServerChanged()
        {
            if (queryCachingServerSelector.SelecteExternalDatabaseServer == null)
                _configuration.QueryCachingServer_ID = null;
            else
                _configuration.QueryCachingServer_ID = queryCachingServerSelector.SelecteExternalDatabaseServer.ID;

            _configuration.SaveToDatabase();
            _activator.RefreshBus.Publish(queryCachingServerSelector,new RefreshObjectEventArgs(_configuration));

        }

        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _configuration = databaseObject;

            tbID.Text = _configuration.ID.ToString();
            tbName.Text = _configuration.Name;
            tbDescription.Text = _configuration.Description;
            ticket.TicketText = _configuration.Ticket;
            
            objectSaverButton1.SetupFor(databaseObject,activator.RefreshBus);

            if (_configuration.QueryCachingServer_ID == null)
                queryCachingServerSelector.SelecteExternalDatabaseServer = null;
            else
                queryCachingServerSelector.SelecteExternalDatabaseServer = _configuration.QueryCachingServer;

            CohortCompilerUI1.SetDatabaseObject(activator,databaseObject);
        }

        public override string GetTabName()
        {
            return "Execute:" + base.GetTabName();
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                tbName.Text = "No Name";
                tbName.SelectAll();
            }

            _configuration.Name = tbName.Text;
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            _configuration.Description = tbDescription.Text;
        }

        private void ticket_TicketTextChanged(object sender, EventArgs e)
        {
            _configuration.Ticket = ticket.TicketText;
        }

        private bool collapse = true;
        private void btnCollapseOrExpand_Click(object sender, EventArgs e)
        {

            ticket.Visible = !collapse;
            lblName.Visible = !collapse;
            lblDescription.Visible = !collapse;
            tbName.Visible = !collapse;
            tbDescription.Visible = !collapse;
            objectSaverButton1.Visible = !collapse;

            if (collapse)
            {
                CohortCompilerUI1.Top = tbID.Bottom;
                CohortCompilerUI1.Height = Bottom - tbID.Bottom;

            }
            else
            {
                CohortCompilerUI1.Top = objectSaverButton1.Bottom;
                CohortCompilerUI1.Height = Bottom - objectSaverButton1.Bottom;
            }

            collapse = !collapse;
            btnCollapseOrExpand.Text = collapse ? "-" : "+";
        }
    }
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CohortIdentificationConfigurationUI_Design, UserControl>))]
    public abstract class CohortIdentificationConfigurationUI_Design : RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
    }
}