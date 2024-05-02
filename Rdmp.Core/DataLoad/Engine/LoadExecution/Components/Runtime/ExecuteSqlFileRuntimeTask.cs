// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
///     RuntimeTask that executes a single .sql file specified by the user in a ProcessTask with ProcessTaskType SQLFile.
/// </summary>
public class ExecuteSqlFileRuntimeTask : RuntimeTask
{
    public string Filepath;
    private readonly IProcessTask _task;

    private LoadStage _loadStage;

    public ExecuteSqlFileRuntimeTask(IProcessTask task, RuntimeArgumentCollection args) : base(task, args)
    {
        _task = task;
        Filepath = task.Path;
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        var db = RuntimeArguments.StageSpecificArguments.DbInfo;
        _loadStage = RuntimeArguments.StageSpecificArguments.LoadStage;

        if (!Exists())
            throw new Exception($"The sql file {Filepath} does not exist");

        string commandText;
        try
        {
            commandText = File.ReadAllText(Filepath);

            // Any string arguments refer to tokens that are to be replaced in the SQL file
            foreach (var kvp in RuntimeArguments.GetAllArgumentsOfType<string>())
            {
                var value = kvp.Value;

                if (value.Contains("<DatabaseServer>"))
                    value = value.Replace("<DatabaseServer>",
                        RuntimeArguments.StageSpecificArguments.DbInfo.Server.Name);

                if (value.Contains("<DatabaseName>"))
                    value = value.Replace("<DatabaseName>",
                        RuntimeArguments.StageSpecificArguments.DbInfo.GetRuntimeName());

                commandText = commandText.Replace($"##{kvp.Key}##", value);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Could not read the sql file at {Filepath}: {e}");
        }

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Executing script {Filepath} ( against {db})"));
        var executer = new ExecuteSqlInDleStage(job, _loadStage);
        return executer.Execute(commandText, db);
    }


    public override bool Exists()
    {
        return File.Exists(Filepath);
    }

    public override void Abort(IDataLoadEventListener postLoadEventListener)
    {
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }

    public override void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(Filepath))
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs($"ExecuteSqlFileTask {_task} does not have a path specified",
                    CheckResult.Fail));
            return;
        }

        if (!File.Exists(Filepath))
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"File '{Filepath}' does not exist! (the only time this would be legal is if you have an exe or a freaky plugin that creates this file)",
                    CheckResult.Warning));
        else
            notifier.OnCheckPerformed(new CheckEventArgs($"Found File '{Filepath}'",
                CheckResult.Success));
    }
}