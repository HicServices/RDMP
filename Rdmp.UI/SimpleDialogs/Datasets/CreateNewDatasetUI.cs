using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;

namespace Rdmp.UI.SimpleDialogs.Datasets;

public partial class CreateNewDatasetUI : RDMPForm
{
    private readonly IActivateItems _activator;
    public CreateNewDatasetUI(IActivateItems activator, ExecuteCommandCreateNewDatasetUI command) : base(activator)
    {
        _activator = activator;
        InitializeComponent();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandCreateDataset(_activator,tbName.Text,tbDOI.Text,tbSource.Text);
        cmd.Execute();
        Close();
    }
}