using Org.BouncyCastle.Asn1.X509.Qualified;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Datasets;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Frozen;
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

        private readonly string PureAssembly = typeof(PureDatasetProvider).ToString();
        private readonly string HDRAssembly = typeof(HDRDatasetProvider).ToString();
        private readonly string JiraAssembly = typeof(JiraDatasetProvider).ToString();
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
            if(((DatasetProviderConfiguration)cbPovider.SelectedItem).Type == JiraAssembly){
                var provider = new JiraDatasetProvider(_activator, (DatasetProviderConfiguration)cbPovider.SelectedItem);
                Dataset dataset;
                var fetchedDataset = (JiraDataset)provider.FetchDatasetByID(int.Parse(tbID.Text));//todo is it always ints?
                if (_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Dataset>("Url", fetchedDataset._links.self).Any())
                {
                    dataset = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Dataset>("Url", fetchedDataset._links.self).First();
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
            if (((DatasetProviderConfiguration)cbPovider.SelectedItem).Type == HDRAssembly)
            {
                var provider = new HDRDatasetProvider(_activator, (DatasetProviderConfiguration)cbPovider.SelectedItem);

                Dataset dataset;
                var fetchedDataset = (HDRDataset)provider.FetchDatasetByID(int.Parse(tbID.Text));//todo is it always ints?
                //todo check if portalurl is the correct one
                var existingUrl = $"{((DatasetProviderConfiguration)cbPovider.SelectedItem).Url}/v1/datasets/{fetchedDataset.data.id}?schema_model=HDRUK&schema_version=3.0.0";
                if (_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Dataset>("Url", existingUrl).Any())
                {
                    dataset = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Dataset>("Url", existingUrl).First();
                }
                else
                {
                    var url = ((DatasetProviderConfiguration)cbPovider.SelectedItem).Url+ "/v1/datasets/" + tbID.Text+ "?schema_model=HDRUK&schema_version=3.0.0";
                    dataset = provider.AddExistingDatasetWithReturn(null, url);
                }
                if (!_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CatalogueDatasetLinkage>("Dataset_ID", dataset.ID).Where(l => l.Catalogue.ID == _catalogue.ID).Any())
                {
                    var linkage = new CatalogueDatasetLinkage(_activator.RepositoryLocator.CatalogueRepository, _catalogue, dataset);
                    linkage.SaveToDatabase();
                }
                Close();
            }
            if (((DatasetProviderConfiguration)cbPovider.SelectedItem).Type == PureAssembly)
            {
                var provider = new PureDatasetProvider(_activator, (DatasetProviderConfiguration)cbPovider.SelectedItem);
                Dataset dataset;
                var fetchedDataset = (PureDataset)provider.FetchDatasetByID(int.Parse(tbID.Text));//todo is it always ints?
                //todo check if portalurl is the correct one
                if (_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Dataset>("Url", fetchedDataset.PortalURL).Any())
                {
                    dataset = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Dataset>("Url", fetchedDataset.PortalURL).First();
                }
                else
                {
                    var url = ((DatasetProviderConfiguration)cbPovider.SelectedItem).Url.Split("/ws/api").First() + "/en/datasets/" + tbID.Text;
                    dataset = provider.AddExistingDatasetWithReturn(null, url);
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
}
