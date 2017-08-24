using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;
using ReusableUIComponents.TransparentHelpSystem;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs
{
    /// <summary>
    /// Allows you to configure each stage in a data load (LoadMetadata).  A LoadMetadata is a recipe for how to load one or more datasets.  It should have a name and description which 
    /// accurately describes what it does (e.g. 'Load GP/Practice data' - 'Downloads PracticeGP.zip from FTP server, unzips and loads.  Also includes duplication resolution logic for 
    /// dealing with null vs 0 exact record duplication').  Clicking 'Edit...' will launch EditLoadMetadataDetails which will let you change this.
    /// 
    /// A data load takes place across 3 stages (RAW, STAGING, LIVE - see UserManual.docx).  Each stage can have 0 or more tasks associated with it (See LoadStagesUI).  The minimum requirement
    /// for a data load is to have an Attacher (class which populates RAW) e.g. AnySeparatorFileAttacher for comma separated files.  This supposes that your project folder loading directory 
    /// already has the files you are trying to load (See HICProjectDirectoryUI).  If you want to build an elegant automated solution then you may choose to use a GetFiles process such as 
    /// FTPDownloader to fetch new files directly off a data providers server.  After this you may need to write some bespoke SQL/Python scripts etc to deal with unclean/unloadable data or 
    /// just to iron out idiosyncrasies in the data.
    ///  
    /// Each module will have 0 or more arguments, each of which (when selected) will give you a description of what it expects and an appropriate control for you to choose an option. For
    /// example the argument SendLoadNotRequiredIfFileNotFound on FTPDownloader explains that when ticked 'If true the entire data load process immediately stops with exit code LoadNotRequired,
    /// if false then the load proceeds as normal'.  This means that you can end cleanly if there are no files to download or proceed anyway on the assumption that one of the other modules will
    /// produce the files that the load needs.
    /// 
    /// Clicking the alarm clock in the top right lets you configure the load to execute periodically (See LoadPeriodicallyUI) or define a LoadProgress associated with iterative loading (See 
    /// LoadProgressManagement).
    /// </summary>
    public partial class LoadMetadataUI : LoadMetadataUI_Design, ISaveableUI
    {
        private LoadMetadata _loadMetadata;
        private bool _bLoading;
        
        public LoadMetadata LoadMetadata
        {
            get { return _loadMetadata; }
            private set
            {
                _loadMetadata = value;

                if (_loadMetadata != null)
                    RefreshUIFromDatabase();

            }
        }

        private void RefreshUIFromDatabase()
        {
            if (VisualStudioDesignMode || RepositoryLocator == null)
                return;

            tbID.Text = _loadMetadata.ID.ToString();
            tbName.Text = _loadMetadata.Name;
            tbDescription.Text = _loadMetadata.Description;
        }

        
        public LoadMetadataUI()
        {
            InitializeComponent();
        }
        
        public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            LoadMetadata = databaseObject;
            objectSaverButton1.SetupFor(LoadMetadata,activator.RefreshBus);
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            _loadMetadata.Description = tbDescription.Text;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoadMetadataUI_Design, UserControl>))]
    public abstract class LoadMetadataUI_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
    {

    }
}