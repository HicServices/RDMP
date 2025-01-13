using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateLoadMetadataVersion: BasicCommandExecution
{
    private LoadMetadata _loadMetadata;
    public ExecuteCommandCreateLoadMetadataVersion([DemandsInitialization("The LoadMetadata to update")] LoadMetadata loadMetadata)
    {

        _loadMetadata = loadMetadata;
    }

    public override void Execute()
    {
        base.Execute();
        if(_loadMetadata.RootLoadMetadata_ID != null)
        {
            throw new Exception("Must Use Root LoadMetadata to create Version");
        }
        var lmd = _loadMetadata.Clone();
        lmd.SaveToDatabase();
    }
}
