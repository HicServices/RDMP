using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCloneLoadMetadata : BasicCommandExecution
{
    private readonly LoadMetadata _loadMetadata;
    private readonly IBasicActivateItems _activator;
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
