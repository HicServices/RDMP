using System;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CohortManager.CommandExecution.AtomicCommands;
using CohortManager.Menus;
using MapsDirectlyToDatabaseTable;

namespace CohortManager.Collections
{
    /// <summary>
    /// Displays all the cohort identification configurations you have configured in RDMP. Cohort Identification Configurations (CIC) are created to identify specific patients e.g. 'all patients 
    /// with 3 or more prescriptions for a diabetes drug or who have been hospitalised for an amputation'.  Each CIC achieves it's goal by combining Cohort Sets with Set operations (UNION,
    /// INTERSECT, EXCEPT) for example Cohort Set 1 '3+ diabetes drug prescriptions' UNION 'hospital admissions for amputations'.  Cohort sets can be from the same or different data sets (as
    /// long as they have a common identifier).
    /// </summary>
    public partial class CohortIdentificationCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        //for expand all/ collapse all
        private IActivateItems _activator;
        
        public CohortIdentificationCollectionUI()
        {
            InitializeComponent();
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            
            //important to register the setup before the lifetime subscription so it gets priority on events
            CommonFunctionality.SetUp(
                tlvCohortIdentificationConfigurations,
                _activator,
                olvName,//column with the icon
                tbFilter, 
                olvName//column that can be renamed
                
                );

            CommonFunctionality.WhitespaceRightClickMenuCommands = new []{new ExecuteCommandCreateNewCohortIdentificationConfiguration(activator)};
            
            _activator.RefreshBus.EstablishLifetimeSubscription(this);
            
            tlvCohortIdentificationConfigurations.AddObjects(_activator.CoreChildProvider.AllCohortIdentificationConfigurations);
        }
        
        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            //if it is a new cohort identification configuration
            if (e.Object is CohortIdentificationConfiguration && e.Object.Exists())
                //it exists and we don't know about it?
                if (!tlvCohortIdentificationConfigurations.Objects.Cast<object>().Contains(e.Object))
                    tlvCohortIdentificationConfigurations.AddObject(e.Object); //add it
        }

        private void tlvCohortIdentificationConfigurations_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            var o = e.Model;
            var cohortContainer = o as CohortAggregateContainer;
            
            if (cohortContainer != null)
            {
                var rootParent = CommonFunctionality.ParentFinder.GetFirstOrNullParentRecursivelyOfType<CohortIdentificationConfiguration>(cohortContainer);
                e.MenuStrip = new CohortAggregateContainerMenu(_activator, rootParent, cohortContainer);
            }
        }
        

        private void btnExpandOrCollapse_Click(object sender, EventArgs e)
        {
            CommonFunctionality.ExpandOrCollapse(btnExpandOrCollapse);
        }

        public static bool IsRootObject(object root)
        {
            return root is CohortIdentificationConfiguration;
        }
    }
}
