using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Providers;
using DataExportLibrary.Providers.Nodes;

namespace DataExportManager.Collections
{
    /// <summary>
    /// RDMP Collection which shows all the Cohorts that have been committed to RDMP accross all Projects / Cohort Sources.
    /// </summary>
    public partial class SavedCohortsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;

        public SavedCohortsCollectionUI()
        {
            InitializeComponent();
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            CommonFunctionality.SetUp(RDMPCollection.SavedCohorts, tlvSavedCohorts,_activator,olvName,olvName);
            
            tlvSavedCohorts.AddObject(((DataExportChildProvider)_activator.CoreChildProvider).RootCohortsNode);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public static bool IsRootObject(object root)
        {
            return root is AllCohortsNode;
        }
    }
}
