using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;

namespace DataLoadEngine.LoadExecution.Components
{
    public class RuntimeTaskPackager
    {
        public readonly IEnumerable<ProcessTask> ProcessTasks;
        private readonly Dictionary<LoadStage, IStageArgs> _loadArgsDictionary;
        private readonly IEnumerable<ICatalogue> _cataloguesToLoad;
        private readonly CatalogueRepository _repository;

        public RuntimeTaskPackager(IEnumerable<ProcessTask> processTasks, Dictionary<LoadStage, IStageArgs> loadArgsDictionary, IEnumerable<ICatalogue> cataloguesToLoad, CatalogueRepository repository)
        {
            ProcessTasks = processTasks;
            _loadArgsDictionary = loadArgsDictionary;
            _cataloguesToLoad = cataloguesToLoad;
            _repository = repository;
        }

        public List<IRuntimeTask> GetRuntimeTasksForStage(LoadStage loadStage)
        {
            var runtimeTasks = new List<IRuntimeTask>();
            var tasksForThisLoadStage = ProcessTasks.Where(task => task.LoadStage == loadStage).ToList();
            var IDsOfCataloguesToLoad = _cataloguesToLoad.Select(cat => cat.ID).ToArray();

            if (!tasksForThisLoadStage.Any())
                return runtimeTasks;

            var factory = new Runtime.RuntimeTaskFactory(_repository);
            foreach (var processTask in tasksForThisLoadStage)
            {
                // Only return runtime tasks that relate to the catalogues to be loaded
                if (!(processTask.RelatesSolelyToCatalogue_ID == null || IDsOfCataloguesToLoad.Contains(processTask.RelatesSolelyToCatalogue_ID.Value)))
                    continue;

                var runtimeTask = factory.Create(processTask, _loadArgsDictionary[processTask.LoadStage]);
                
                runtimeTasks.Add(runtimeTask);
            }

            runtimeTasks = runtimeTasks.OrderBy(task => task.Order).ToList();
            return runtimeTasks;
        }

        public IEnumerable<IRuntimeTask> GetAllRuntimeTasks()
        {
            var runtimeTasks = new List<IRuntimeTask>();

            foreach (LoadStage loadStage in Enum.GetValues(typeof (LoadStage)))
                runtimeTasks.AddRange(GetRuntimeTasksForStage(loadStage));

            return runtimeTasks;
        }

        public CompositeDataLoadComponent CreateCompositeDataLoadComponentFor(LoadStage loadStage,string descriptionForComponent)
        {
            RuntimeTaskFactory factory = new RuntimeTaskFactory(_repository);

            var tasks = new List<IDataLoadComponent>();

            foreach (var task in GetRuntimeTasksForStage(loadStage))
                tasks.Add(factory.Create(task, _loadArgsDictionary[loadStage]));

            return new CompositeDataLoadComponent(tasks) { Description = descriptionForComponent };
        }
    }
}