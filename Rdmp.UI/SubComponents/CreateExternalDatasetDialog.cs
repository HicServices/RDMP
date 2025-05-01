using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Policy;

namespace Rdmp.UI.SubComponents
{
    public partial class CreateExternalDatasetDialog : Form
    {
        IActivateItems _activator;
        private Catalogue _catalogue;


        public CreateExternalDatasetDialog()
        {
            InitializeComponent();
        }

        public void Setup(IActivateItems activator, Catalogue catalogue)
        {
            _activator = activator;
            _catalogue = catalogue;
            cbPovider.Items.AddRange(_activator.RepositoryLocator.CatalogueRepository.GetAllObjects<DatasetProviderConfiguration>());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var provider = ((DatasetProviderConfiguration)cbPovider.SelectedItem).GetProviderInstance(_activator);
            var dataset = provider.Create(_catalogue);
            var ds = provider.AddExistingDatasetWithReturn(null, dataset.GetID());
            if (!_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CatalogueDatasetLinkage>("Dataset_ID", ds.ID).Where(l => l.Catalogue.ID == _catalogue.ID).Any())
            {
                var linkage = new CatalogueDatasetLinkage(_activator.RepositoryLocator.CatalogueRepository, _catalogue, ds);
                linkage.SaveToDatabase();
            }
            Close();
        }
    }
}
