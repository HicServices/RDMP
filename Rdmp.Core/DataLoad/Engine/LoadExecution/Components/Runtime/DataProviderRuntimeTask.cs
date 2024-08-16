// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
/// RuntimeTask that hosts an IDataProvider.  The instance is hydrated from the users configuration (ProcessTask and ProcessTaskArguments) See
/// RuntimeArgumentCollection
/// </summary>
public class DataProviderRuntimeTask : RuntimeTask, IMEFRuntimeTask
{
    public IDataProvider Provider { get; private set; }
    public ICheckable MEFPluginClassInstance => Provider;

    private IBasicActivateItems _activator;

    public DataProviderRuntimeTask(IProcessTask task, RuntimeArgumentCollection args)
        : base(task, args)
    {
        var classNameToInstantiate = task.Path;

        if (string.IsNullOrWhiteSpace(task.Path))
            throw new ArgumentException(
                $"Path is blank for ProcessTask '{task}' - it should be a class name of type {nameof(IDataProvider)}");

        Provider = MEF.CreateA<IDataProvider>(classNameToInstantiate);
        if(Provider is IInteractiveCheckable)
        {
            ((IInteractiveCheckable)Provider).SetActivator(_activator);
        }

        try
        {
            SetPropertiesForClass(RuntimeArguments, Provider);
        }
        catch (Exception e)
        {
            throw new Exception($"Error when trying to set the properties for '{task.Name}'", e);
        }

        Provider.Initialize(args.StageSpecificArguments.RootDir, RuntimeArguments.StageSpecificArguments.DbInfo);
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        job.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, $"About to run Task '{ProcessTask.Name}'"));

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"About to fetch data using class {Provider.GetType().FullName}"));
        if (Provider is IInteractiveCheckable)
        {
            ((IInteractiveCheckable)Provider).SetActivator(_activator);
        }
        return Provider.Fetch(job, cancellationToken);
    }

    public override bool Exists() => true;

    public override void Abort(IDataLoadEventListener postLoadEventListener)
    {
    }

    public void SetActivator(IBasicActivateItems activator)
    {
        _activator = activator;
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        Provider.LoadCompletedSoDispose(exitCode, postLoadEventListener);
    }

    public override void Check(ICheckNotifier checker)
    {
        if (Provider is IInteractiveCheckable)
        {
            ((IInteractiveCheckable)Provider).SetActivator(_activator);
        }
        new MandatoryPropertyChecker(Provider).Check(checker);
        Provider.Check(checker);
    }
}