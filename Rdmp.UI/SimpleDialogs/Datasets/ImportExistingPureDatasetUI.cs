using Amazon.Auth.AccessControlPolicy;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Datasets;
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

namespace Rdmp.UI.SimpleDialogs.Datasets;

public partial class ImportExistingPureDatasetUI : RDMPForm
{
    private readonly IActivateItems _activator;

    private readonly string[] _visibilities = { "FREE", "CAMPUS", "BACKEND", "CONFIDENTIAL" };

    private DatasetProviderConfiguration _providerConfiguration;
    private PureDatasetProvider _datasetProvider;

    public ImportExistingPureDatasetUI(IActivateItems activator, ExecuteCommandImportExistingPureDatasetUI command) : base(activator)
    {
        _activator = activator;
        InitializeComponent();
        var configs = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("Type", typeof(PureDatasetProvider).ToString());
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
            if (!_datasetProvider.CheckDatasetExistsAtURL(tbUrl.Text))
            {
                lblError.Text = "Unable to locate dataset. Ensure UUID url is used";
                lblError.Visible = true;
                return;
            }
            else
            {
                _datasetProvider.AddExistingDataset(null, tbUrl.Text);
                Close();
                Dispose();
            }
        }
    }

    private void cbProviders_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cbProviders.SelectedItem is DatasetProviderConfiguration config)
        {
            _providerConfiguration = config;
            _datasetProvider = new PureDatasetProvider(_activator, _providerConfiguration);
        }
    }
}
