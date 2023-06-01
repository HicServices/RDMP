// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components;

/// <summary>
/// Converts multiple user defined DLE ProcessTasks into a single hydrated CompositeDataLoadComponent.  This involves converting the ProcessTasks
/// (which are user defined class names, argument values etc) into instances of IRuntimeTask.  You can either call CreateCompositeDataLoadComponentFor
/// to create a generic CompositeDataLoadComponent containing all the IRuntimeTasks or you can get the IRuntimeTask list directly and use it yourself in
/// a more advanced DataLoadComponent (e.g. PopulateRAW - See usages in HICDataLoadFactory)
/// </summary>
public class RuntimeTaskPackager
{
    public readonly IEnumerable<IProcessTask> ProcessTasks;
    private readonly Dictionary<LoadStage, IStageArgs> _loadArgsDictionary;
    private readonly IEnumerable<ICatalogue> _cataloguesToLoad;
    private readonly ICatalogueRepository _repository;

    public RuntimeTaskPackager(IEnumerable<IProcessTask> processTasks, Dictionary<LoadStage, IStageArgs> loadArgsDictionary, IEnumerable<ICatalogue> cataloguesToLoad, ICatalogueRepository repository)
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

        if (!tasksForThisLoadStage.Any())
            return runtimeTasks;

        var factory = new Runtime.RuntimeTaskFactory(_repository);
        foreach (var processTask in tasksForThisLoadStage)
            runtimeTasks.Add(factory.Create(processTask, _loadArgsDictionary[processTask.LoadStage]));
            

        runtimeTasks = runtimeTasks.OrderBy(task => task.ProcessTask.Order).ToList();
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
        var factory = new RuntimeTaskFactory(_repository);

        var tasks = new List<IDataLoadComponent>();

        foreach (var task in GetRuntimeTasksForStage(loadStage))
            tasks.Add(factory.Create(task.ProcessTask, _loadArgsDictionary[loadStage]));

        return new CompositeDataLoadComponent(tasks) { Description = descriptionForComponent };
    }
}