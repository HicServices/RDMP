using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandToggleAllowReservedPrefixForLoadMetadata : BasicCommandExecution
{
    private LoadMetadata _loadMetadata;
    public ExecuteCommandToggleAllowReservedPrefixForLoadMetadata([DemandsInitialization("The LoadMetadata to update")] LoadMetadata loadMetadata)
    {

        _loadMetadata = loadMetadata;
    }

    public override void Execute()
    {
        base.Execute();
        _loadMetadata.AllowReservedPrefix = !_loadMetadata.AllowReservedPrefix;
        _loadMetadata.SaveToDatabase();
    }
}
