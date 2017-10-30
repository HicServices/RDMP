using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using ReusableUIComponents.Annotations;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// Shows all your data load configurations (See LoadMetadataUI for description of how these operate).  Each load configuration (LoadMetadata) is associated with 1 or more Catalogues. But 
    /// each Catalogue can only have one load configuration
    /// 
    /// Loads are made up of a collection Process Tasks  (See PluginProcessTaskUI, SqlProcessTaskUI and ExeProcessTaskUI). which are run in sequence at pre defined states of the data load
    /// (RAW => STAGING => LIVE).
    /// 
    /// There are many plugins that come as standard in the RDMP distribution such as the DelimitedFlatFileAttacher which lets you load files where cells are delimited by a specific character
    /// (e.g. commas).  Clicking 'Description' will display the plugins instructions on how/what stage in which to use it.  Where a plugin has a base class the help text will also be displayed
    /// for the base class which will in most cases be more generic description of the use case.
    /// 
    /// DataProvider tasks should mostly be used in GetFiles stage and are intended to be concerned with creating files in the ForLoading directory
    /// 
    /// Attacher tasks can only be used in 'Mounting' and are concerned with taking loading records into RAW tables
    /// 
    /// Mutilator tasks operate in the Adjust stages (usually AdjustRAW or AdjustSTAGING - mutilating LIVE would be a very bad idea).  These can do any task on a table(s) e.g. resolve duplication
    ///  
    /// The above guidelines are deliberately vague because plugins (especially third party plugins - see PluginManagementForm) can do almost anything.  For example you could have 
    /// a DataProvider which emailed you every time the data load began or a Mutilator which read data and sent it to a remote web service (or anything!).  Always read the descriptions of plugins 
    /// to make sure they do what you want. 
    /// 
    /// In addition to the plugins you can define SQL or EXE tasks that run during the load (See SqlProcessTaskUI and ExeProcessTaskUI). 
    /// </summary>
    public partial class LoadMetadataCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;
   
        public LoadMetadata SelectedLoadMetadata { get { return tlvLoadMetadata.SelectedObject as LoadMetadata; } }
        
        public LoadMetadataCollectionUI()
        {
            InitializeComponent();
            tlvLoadMetadata.RowHeight = 19;
            tlvLoadMetadata.FormatRow += tlvLoadMetadata_FormatRow;
        }

        void tlvLoadMetadata_FormatRow(object sender, FormatRowEventArgs e)
        {
            var projDir = e.Model as HICProjectDirectoryNode;

            if(projDir != null && projDir.IsEmpty)
                e.Item.ForeColor = Color.Red;
        }

        
        private void otvLoadMetadata_ItemActivate(object sender, EventArgs e)
        {
            var o = tlvLoadMetadata.SelectedObject;
            var hicDir = o as HICProjectDirectoryNode;
            var loadProgress = o as LoadProgress;
            
            var permissionWindow = o as PermissionWindow;
            var permissionWindowUsage = o as PermissionWindowUsedByCacheProgress;
            
            if(hicDir != null)
                hicDir.Activate();

            if (loadProgress != null)
                _activator.ActivateLoadProgress(this, loadProgress);

            if (permissionWindowUsage != null)
                permissionWindow = permissionWindowUsage.PermissionWindow;

            if (permissionWindow != null)
                _activator.ActivatePermissionWindow(this, permissionWindow);
        }
        private void otvLoadMetadata_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var cataNode = tlvLoadMetadata.SelectedObject as CatalogueUsedByLoadMetadataNode;

                var deletable = tlvLoadMetadata.SelectedObject as IDeleteable;
                if (deletable != null)
                    _activator.DeleteWithConfirmation(this, deletable);

                if(cataNode != null)
                    if (
                        MessageBox.Show(
                            "Confirm Disassociating Catalogue '" + cataNode +
                            "' from it's Load logic? This will not delete the Catalogue itself just the relationship to the load",
                            "Confirm disassociating Catalogue", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        cataNode.Catalogue.LoadMetadata_ID = null;
                        cataNode.Catalogue.SaveToDatabase();

                        _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(cataNode.LoadMetadata));
                    }
            }
        }

        public void ExpandToCatalogueLevel()
        {
            foreach (LoadMetadata loadMetadata in tlvLoadMetadata.Objects)
                tlvLoadMetadata.Expand(loadMetadata);
        }
        
        private void otvLoadMetadata_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            var factory = new AtomicCommandUIFactory(_activator.CoreIconProvider);

            var lmd = e.Model as LoadMetadata;
            var allCataloguesNode = e.Model as AllCataloguesUsedByLoadMetadataNode;
            var hicProjectDirectory = e.Model as HICProjectDirectoryNode;
            var schedulingNode = e.Model as LoadMetadataScheduleNode;
            
            var loadProgress = e.Model as LoadProgress;
            var cacheProgress = e.Model as CacheProgress;
            var permissionWindowUsage = e.Model as PermissionWindowUsedByCacheProgress;

            var loadStageNode = e.Model as LoadStageNode;

            if (e.Model == null)
                e.MenuStrip = factory.CreateMenu(new ExecuteCommandCreateNewLoadMetadata(_activator));

            if (allCataloguesNode != null)
                e.MenuStrip = factory.CreateMenu(new ExecuteCommandAssociateCatalogueWithLoadMetadata(_activator,allCataloguesNode.LoadMetadata));

            if (hicProjectDirectory != null)
                e.MenuStrip = factory.CreateMenu(new ExecuteCommandChooseHICProjectDirectory(_activator,hicProjectDirectory.LoadMetadata));

            if (loadStageNode != null)
                e.MenuStrip = new LoadStageNodeMenu(_activator, loadStageNode);

            if (schedulingNode != null)
                e.MenuStrip = new LoadMetadataScheduleNodeMenu(_activator, schedulingNode);

            if (lmd != null)
                e.MenuStrip = new LoadMetadataMenu(_activator, lmd);

            if (loadProgress != null)
                e.MenuStrip = new LoadProgressMenu(_activator, loadProgress);

            if (cacheProgress != null)
                e.MenuStrip = new CacheProgressMenu(_activator, cacheProgress);


            if (permissionWindowUsage != null)
                e.MenuStrip = new PermissionWindowUsageMenu(_activator, permissionWindowUsage);
        }
        
        public override void SetItemActivator(IActivateItems activator) 
        {
            _activator = activator;
            _activator.RefreshBus.EstablishLifetimeSubscription(this);
            
            CommonFunctionality.SetUp(
                tlvLoadMetadata,
                activator,
                olvName,
                tbFilter,
                olvName);
            
            RefreshAll();
        }

        public void RefreshAll()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(RefreshAll));
                return;
            }

            //get all those that still exist
            tlvLoadMetadata.ClearObjects();
            tlvLoadMetadata.AddObjects(_activator.CoreChildProvider.AllLoadMetadatas);
            ExpandToCatalogueLevel();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            var lmd = e.Object as LoadMetadata;

            if (lmd != null)
                if (lmd.Exists())
                {
                    if (!tlvLoadMetadata.Objects.Cast<LoadMetadata>().Contains(lmd)) //it exists and is a new one
                        tlvLoadMetadata.AddObject(lmd);
                }
                else
                    tlvLoadMetadata.RemoveObject(lmd);//it doesn't exist
        }
        
        public static bool IsRootObject(object root)
        {
            return root is LoadMetadata;
        }
    }
}
