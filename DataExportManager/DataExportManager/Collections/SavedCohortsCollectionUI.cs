using System;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
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

            olvProjectNumber.AspectGetter = AspectGetter_ProjectNumber;
            olvVersion.AspectGetter = AspectGetter_Version;
        }

        private object AspectGetter_Version(object rowObject)
        {
            var c = rowObject as ExtractableCohort;

            if (c != null)
                return c.ExternalVersion;

            return null;
        }

        private object AspectGetter_ProjectNumber(object rowObject)
        {
            var c = rowObject as ExtractableCohort;

            if (c != null)
                return c.ExternalProjectNumber;;

            return null;
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
