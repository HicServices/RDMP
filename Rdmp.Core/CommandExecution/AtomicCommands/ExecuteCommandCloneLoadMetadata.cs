using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCloneLoadMetadata : BasicCommandExecution
{
    private LoadMetadata _loadMetadata;
    private IBasicActivateItems _activator;
    public ExecuteCommandCloneLoadMetadata(IBasicActivateItems activator,[DemandsInitialization("The LoadMetadata to clone")] LoadMetadata loadMetadata)
    {

        _loadMetadata = loadMetadata;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        var lmd = _loadMetadata.Clone();
        lmd.Name = lmd.Name + " (Clone)";
        lmd.SaveToDatabase();
        _activator.Publish(lmd);

    }
}
