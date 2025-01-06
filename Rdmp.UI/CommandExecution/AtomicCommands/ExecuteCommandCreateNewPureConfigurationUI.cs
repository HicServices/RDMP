using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.Datasets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewPureConfigurationUI : BasicCommandExecution, IAtomicCommand
{
    private readonly IActivateItems _activator;

    public ExecuteCommandCreateNewPureConfigurationUI(IActivateItems activator) : base(activator)
    {
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        var ui = new CreateNewPureConfigurationUI(_activator);
        ui.ShowDialog();
    }
}
