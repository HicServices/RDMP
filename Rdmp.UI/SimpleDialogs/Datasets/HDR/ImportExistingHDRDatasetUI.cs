using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Curation.Data.Datasets.HDR;
using Rdmp.UI.CommandExecution.AtomicCommands;
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

namespace Rdmp.UI.SimpleDialogs.Datasets.HDR
{
    public partial class ImportExistingHDRDatasetUI : RDMPForm
    {
        private readonly IActivateItems _activator;

        private readonly string[] _visibilities = { "FREE", "CAMPUS", "BACKEND", "CONFIDENTIAL" };

        private DatasetProviderConfiguration _providerConfiguration;
        private HDRDatasetProvider _datasetProvider;

        public ImportExistingHDRDatasetUI(IActivateItems activator, ExecuteCommandImportExistingHDRDatasetUI command) : base(activator)
        {
            _activator = activator;
            InitializeComponent();
            var configs = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("Type", typeof(HDRDatasetProvider).ToString());
            cbProviders.Items.AddRange(configs);
        }

        private void ImportExistingPureDatasetUI_Load(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (_activator.YesNo("Please confirm you wish to import this Dataset", "Import Dataset"))
            {
                try
                {
                    var provider = new HDRDatasetProvider(_activator, (DatasetProviderConfiguration)cbProviders.SelectedItem);
                    provider.AddExistingDataset(null, tbUrl.Text);
                    Close();
                    Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    lblError.Text = "Unable to locate dataset. Ensure UUID is correct.";
                    lblError.Visible = true;
                    return;
                }
            }
        }

        private void cbProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbProviders.SelectedItem is DatasetProviderConfiguration config)
            {
                _providerConfiguration = config;
                _datasetProvider = new HDRDatasetProvider(_activator, _providerConfiguration);
            }
        }
    }
}
