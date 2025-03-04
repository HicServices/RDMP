﻿using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRestoreLoadMetadataVersion : BasicCommandExecution
{
    private readonly LoadMetadata _loadMetadata;
    private readonly IBasicActivateItems _activator;
    public ExecuteCommandRestoreLoadMetadataVersion(IBasicActivateItems activator, [DemandsInitialization("The LoadMetadata to version")] LoadMetadata loadMetadata)
    {

        _loadMetadata = loadMetadata;
        _activator = activator;
    }

    public override void Execute()
    {
        if (_activator.IsInteractive && !_activator.YesNo("Replace root Load Metadata with this configuration?", "Restore Load Metadata Version")) return;
        base.Execute();
        if (_loadMetadata.RootLoadMetadata_ID is null)
        {
            throw new Exception("Must Use a versioned LoadMetadata to create Version");
        }
        LoadMetadata lmd = (LoadMetadata)_activator.RepositoryLocator.CatalogueRepository.GetObjectByID(typeof(LoadMetadata), (int)_loadMetadata.RootLoadMetadata_ID) ?? throw new Exception("Could not find root load metadata");
        foreach (ProcessTask task in lmd.ProcessTasks.Cast<ProcessTask>())
        {
            task.DeleteInDatabase();
        }
        foreach (ProcessTask task in _loadMetadata.ProcessTasks.Cast<ProcessTask>())
        {
            task.Clone(lmd);
        }
        lmd.SaveToDatabase();
        _activator.Publish(lmd);
    }
}
