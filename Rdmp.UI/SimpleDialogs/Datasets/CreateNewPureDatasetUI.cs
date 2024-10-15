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

public partial class CreateNewPureDatasetUI : RDMPForm
{
    private readonly IActivateItems _activator;

    private readonly string[] _visibilities = { "FREE", "CAMPUS", "BACKEND", "CONFIDENTIAL" };

    private DatasetProviderConfiguration _providerConfiguration;
    private PureDatasetProvider _datasetProvider;
    private List<PureSystem> _publishers;

    public CreateNewPureDatasetUI(IActivateItems activator, ExecuteCommandCreateNewPureDatasetUI command) : base(activator)
    {
        _activator = activator;
        InitializeComponent();
        var configs = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("Type", typeof(PureDatasetProvider).ToString());
        cbDatasetProvider.Items.AddRange(configs);
        cbVisibility.Items.AddRange(_visibilities);
        tbDateMadeAvailable.Text = DateTime.Now.ToString();
    }

    private void CreateNewPureDatasetUI_Load(object sender, EventArgs e)
    {

    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {
        if (_activator.YesNo("Are you sure?", "Create dataset?"))
        {
            //var provider = new PureDatasetProvider(_activator, (DatasetProviderConfiguration)cbDatasetProvider.SelectedItem);
            //var date = new PureDate(2024);//TODO
            //provider.Create(tbName.Text,((PureDataset.System)cbPublisher.SelectedItem).UUID,lbPeople.SelectedItems,cbVisibility.SelectedItem,date);
        }
    }

    private void Reset()
    {
        cbPublisher.Items.Clear();
        cbPublisher.Enabled = false;
        lbPeople.Items.Clear();
        lbPeople.Enabled = false;
    }

    private void cbDatasetProvider_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cbDatasetProvider.SelectedItem is DatasetProviderConfiguration config)
        {
            _providerConfiguration = config;
            _datasetProvider = new PureDatasetProvider(_activator,_providerConfiguration);
            //fetch people
            //add people
            lbPeople.Enabled = true;

            _publishers = _datasetProvider.FetchPublishers();
            cbPublisher.Items.Add(_publishers);
            cbPublisher.Enabled = true;

        }
        else
        {
            Reset();
        }

    }
}
