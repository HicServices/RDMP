using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Datasets;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs.Datasets
{
    public partial class CreateNewPureConfigurationUI : RDMPForm
    {

        private readonly IActivateItems _activator;
        public CreateNewPureConfigurationUI(IActivateItems activator)
        {
            InitializeComponent();
            _activator = activator;
            var dataAccessCredentials = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>();
            cbCredentials.Items.Clear();
            cbCredentials.Items.AddRange(dataAccessCredentials);
        }

        private void DisableSave()
        {
            btnSave.Enabled = false;
        }

        private void ValidateForm(object sender, EventArgs e)
        {

            if (cbCredentials.SelectedItem is null)
            {
                DisableSave();
                return;
            }
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                DisableSave();
                return;
            }
            if (string.IsNullOrWhiteSpace(tbUrl.Text))
            {
                DisableSave();
                return;
            }
            if (string.IsNullOrWhiteSpace(tbOrganisationId.Text))
            {
                DisableSave();
                return;
            }
            btnSave.Enabled = true;
        }

        private void Save(object sender, EventArgs e)
        {
            var config = new DatasetProviderConfiguration(_activator.RepositoryLocator.CatalogueRepository, tbName.Text, typeof(PureDatasetProvider).ToString(), tbUrl.Text, ((DataAccessCredentials)cbCredentials.SelectedItem).ID, tbOrganisationId.Text);
            config.SaveToDatabase();
            _activator.Publish(config);
            Close();
            _activator.Show($"Dataset Provider '{tbName.Text}' has successfully been created");
        }
    }
}
