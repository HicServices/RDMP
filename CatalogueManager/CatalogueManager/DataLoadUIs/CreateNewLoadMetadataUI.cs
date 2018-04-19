using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Logging;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.DataLoadUIs
{
    /// <summary>
    /// There is a M-1 relationship between Catalogues (datasets) and LoadMetadata (data load recipes).  This window is accessed by right clicking a Catalogue and choosing
    /// to configure it's LoadMetadata (how data is loaded).  You can either select an existing LoadMetadata (which will probably need modifying such that it correctly loads
    /// the new table in addition to what other datasets it already loaded.  Or you can create a new LoadMetadata and create a load from scratch.
    /// 
    /// <para>Once selected you will be taken to the dataset load configuration screen (See LoadMetadataUI)</para>
    /// 
    /// </summary>
    public partial class CreateNewLoadMetadataUI : RDMPForm
    {
        private readonly Catalogue _catalogue;
        private readonly IActivateItems _activator;
        public LoadMetadata LoadMetadataCreatedIfAny { get; set; }

        public CreateNewLoadMetadataUI(Catalogue catalogue, IActivateItems activator)
        {
            _catalogue = catalogue;
            _activator = activator;
            InitializeComponent();
            
            chooseLoggingTaskUI1.Catalogue = catalogue;
        }

        private void tbLoadMetadataNameToCreate_TextChanged(object sender, EventArgs e)
        {
            btnCreate.Enabled = !string.IsNullOrWhiteSpace(tbLoadMetadataNameToCreate.Text);
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(_catalogue.LoggingDataTask))
            {
                MessageBox.Show("You must configure a logging task first");
                return;
            }

            LoadMetadataCreatedIfAny = new LoadMetadata(RepositoryLocator.CatalogueRepository, tbLoadMetadataNameToCreate.Text);
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
