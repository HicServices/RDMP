// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Engine.Checks.Checkers;

/// <summary>
///     Checks all ProcessTasks that the user has configured for a given data load (See LoadMetadata).  This involves both
///     constructing and initializing
///     the instances (which can fail if Type names don't resolve etc) and calling check on the instantiated ProcessTask.
/// </summary>
public class ProcessTaskChecks : ICheckable
{
    private readonly ILoadMetadata _loadMetadata;
    private LoadArgsDictionary dictionary;

    public ProcessTaskChecks(ILoadMetadata loadMetadata)
    {
        _loadMetadata = loadMetadata;
    }

    public void Check(ProcessTask processTask, ICheckNotifier notifier)
    {
        if (dictionary == null)
            try
            {
                dictionary =
                    new LoadArgsDictionary(_loadMetadata, new HICDatabaseConfiguration(_loadMetadata).DeployInfo);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs("Could not assemble LoadArgsDictionary, see inner exception for specifics",
                        CheckResult.Fail, e));
                return;
            }


        var created = RuntimeTaskFactory.Create(processTask, dictionary.LoadArgs[processTask.LoadStage]);

        created.Check(notifier);
    }

    public void Check(ICheckNotifier notifier)
    {
        foreach (ProcessTask processTask in _loadMetadata.ProcessTasks.Where(pt => !pt.IsDisabled))
            Check(processTask, notifier);
    }
}