// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution;

/// <summary>
///     Pipeline which processes a single job through all stages before accepting another.  Execution involves running each
///     DataLoadComponent with the current
///     IDataLoadJob and then disposing them.
/// </summary>
public class SingleJobExecution : IDataLoadExecution
{
    public List<IDataLoadComponent> Components { get; set; }


    public SingleJobExecution(List<IDataLoadComponent> components)
    {
        Components = components;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    public ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        job.StartLogging();
        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Starting load for {job.LoadMetadata.Name}"));

        try
        {
            foreach (var component in Components)
            {
                cancellationToken.ThrowIfAbortRequested();

                try
                {
                    //schedule the component for disposal
                    job.PushForDisposal(component);

                    //run current component
                    var exitCodeType = component.Run(job, cancellationToken);

                    //current component failed so jump out, either because load not nessesary or crash
                    if (exitCodeType == ExitCodeType.OperationNotRequired)
                    {
                        TryDispose(exitCodeType, job);
                        //load not nessesary so abort entire DLE process but also cleanup still
                        return exitCodeType;
                    }

                    if (exitCodeType != ExitCodeType.Success)
                        throw new Exception($"Component {component.Description} returned result {exitCodeType}");
                }
                catch (OperationCanceledException e)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"{component.Description} has been cancelled by the user", e));
                    throw;
                }
                catch (Exception e)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                        $"{component.Description} crashed while running Job ", e));
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Job crashed", e));
                    TryDispose(ExitCodeType.Error, job);
                    return ExitCodeType.Error;
                }
            }

            TryDispose(ExitCodeType.Success, job);

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Completed job {job.JobID}"));

            if (job.CrashAtEndMessages.Count > 0)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"There were {job.CrashAtEndMessages.Count} {nameof(IDataLoadJob.CrashAtEndMessages)} registered for job {job.JobID}"));

                // pop the messages into the handler
                foreach (var m in
                         job.CrashAtEndMessages)
                    job.OnNotify(job, m); // depending on the listener these may break flow of control (e.g.
                // return failed (even if the messages are all warnings)
                TryDispose(ExitCodeType.Error, job);
                return ExitCodeType.Error;
            }

            //here
            TryDispose(ExitCodeType.Success, job);
            return ExitCodeType.Success;
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsAbortRequested)
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Job {job.JobID}cancelled in pipeline"));

            TryDispose(cancellationToken.IsAbortRequested ? ExitCodeType.Abort : ExitCodeType.Success, job);
            throw;
        }
    }

    private void TryDispose(ExitCodeType exitCode, IDataLoadJob job)
    {
        try
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Disposing disposables..."));
            job.LoadCompletedSoDispose(exitCode, job);
            job.CloseLogging();
        }
        catch (Exception e)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Job {job.JobID} crashed again during disposing", e));
            throw;
        }
    }
}