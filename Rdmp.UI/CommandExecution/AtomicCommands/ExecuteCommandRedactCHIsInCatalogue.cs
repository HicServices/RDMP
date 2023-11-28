using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandRedactCHIsInCatalogue: BasicUICommandExecution
{
    private ICatalogue _catalogue;
    private IActivateItems _activator;
    public ExecuteCommandRedactCHIsInCatalogue(IActivateItems activator, ICatalogue catalogue) : base(activator)
    {
        _catalogue = catalogue;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        var dialog = new RedactChisInCatalogueDialog(_activator, _catalogue);
        dialog.Show();
    }
}
