// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline;

/// <summary>
///     Extraction component for extracting (and optionally transforming in some way) arbitrary files on disk as part of an
///     extraction
/// </summary>
public abstract class FileExtractor : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<IExtractCommand>
{
    protected ExtractGlobalsCommand _command { get; set; }

    private bool _isFirstTime = true;

    public virtual void Abort(IDataLoadEventListener listener)
    {
    }

    public virtual void Check(ICheckNotifier notifier)
    {
    }

    public virtual void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
    {
        // We only want to extract the files once so let's do it as part of extracting globals
        _command = value as ExtractGlobalsCommand;
    }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        if (_command == null)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    $"Ignoring {GetType().Name} component because command is not ExtractGlobalsCommand"));
            return toProcess;
        }

        if (_isFirstTime)
            MoveFiles(_command, listener, cancellationToken);

        _isFirstTime = false;

        return toProcess;
    }

    /// <summary>
    ///     Gets called once only per extraction pipeline run (at the time globals start being extracted)
    /// </summary>
    /// <param name="command"></param>
    /// <param name="listener"></param>
    /// <param name="cancellationToken"></param>
    protected abstract void MoveFiles(ExtractGlobalsCommand command, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken);
}