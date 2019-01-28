using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
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
    /// Allows you to record a user friendly indepth description for your LoadMetadata, how it works and how to use it/maintain it.
    /// </summary>
    public partial class LoadMetadataUI : LoadMetadataUI_Design, ISaveableUI
    {
        private LoadMetadata _loadMetadata;
        
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
            AssociatedCollection = RDMPCollection.DataLoad;
        }
        
        public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            LoadMetadata = databaseObject;
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