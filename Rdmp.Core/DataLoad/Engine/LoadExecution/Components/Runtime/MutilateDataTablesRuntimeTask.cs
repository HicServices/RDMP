// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
///     RuntimeTask that hosts an IMutilateDataTables.  The instance is hydrated from the users configuration (ProcessTask
///     and ProcessTaskArguments) See
///     RuntimeArgumentCollection
/// </summary>
public class MutilateDataTablesRuntimeTask : RuntimeTask, IMEFRuntimeTask
{
    //the class (built by reflection) that will do all the heavy lifting
    public IMutilateDataTables MutilateDataTables { get; set; }
    public ICheckable MEFPluginClassInstance => MutilateDataTables;

    public MutilateDataTablesRuntimeTask(IProcessTask task, RuntimeArgumentCollection args)
        : base(task, args)
    {
        //All attachers must be marked as mounting stages, and therefore we can pull out the RAW Server and Name
        var stageArgs = args.StageSpecificArguments ?? throw new NullReferenceException("Stage args was null");
        if (stageArgs.DbInfo == null)
            throw new NullReferenceException(
                "Stage args had no DbInfo, unable to mutilate tables without a database - mutilator is sad");

        if (string.IsNullOrWhiteSpace(task.Path))
            throw new ArgumentException(
                $"Path is blank for ProcessTask '{task}' - it should be a class name of type {nameof(IMutilateDataTables)}");

        MutilateDataTables = MEF.CreateA<IMutilateDataTables>(ProcessTask.Path);
        SetPropertiesForClass(RuntimeArguments, MutilateDataTables);
        MutilateDataTables.Initialize(stageArgs.DbInfo, ProcessTask.LoadStage);
    }


    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        job.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, $"About to run Task '{ProcessTask.Name}'"));
        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Mutilate class is{MutilateDataTables.GetType().FullName}"));

        try
        {
            return MutilateDataTables.Mutilate(job);
        }
        catch (Exception e)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Mutilate failed on job {job} Mutilator was of type {MutilateDataTables.GetType().Name} see InnerException for specifics",
                e));
            return ExitCodeType.Error;
        }
    }

    public override bool Exists()
    {
        return true;
    }

    public override void Abort(IDataLoadEventListener postLoadEventListener)
    {
        LoadCompletedSoDispose(ExitCodeType.Abort, postLoadEventListener);
    }

    public override void Check(ICheckNotifier checker)
    {
        new MandatoryPropertyChecker(MutilateDataTables).Check(checker);
        MutilateDataTables.Check(checker);
    }


    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postDataLoadEventListener)
    {
        MutilateDataTables.LoadCompletedSoDispose(exitCode, postDataLoadEventListener);
    }
}