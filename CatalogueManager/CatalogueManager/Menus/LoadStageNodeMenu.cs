using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using DataLoadEngine.Attachers;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Mutilators;

namespace CatalogueManager.Menus
{
    internal class LoadStageNodeMenu : RDMPContextMenuStrip
    {
        private readonly LoadStageNode _loadStageNode;
        private MEF _mef;

        public LoadStageNodeMenu(RDMPContextMenuStripArgs args, LoadStageNode loadStageNode):base(args,null)
        {
            _loadStageNode = loadStageNode;
            _mef = _activator.RepositoryLocator.CatalogueRepository.MEF;
            
           AddMenu<IDataProvider>("Add New Data Provider");
           AddMenu<IAttacher>("Add New Attacher");
           AddMenu<IMutilateDataTables>("Add New Mutilator");

           Add(new ExecuteCommandCreateNewProcessTask(_activator, ProcessTaskType.SQLFile,loadStageNode.LoadMetadata, loadStageNode.LoadStage));
           Add(new ExecuteCommandCreateNewProcessTask(_activator, ProcessTaskType.Executable, loadStageNode.LoadMetadata, loadStageNode.LoadStage));
        }
        
        private void AddMenu<T>(string menuName)
        {
            var types = _mef.GetTypes<T>().ToArray();
            var menu = new ToolStripMenuItem(menuName);

            ProcessTaskType taskType;

            if(typeof(T) == typeof(IDataProvider))
                taskType = ProcessTaskType.DataProvider;
            else
                if (typeof(T) == typeof(IAttacher))
                    taskType = ProcessTaskType.Attacher;
                else if (typeof (T) == typeof (IMutilateDataTables))
                    taskType = ProcessTaskType.MutilateDataTable;
                else
                    throw new ArgumentException("Type '" + typeof (T) + "' was not expected", "T");
            
            foreach (Type type in types)
            {
                Type toAdd = type;
                menu.DropDownItems.Add(type.Name, null, (s, e) => AddTypeIntoStage(toAdd, taskType));
            }

            menu.Enabled = ProcessTask.IsCompatibleStage(taskType, _loadStageNode.LoadStage) && types.Any();

            Items.Add(menu);
        }



        private void AddTypeIntoStage(Type type, ProcessTaskType taskType)
        {
            var lmd = _loadStageNode.LoadMetadata;
            var stage = _loadStageNode.LoadStage;

            ProcessTask newTask = new ProcessTask((ICatalogueRepository)lmd.Repository, lmd, stage);
            newTask.Path = type.FullName;
            newTask.ProcessTaskType = taskType;
            newTask.Order = 0;
            newTask.Name = type.Name;

            newTask.SaveToDatabase();
            Publish(lmd);
            Activate(newTask);
        }
    }
}