// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components;

/// <summary>
/// DLE component responsible for the LoadStage.GetFiles.  This includes running any user configured ProcessTask (which will be RuntimeTasks in _components).
/// Also pushes DeleteForLoadingFilesOperation onto the disposal stack so that any files accumulated in ForLoading are cleared at the end of the DLE run (After
/// archiving).
/// </summary>
public class LoadFiles : CompositeDataLoadComponent
{
    public LoadFiles(List<IRuntimeTask> collection) : base(collection.Cast<IDataLoadComponent>().ToList())
    {
        Description = Description = "LoadFiles";
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        if (Skip(job))
            return ExitCodeType.Error;

        var
            toReturn = ExitCodeType
                .Success; //This default will be returned unless there is an explicit DataProvider or collection of runtime tasks to run which return a different result (See below)

        // Figure out where we are getting the source files from
        try
        {
            if (Components.Any())
                toReturn = base.Run(job, cancellationToken);
            else
            {
                if (!((LoadDirectory)job.LoadDirectory).AllSubdirectoriesExist())
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                            $"One or more of the Load Metadata directories does not exist. Check the following locations exits: {string.Join(", ", job.LoadDirectory.ForLoading.FullName, job.LoadDirectory.ForArchiving.FullName, job.LoadDirectory.Cache.FullName, job.LoadDirectory.ExecutablesPath.FullName)}"));
                    return ExitCodeType.Error;
                }
                else if (job.LoadDirectory.ForLoading.EnumerateFileSystemInfos().Any())
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                        $"Using existing files in '{job.LoadDirectory.ForLoading.FullName}', there are no GetFiles processes or DataProviders configured"));
                else
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"There are no GetFiles tasks and there are no files in the ForLoading directory ({job.LoadDirectory.ForLoading.FullName})"));
            }

        }
        finally
        {
            // We can only clean up ForLoading after the job is finished, so give it the necessary disposal operation
            job.PushForDisposal(new DeleteForLoadingFilesOperation(job));
        }

        return toReturn;
    }

    public void DeleteAllFilesInForLoading(LoadDirectory directory, DataLoadJob job)
    {
        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Deleting files in ForLoading ({directory.ForLoading.FullName})"));
        directory.ForLoading.EnumerateFiles().ToList().ForEach(info => info.Delete());
        directory.ForLoading.EnumerateDirectories().ToList().ForEach(info => info.Delete(true));
    }
}