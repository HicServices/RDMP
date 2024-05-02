// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
///     Translates a data load engine ProcessTask (design time template) configured by the user into the correct
///     RuntimeTask (realisation) based on the
///     ProcessTaskType (Attacher, Executable etc).  See DataLoadEngine.cd
/// </summary>
public class RuntimeTaskFactory
{
    public RuntimeTaskFactory(ICatalogueRepository repository)
    {
    }

    public static RuntimeTask Create(IProcessTask task, IStageArgs stageArgs)
    {
        //get the user configured Design Time arguments + stage specific arguments
        var args = new RuntimeArgumentCollection(task.GetAllArguments().ToArray(), stageArgs);

        //Create an instance of the the appropriate ProcessTaskType
        return task.ProcessTaskType switch
        {
            ProcessTaskType.Executable => new ExecutableRuntimeTask(task, args),
            ProcessTaskType.SQLFile => new ExecuteSqlFileRuntimeTask(task, args),
            ProcessTaskType.Attacher => new AttacherRuntimeTask(task, args),
            ProcessTaskType.DataProvider => new DataProviderRuntimeTask(task, args),
            ProcessTaskType.MutilateDataTable => new MutilateDataTablesRuntimeTask(task, args),
            ProcessTaskType.SQLBakFile => new ExecuteSqlBakFileRuntimeTask(task, args),
            _ => throw new Exception($"Cannot create runtime task: Unknown process task type '{task.ProcessTaskType}'")
        };
    }
}