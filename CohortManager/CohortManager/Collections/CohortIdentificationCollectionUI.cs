using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CohortManager.Collections.Providers;
using CohortManager.ItemActivation;
using CohortManager.Menus;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using ReusableUIComponents.TreeHelper;

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
        private IActivateCohortIdentificationItems _activator;
        
        public CohortIdentificationCollectionUI()
        {
            InitializeComponent();
            
        }


        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = (IActivateCohortIdentificationItems)activator;
            
            //important to register the setup before the lifetime subscription so it gets priority on events
            CommonFunctionality.SetUp(
                tlvCohortIdentificationConfigurations,
                _activator,
                olvName,//column with the icon
                tbFilter, 
                olvName,//column that can be renamed
                lblHowToEdit
                );
            
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
            var cic = o as CohortIdentificationConfiguration;
            var cohortContainer = o as CohortAggregateContainer;
            var patientIndexTablesNode = o as JoinableCollectionNode;

            //if user clicked on a cohort identification configuration or on whitespace
            if(cic != null || e.Model == null)
                e.MenuStrip = new CohortIdentificationConfigurationMenu(_activator,cic);
            
            if (cohortContainer != null)
            {
                var rootParent = CommonFunctionality.ParentFinder.GetFirstOrNullParentRecursivelyOfType<CohortIdentificationConfiguration>(cohortContainer);
                e.MenuStrip = new CohortAggregateContainerMenu(_activator, rootParent, cohortContainer);
            }

            if (patientIndexTablesNode != null)
                e.MenuStrip = new JoinableCollectionNodeMenu(_activator, patientIndexTablesNode);
        }
        
        private void tlvCohortIdentificationConfigurations_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                var deletable = tlvCohortIdentificationConfigurations.SelectedObject as IDeleteable;
                if(deletable != null)
                    _activator.DeleteWithConfirmation(this,deletable);
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
