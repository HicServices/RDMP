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
using DataExportManager.Collections.Providers;

namespace DataExportManager.Collections
{
    public partial class SavedCohortsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;
        private RDMPCollectionCommonFunctionality _commonCollectionFunctionality;

        public SavedCohortsCollectionUI()
        {
            InitializeComponent();
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            _commonCollectionFunctionality = new RDMPCollectionCommonFunctionality();
            _commonCollectionFunctionality.SetUp(tlvSavedCohorts,_activator,olvName,olvName);
            
            tlvSavedCohorts.AddObject(((DataExportChildProvider)_activator.CoreChildProvider).RootCohortsNode);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }
    }
}
