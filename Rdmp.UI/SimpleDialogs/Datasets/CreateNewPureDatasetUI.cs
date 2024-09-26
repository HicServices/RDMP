using Amazon.Auth.AccessControlPolicy;
using Rdmp.Core.CommandExecution.AtomicCommands;
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

    public CreateNewPureDatasetUI(IActivateItems activator, ExecuteCommandCreateNewPureDatasetUI command) : base(activator)
    {
        _activator = activator;
        InitializeComponent();
        var configs = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("Type", typeof(PureDatasetProvider).ToString());
        cbDatasetProvider.Items.AddRange(configs);
    }

    private void CreateNewPureDatasetUI_Load(object sender, EventArgs e)
    {

    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {

    }
}
