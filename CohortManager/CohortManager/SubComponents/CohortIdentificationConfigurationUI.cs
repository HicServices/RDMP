using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.QueryBuilding.Parameters;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CohortManagerLibrary.QueryBuilding;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;

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
        private bool _collapse = true;
        
        public CohortIdentificationConfigurationUI()
        {
            InitializeComponent();
            
        }

        public CohortIdentificationConfiguration Configuration
        {
            get { return _configuration; }
            private set
            {
                _configuration = value;
                
            
                Enabled = !Configuration.Frozen;

                tbDescription.Text = value.Description;
                tbName.Text = value.Name;
                tbID.Text = value.ID.ToString();
                ticket.TicketText = value.Ticket;
                
                btnSave.Enabled = false;
            }
        }


        public void RefreshUIFromDatabase()
        {
            //make like we've just been opened with the same value as we had before though
            Configuration = Configuration;
        }


        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            Configuration.Description = tbDescription.Text;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (Configuration != null)
            {
                if (string.IsNullOrWhiteSpace(tbName.Text))
                {
                    tbName.Text = "No Name";
                    tbName.SelectAll();
                }

                Configuration.Name = tbName.Text;
            }
        }

        private void ticket_TicketTextChanged(object sender, EventArgs e)
        {
            if (Configuration != null)
                Configuration.Ticket = ticket.TicketText;
        }

        private void btnConfigureGlobalParameters_Click(object sender, EventArgs e)
        {
            if (Configuration != null)
            {
                var builder = new CohortQueryBuilder(Configuration);

                try
                {
                    builder.RegenerateSQL();
                }
                catch (QueryBuildingException ex)
                {
                    ExceptionViewer.Show("There was a problem resolving all the underlying parameters in all your various Aggregates, the following dialogue is reliable only for the Globals",ex);
                }
                
                var paramManager = builder.ParameterManager;

                var f = ParameterCollectionUI.ShowAsDialog(new ParameterCollectionUIOptions(ConfigureCohortIdentificationConfigurationGlobalParametersUseCase,Configuration, ParameterLevel.Global, paramManager));
                f.Closed+=(s,ev)=> RefreshUIFromDatabase();
            }
            
        }
        
        private const string ConfigureCohortIdentificationConfigurationGlobalParametersUseCase
            = "You are trying to build a cohort by performing SQL set operations on number of datasets, each dataset can have many filters which can have parameters.  It is likely that your datasets contain filters (e.g. 'only records from Tayside').  These filters may contain duplicate parameters (e.g. if you have 5 datasets each filtered by healthboard each with a parameter called @healthboard).  This dialog lets you configure a single 'overriding' master copy at the 'Cohort Identification Configuration' level which will allow you to change all copies at once in one place";
        
        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            Configuration = databaseObject;
            btnSave.SetupFor(databaseObject,activator.RefreshBus);
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return btnSave;
        }
    }
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CohortIdentificationConfigurationUI_Design, UserControl>))]
    public abstract class CohortIdentificationConfigurationUI_Design:RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
    }
}
