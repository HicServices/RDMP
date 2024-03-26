// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System.Text.Json;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
/// RuntimeTask that executes a single .bak file specified by the user in a ProcessTask with ProcessTaskType SQLBakFile.
/// </summary>
public class ExecuteSqlBakFileRuntimeTask : RuntimeTask
{
    public string Filepath;
    private readonly IProcessTask _task;

    public string PrimaryFilePhysicalName;
    public string LogFilePhysicalName;
    private LoadStage _loadStage;

    public ExecuteSqlBakFileRuntimeTask(IProcessTask task, RuntimeArgumentCollection args) : base(task, args)
    {
        _task = task;
        Filepath = task.Path;
        if (!string.IsNullOrWhiteSpace(task.SerialisableConfiguration))
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(task.SerialisableConfiguration);
            config.TryGetValue("PrimaryFilePhysicalName", out PrimaryFilePhysicalName);
            config.TryGetValue("LogFilePhysicalName", out LogFilePhysicalName);
        }
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        var db = RuntimeArguments.StageSpecificArguments.DbInfo;
        _loadStage = RuntimeArguments.StageSpecificArguments.LoadStage;

        if (!Exists())
            throw new Exception($"The sql bak file {Filepath} does not exist");

        var fileOnlyCommand = $"RESTORE FILELISTONLY FROM DISK = '{Filepath}'";
        using var fileInfo = new DataTable();
        using var con = (SqlConnection)db.Server.GetConnection();
        using (var cmd = new SqlCommand(fileOnlyCommand, con))
        using (var da = new SqlDataAdapter(cmd))
            da.Fill(fileInfo);
        var primaryFiles = fileInfo.Select("Type = 'D'");
        var logFiles = fileInfo.Select("Type = 'L'");
        if (primaryFiles.Length != 1 || logFiles.Length != 1)
        {
            //Something has gone wrong
            return ExitCodeType.Error;
        }


        var primaryFile = primaryFiles[0];
        var logFile = logFiles[0];
        if (string.IsNullOrWhiteSpace(PrimaryFilePhysicalName))
            PrimaryFilePhysicalName = primaryFile["PhysicalName"].ToString();
        if (string.IsNullOrWhiteSpace(LogFilePhysicalName))
            LogFilePhysicalName = logFile["PhysicalName"].ToString();

        if (File.Exists(PrimaryFilePhysicalName) || File.Exists(LogFilePhysicalName))
        {
            var timestamp = DateTime.Now.Millisecond.ToString();
            var primaryFileName = PrimaryFilePhysicalName[..^4];
            var primaryFileExtension = PrimaryFilePhysicalName[^4..];
            PrimaryFilePhysicalName = $"{primaryFileName}_{timestamp}{primaryFileExtension}";
            var logFileName = LogFilePhysicalName[..^4];
            var logFileExtension = LogFilePhysicalName[^4..];
            LogFilePhysicalName = $"{logFileName}_{timestamp}{logFileExtension}";
        }

        var name = db.ToString();

        var restoreCommand = @$"
        use master;
        ALTER DATABASE {name} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        RESTORE DATABASE {name}
        FROM DISK = '{Filepath}'
        WITH MOVE '{primaryFile["LogicalName"]}' TO '{PrimaryFilePhysicalName}',
        MOVE '{logFile["LogicalName"]}' TO '{LogFilePhysicalName}' ,  NOUNLOAD,  REPLACE,  STATS = 5;
        ALTER DATABASE {name} SET MULTI_USER;
        ";

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Executing script {Filepath} ( against {db})"));

        var executer = new ExecuteSqlInDleStage(job, _loadStage);
        return executer.Execute(restoreCommand, db);
    }


    public override bool Exists() => File.Exists(Filepath);

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