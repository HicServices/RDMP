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
    public partial class CreateExternalDatasetDialog : Form
    {
        IActivateItems _activator;
        private Catalogue _catalogue;

        private readonly string PureAssembly = typeof(PureDatasetProvider).ToString();
        private readonly string HDRAssembly = typeof(HDRDatasetProvider).ToString();
        private readonly string JiraAssembly = typeof(JiraDatasetProvider).ToString();
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
            PluginDatasetProvider provider = null;
            if (((DatasetProviderConfiguration)cbPovider.SelectedItem).Type == JiraAssembly)
            {
                provider = new JiraDatasetProvider(_activator, (DatasetProviderConfiguration)cbPovider.SelectedItem);
            }
            if (((DatasetProviderConfiguration)cbPovider.SelectedItem).Type == HDRAssembly)
            {
                provider = new HDRDatasetProvider(_activator, (DatasetProviderConfiguration)cbPovider.SelectedItem);
            }
            if (((DatasetProviderConfiguration)cbPovider.SelectedItem).Type == PureAssembly)
            {
                provider = new PureDatasetProvider(_activator, (DatasetProviderConfiguration)cbPovider.SelectedItem);
            }
            if (provider is null) throw new Exception("Unknown Provider Type");
            var dataset = provider.Create(_catalogue);

            if (!_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CatalogueDatasetLinkage>("Dataset_ID", dataset.ID).Where(l => l.Catalogue.ID == _catalogue.ID).Any())
            {
                var linkage = new CatalogueDatasetLinkage(_activator.RepositoryLocator.CatalogueRepository, _catalogue, dataset);
                linkage.SaveToDatabase();
            }
            Close();
        }
    }
}
