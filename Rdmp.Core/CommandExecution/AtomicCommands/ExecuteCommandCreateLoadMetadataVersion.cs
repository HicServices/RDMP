using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using System;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateLoadMetadataVersion: BasicCommandExecution
{
    private readonly LoadMetadata _loadMetadata;
    private readonly IBasicActivateItems _activator;
    public ExecuteCommandCreateLoadMetadataVersion(IBasicActivateItems activator,[DemandsInitialization("The LoadMetadata to version")] LoadMetadata loadMetadata)
    {

        _loadMetadata = loadMetadata;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        if(_loadMetadata.RootLoadMetadata_ID != null)
        {
            throw new Exception("Must Use Root LoadMetadata to create Version");
        }
        var lmd = _loadMetadata.SaveNewVersion();
        lmd.SaveToDatabase();
        _activator.Publish(lmd);
    }
}
