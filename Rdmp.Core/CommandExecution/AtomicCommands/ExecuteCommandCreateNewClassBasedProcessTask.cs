using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Mutilators;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewClassBasedProcessTask : BasicCommandExecution
    {
        private readonly LoadMetadata _loadMetadata;
        private readonly LoadStage _loadStage;
        private readonly Type _type;
        private readonly ProcessTaskType _processTaskType;

        public ExecuteCommandCreateNewClassBasedProcessTask(IBasicActivateItems activator, LoadMetadata loadMetadata, LoadStage loadStage, 
            [DemandsInitialization("Class to execute, must be an attacher, mutilater etc", TypeOf = typeof(IDisposeAfterDataLoad))]
            Type type) : base(activator)
        {
            _loadMetadata = loadMetadata;
            _loadStage = loadStage;
            _type = type;

            if (typeof(IAttacher).IsAssignableFrom(_type))
                _processTaskType = ProcessTaskType.Attacher;
            else
            if (typeof(IDataProvider).IsAssignableFrom(_type))
                _processTaskType = ProcessTaskType.DataProvider;
            else
            if (typeof(IMutilateDataTables).IsAssignableFrom(_type))
                _processTaskType = ProcessTaskType.MutilateDataTable;
            else
            {
                SetImpossible($"Type '{_type}' was not a compatible one e.g. IAttacher, IDataProvider or IMutilateDataTables");
            }

        }

        public override void Execute()
        {
            ProcessTask newTask = new ProcessTask(BasicActivator.RepositoryLocator.CatalogueRepository,_loadMetadata, _loadStage);
            newTask.Path = _type.FullName;
            newTask.ProcessTaskType = _processTaskType;
            newTask.Name = _type.Name;
            newTask.SaveToDatabase();

            newTask.CreateArgumentsForClassIfNotExists(_type);

            Publish(_loadMetadata);
            Activate(newTask);
        }
    }
}