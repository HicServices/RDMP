using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRestoreLoadMetadataVersion : BasicCommandExecution
{
    private LoadMetadata _loadMetadata;
    private IBasicActivateItems _activator;
    public ExecuteCommandRestoreLoadMetadataVersion(IBasicActivateItems activator, [DemandsInitialization("The LoadMetadata to version")] LoadMetadata loadMetadata)
    {

        _loadMetadata = loadMetadata;
        _activator = activator;
    }

    public override void Execute()
    {
        if (!_activator.YesNo("Replace root Load Metadata with this configuration?","Resotre Load Metadata Version")) return;
        base.Execute();
        if (_loadMetadata.RootLoadMetadata_ID is null)
        {
            throw new Exception("Must Use a versioned LoadMetadata to create Version");
        }
        LoadMetadata lmd = (LoadMetadata)_activator.RepositoryLocator.CatalogueRepository.GetObjectByID(typeof(LoadMetadata), (int)_loadMetadata.RootLoadMetadata_ID);
        if (lmd is null)
        {
            throw new Exception("Could not find root load metadata");

        }
        foreach (ProcessTask task in lmd.ProcessTasks)
        {
            task.DeleteInDatabase();
        }
        foreach(ProcessTask task in _loadMetadata.ProcessTasks)
        {
            task.Clone(lmd);
        }
        lmd.SaveToDatabase();
        _activator.Publish(lmd);
    }
}
