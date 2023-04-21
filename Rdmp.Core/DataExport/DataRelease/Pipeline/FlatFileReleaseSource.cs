// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataRelease.Pipeline;

/// <summary>
/// Prepares the Source Global Folder for the ReleaseEngine.
/// </summary>
/// <typeparam name="T">The ReleaseAudit object passed around in the pipeline</typeparam>
public class FlatFileReleaseSource<T> : FixedReleaseSource<ReleaseAudit>
{
    protected override ReleaseAudit GetChunkImpl(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        return flowData ?? new ReleaseAudit()
        {
            SourceGlobalFolder = PrepareSourceGlobalFolder()
        };
    }

    public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        firstTime = true;
    }

    public override void Abort(IDataLoadEventListener listener)
    {
        firstTime = true;
    }

    protected override void RunSpecificChecks(ICheckNotifier notifier, bool isRunTime)
    {
            
    }

    protected override DirectoryInfo PrepareSourceGlobalFolder()
    {
        if (_releaseData.ReleaseGlobals)
            return base.PrepareSourceGlobalFolder();

        return null;
    }
}