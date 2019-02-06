// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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