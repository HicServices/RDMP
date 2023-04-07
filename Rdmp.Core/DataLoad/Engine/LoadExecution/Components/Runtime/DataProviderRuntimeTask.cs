// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
/// RuntimeTask that hosts an IDataProvider.  The instance is hydrated from the users configuration (ProcessTask and ProcessTaskArguments) See
/// RuntimeArgumentCollection
/// </summary>
public class DataProviderRuntimeTask : RuntimeTask, IMEFRuntimeTask
{
    public IDataProvider Provider { get; private set; }
    public ICheckable MEFPluginClassInstance { get { return Provider; } }

    public DataProviderRuntimeTask(IProcessTask task, RuntimeArgumentCollection args, MEF mef)
        : base(task, args)
    {
        string classNameToInstantiate = task.Path;

        if (string.IsNullOrWhiteSpace(task.Path))
            throw new ArgumentException("Path is blank for ProcessTask '" + task + "' - it should be a class name of type " + typeof(IDataProvider).Name);

        Provider = mef.CreateA<IDataProvider>(classNameToInstantiate);

        try
        {
            SetPropertiesForClass(RuntimeArguments, Provider);
        }
        catch (Exception e)
        {
            throw new Exception("Error when trying to set the properties for '" + task.Name + "'", e);
        }

        Provider.Initialize(args.StageSpecificArguments.RootDir, RuntimeArguments.StageSpecificArguments.DbInfo);
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to run Task '" + ProcessTask.Name + "'"));

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to fetch data using class " + Provider.GetType().FullName));

        return Provider.Fetch(job, cancellationToken);
    }

    public override bool Exists()
    {
        return true;
    }
        
    public override void Abort(IDataLoadEventListener postLoadEventListener)
    {
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
    {
        Provider.LoadCompletedSoDispose(exitCode,postLoadEventListener);
    }

    public override void Check(ICheckNotifier checker)
    {
        new MandatoryPropertyChecker(Provider).Check(checker);
        Provider.Check(checker);
    }
}