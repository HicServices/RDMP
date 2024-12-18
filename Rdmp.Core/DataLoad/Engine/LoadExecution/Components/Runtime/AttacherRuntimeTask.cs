// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
/// RuntimeTask that hosts an IAttacher.  The instance is hydrated from the user's configuration (ProcessTask and ProcessTaskArguments) See
/// RuntimeArgumentCollection
/// </summary>
public class AttacherRuntimeTask : RuntimeTask, IMEFRuntimeTask
{
    public IAttacher Attacher { get; private set; }
    public ICheckable MEFPluginClassInstance => Attacher;

    public AttacherRuntimeTask(IProcessTask task, RuntimeArgumentCollection args)
        : base(task, args)
    {
        //All attachers must be marked as mounting stages, and therefore we can pull out the RAW Server and Name
        var mountingStageArgs = args.StageSpecificArguments;
        if (mountingStageArgs.LoadStage != LoadStage.Mounting)
            throw new Exception("AttacherRuntimeTask can only be called as a Mounting stage process");

        if (string.IsNullOrWhiteSpace(task.Path))
            throw new ArgumentException(
                $"Path is blank for ProcessTask '{task}' - it should be a class name of type {nameof(IAttacher)}");

        Attacher = MEF.CreateA<IAttacher>(ProcessTask.Path);

        SetPropertiesForClass(RuntimeArguments, Attacher);
        Attacher.Initialize(args.StageSpecificArguments.RootDir, RuntimeArguments.StageSpecificArguments.DbInfo);
    }


    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        var beforeServer = RuntimeArguments.StageSpecificArguments.DbInfo.Server.Name;
        var beforeDatabase =
            RuntimeArguments.StageSpecificArguments.DbInfo.Server.GetCurrentDatabase().GetRuntimeName();
        var beforeDatabaseType = RuntimeArguments.StageSpecificArguments.DbInfo.Server.DatabaseType;

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"About to run Task '{ProcessTask.Name}'"));
        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Attacher class is:{Attacher.GetType().FullName}"));

        try
        {
            return Attacher.Attach(job, cancellationToken);
        }
        catch (Exception e)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Attach failed on job {job} Attacher was of type {Attacher.GetType().Name} see InnerException for specifics",
                e));
            return ExitCodeType.Error;
        }
        finally
        {
            var afterServer = RuntimeArguments.StageSpecificArguments.DbInfo.Server.Name;
            var afterDatabase = RuntimeArguments.StageSpecificArguments.DbInfo.Server.GetCurrentDatabase()
                .GetRuntimeName();
            var afterDatabaseType = RuntimeArguments.StageSpecificArguments.DbInfo.Server.DatabaseType;

            if (!(beforeServer.Equals(afterServer) && beforeDatabase.Equals(afterDatabase) &&
                  beforeDatabaseType == afterDatabaseType))
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Attacher {Attacher.GetType().Name} modified the ConnectionString during attaching"));
        }
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        Attacher.LoadCompletedSoDispose(exitCode, postLoadEventListener);
    }


    public override bool Exists()
    {
        var className = ProcessTask.Path;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var type = assembly.GetTypes().FirstOrDefault(t => t.FullName == className);
            if (type != null) return true;
        }

        return false;
    }

    public override void Abort(IDataLoadEventListener postLoadEventListener)
    {
        LoadCompletedSoDispose(ExitCodeType.Abort, postLoadEventListener);
    }

    public override void Check(ICheckNotifier checker)
    {
        new MandatoryPropertyChecker(Attacher).Check(checker);
        Attacher.Check(checker);
    }
}