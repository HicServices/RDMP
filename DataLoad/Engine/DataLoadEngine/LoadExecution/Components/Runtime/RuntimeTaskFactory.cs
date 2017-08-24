using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DataProvider;
using DataLoadEngine.LoadExecution.Components.Arguments;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public class RuntimeTaskFactory
    {
        private readonly CatalogueRepository _repository;

        public RuntimeTaskFactory(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public RuntimeTask Create(IProcessTask task, IStageArgs stageArgs)
        {
            RuntimeTask runtimeTask;

            //get the immutable Design Time arguments
            var args = new RuntimeArgumentCollection(task.GetAllArguments().ToArray(), stageArgs);

            //Create an instance of the the appropriate ProcessTaskType
            switch (task.ProcessTaskType)
            {
                case ProcessTaskType.Executable:
                    runtimeTask = new ExecutableRuntimeTask(task, args);
                    break;
                case ProcessTaskType.SQLFile:
                    runtimeTask = new ExecuteSqlFileRuntimeTask(task, args);
                    break;
                case ProcessTaskType.StoredProcedure:
                    runtimeTask = new StoredProcedureRuntimeTask(task, args, _repository);
                    break;
                case ProcessTaskType.Attacher:
                    runtimeTask = new AttacherRuntimeTask(task, args, _repository.MEF);
                    break;
                case ProcessTaskType.DataProvider:
                    runtimeTask = new DataProviderRuntimeTask(task, args,_repository.MEF);
                    break;
                case ProcessTaskType.MutilateDataTable:
                    runtimeTask = new MutilateDataTablesRuntimeTask(task, args, _repository.MEF);
                    break;
                default:
                    throw new Exception("Cannot create runtime task: Unknown process task type '" + task.ProcessTaskType + "'");
            }

            return runtimeTask;
        }
    }
}