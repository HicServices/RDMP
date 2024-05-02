// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataRelease.Pipeline;

/// <summary>
///     To be used at design time only. Using this in runtime will generate Not Implemented Exceptions.
/// </summary>
public class NullReleaseSource : FixedReleaseSource<ReleaseAudit>
{
    protected override ReleaseAudit GetChunkImpl(IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        throw new NotImplementedException();
    }

    public override void Abort(IDataLoadEventListener listener)
    {
        throw new NotImplementedException();
    }

    protected override void RunSpecificChecks(ICheckNotifier notifier, bool isRunTime)
    {
    }

    public override string ToString()
    {
        return "Fixed Release Source";
    }
}