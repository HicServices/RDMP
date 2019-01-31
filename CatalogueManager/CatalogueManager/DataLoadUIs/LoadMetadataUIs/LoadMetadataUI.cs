using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs
{
    /// <summary>
    /// Allows you to record a user friendly indepth description for your LoadMetadata, how it works and how to use it/maintain it.
    /// </summary>
    public partial class LoadMetadataUI : LoadMetadataUI_Design, ISaveableUI
    {
        public LoadMetadataUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.DataLoad;
        }
        
        protected override void SetBindings(BinderWithErrorProviderFactory rules, LoadMetadata databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID,"Text","ID",l=>l.ID);
            Bind(tbName,"Text","Name",l=>l.Name);
            Bind(tbDescription,"Text","Description",l=>l.Description);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoadMetadataUI_Design, UserControl>))]
    public abstract class LoadMetadataUI_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
    {

    }
}