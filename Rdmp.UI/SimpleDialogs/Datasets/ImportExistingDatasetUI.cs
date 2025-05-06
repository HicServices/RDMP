using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data.Datasets;
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

namespace Rdmp.UI.SimpleDialogs.Datasets
{
    public partial class ImportExistingDatasetUI: RDMPForm
    {
        private readonly IActivateItems _activator;
        private readonly Type _providerType;
        public ImportExistingDatasetUI(IActivateItems activator, Type providerType) : base(activator)
        {
            _activator = activator;
            _providerType = providerType;
            InitializeComponent();
            var configs = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("Type", _providerType.ToString());
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
                    var provider = (cbProviders.SelectedItem as DatasetProviderConfiguration).GetProviderInstance(_activator);
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
        }
    }
}

