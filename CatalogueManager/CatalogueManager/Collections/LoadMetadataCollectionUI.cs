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
using CatalogueLibrary.Nodes;
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
using CatalogueManager.Copying;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// This collection Shows all your data load configurations.  Each load configuration (LoadMetadata) is associated with 1 or more Catalogues. But each Catalogue can only have one load configuration
    /// 
    /// <para>Loads are made up of a collection Process Tasks  (See PluginProcessTaskUI, SqlProcessTaskUI and ExeProcessTaskUI). which are run in sequence at pre defined states of the data load
    /// (RAW => STAGING => LIVE).</para>
    /// 
    /// <para>Within the tree collection you can configure each stage in a data load (LoadMetadata).  A LoadMetadata is a recipe for how to load one or more datasets.  It should have a name and 
    /// description which accurately describes what it does (e.g. 'Load GP/Practice data' - 'Downloads PracticeGP.zip from FTP server, unzips and loads.  Also includes duplication resolution logic for 
    /// dealing with null vs 0 exact record duplication').</para>
    /// 
    /// <para>A data load takes place across 3 stages (RAW, STAGING, LIVE - see UserManual.docx).  Each stage can have 0 or more tasks associated with it (See PluginProcessTaskUI).  The minimum requirement
    /// for a data load is to have an Attacher (class which populates RAW) e.g. AnySeparatorFileAttacher for comma separated files.  This supposes that your project folder loading directory 
    /// already has the files you are trying to load (See ChooseHICProjectDialog).  If you want to build an elegant automated solution then you may choose to use a GetFiles process such as 
    /// FTPDownloader to fetch new files directly off a data providers server.  After this you may need to write some bespoke SQL/Python scripts etc to deal with unclean/unloadable data or 
    /// just to iron out idiosyncrasies in the data.</para>
    ///  
    /// <para>Each module will have 0 or more arguments, each of which (when selected) will give you a description of what it expects and an appropriate control for you to choose an option. For
    /// example the argument SendLoadNotRequiredIfFileNotFound on FTPDownloader explains that when ticked 'If true the entire data load process immediately stops with exit code LoadNotRequired,
    /// if false then the load proceeds as normal'.  This means that you can end cleanly if there are no files to download or proceed anyway on the assumption that one of the other modules will
    /// produce the files that the load needs.</para>
    ///
    /// <para> There are many plugins that come as standard in the RDMP distribution such as the DelimitedFlatFileAttacher which lets you load files where cells are delimited by a specific character
    /// (e.g. commas).  Clicking 'Description' will display the plugins instructions on how/what stage in which to use it.</para>
    /// 
    /// <para>DataProvider tasks should mostly be used in GetFiles stage and are intended to be concerned with creating files in the ForLoading directory</para>
    /// 
    /// <para>Attacher tasks can only be used in 'Mounting' and are concerned with taking loading records into RAW tables</para>
    /// 
    /// <para>Mutilator tasks operate in the Adjust stages (usually AdjustRAW or AdjustSTAGING - mutilating LIVE would be a very bad idea).  These can do any task on a table(s) e.g. resolve duplication</para>
    ///  
    /// <para>The above guidelines are deliberately vague because plugins (especially third party plugins - see PluginManagementForm) can do almost anything.  For example you could have 
    /// a DataProvider which emailed you every time the data load began or a Mutilator which read data and sent it to a remote web service (or anything!).  Always read the descriptions of plugins 
    /// to make sure they do what you want. </para>
    /// 
    /// <para>In addition to the plugins you can define SQL or EXE tasks that run during the load (See SqlProcessTaskUI and ExeProcessTaskUI). </para>
    /// </summary>
    public partial class LoadMetadataCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;
   
        public LoadMetadata SelectedLoadMetadata { get { return tlvLoadMetadata.SelectedObject as LoadMetadata; } }
        
        public LoadMetadataCollectionUI()
        {
            InitializeComponent();
            tlvLoadMetadata.RowHeight = 19;
        }
        
        public override void SetItemActivator(IActivateItems activator) 
        {
            _activator = activator;
            _activator.RefreshBus.EstablishLifetimeSubscription(this);
            
            CommonFunctionality.SetUp(
                RDMPCollection.DataLoad,
                tlvLoadMetadata,
                activator,
                olvName,
                olvName);

            CommonFunctionality.WhitespaceRightClickMenuCommands = new[] {new ExecuteCommandCreateNewLoadMetadata(_activator)};
            
            tlvLoadMetadata.AddObject(_activator.CoreChildProvider.AllPermissionWindowsNode);
            tlvLoadMetadata.AddObject(_activator.CoreChildProvider.AllLoadMetadatasNode);
        }


        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            if (e.Object is LoadMetadata)
                tlvLoadMetadata.RefreshObject(tlvLoadMetadata.Objects.OfType<AllLoadMetadatasNode>());

            if (e.Object is PermissionWindow)
                tlvLoadMetadata.RefreshObject(tlvLoadMetadata.Objects.OfType<AllPermissionWindowsNode>());

            if (e.Object is CacheProgress)
                tlvLoadMetadata.RefreshObject(tlvLoadMetadata.Objects.OfType<AllPermissionWindowsNode>());
        }
        
        public static bool IsRootObject(object root)
        {
            return
                root is AllPermissionWindowsNode ||
                root is AllLoadMetadatasNode;
        }
    }
}
