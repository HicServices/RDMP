using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DataProvider;
using DataLoadEngine.LoadExecution.Components.Arguments;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// Translates a data load engine ProcessTask (design time template) configured by the user into the correct RuntimeTask (realisation) based on the
    /// ProcessTaskType (Attacher, Executable etc).  See DataLoadEngine.cd
    /// </summary>
    public class RuntimeTaskFactory
    {
        private readonly CatalogueRepository _repository;

        public RuntimeTaskFactory(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public RuntimeTask Create(IProcessTask task, IStageArgs stageArgs)
        {
            //get the user configured Design Time arguments + stage specific arguments
            var args = new RuntimeArgumentCollection(task.GetAllArguments().ToArray(), stageArgs);

            //Create an instance of the the appropriate ProcessTaskType
            switch (task.ProcessTaskType)
            {
                case ProcessTaskType.Executable:
                    return new ExecutableRuntimeTask(task, args);
                case ProcessTaskType.SQLFile:
                    return new ExecuteSqlFileRuntimeTask(task, args);
                case ProcessTaskType.StoredProcedure:
                    return new StoredProcedureRuntimeTask(task, args, _repository);
                case ProcessTaskType.Attacher:
                    return new AttacherRuntimeTask(task, args, _repository.MEF);
                case ProcessTaskType.DataProvider:
                    return new DataProviderRuntimeTask(task, args,_repository.MEF);
                case ProcessTaskType.MutilateDataTable:
                    return new MutilateDataTablesRuntimeTask(task, args, _repository.MEF);
                default:
                    throw new Exception("Cannot create runtime task: Unknown process task type '" + task.ProcessTaskType + "'");
            }
        }
    }
}