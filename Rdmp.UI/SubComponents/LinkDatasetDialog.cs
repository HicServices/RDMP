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

namespace Rdmp.UI.SubComponents
{
    public partial class LinkDatasetDialog : Form
    {
        IActivateItems _activator;
        private Catalogue _catalogue;
        public LinkDatasetDialog()
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
            Dataset dataset;
            var fetchedDataset = provider.FetchDatasetByID(int.Parse(tbID.Text));//todo is it always ints?
            if (_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Dataset>("Url", fetchedDataset.Url).Any())
            {
                dataset = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Dataset>("Url", fetchedDataset.Url).First();
            }
            else
            {
                dataset = provider.AddExistingDatasetWithReturn(null, tbID.Text);
            }
            if (!_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CatalogueDatasetLinkage>("Dataset_ID", dataset.ID).Where(l => l.Catalogue.ID == _catalogue.ID).Any())
            {
                var linkage = new CatalogueDatasetLinkage(_activator.RepositoryLocator.CatalogueRepository, _catalogue, dataset);
                linkage.SaveToDatabase();
            }
            Close();

        }
    }
}
