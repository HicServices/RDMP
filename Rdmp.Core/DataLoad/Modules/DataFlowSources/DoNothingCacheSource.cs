// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Caching.Pipeline.Sources;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources;

/// <summary>
///     Cache source component which does nothing.  Can be used by user to build a caching pipeline even when there is
///     nothing to do.  Use this source only if
///     you have some bespoke process for populating / updating the cache progress and you only want a caching pipeline to
///     exist for validation reasons not to
///     actually run it.
/// </summary>
public class DoNothingCacheSource : CacheSource<ICacheChunk>
{
    private int runs;

    public override ICacheChunk DoGetChunk(ICacheFetchRequest request, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        //Data is never available for download
        if (runs < 10)
        {
            runs++;

            return new DoNothingCacheChunk(CatalogueRepository)
            {
                RunIteration = runs
            };
        }

        return null;
    }

    public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public override void Abort(IDataLoadEventListener listener)
    {
    }

    public override ICacheChunk TryGetPreview()
    {
        return null;
    }

    public override void Check(ICheckNotifier notifier)
    {
        notifier.OnCheckPerformed(
            new CheckEventArgs(
                "This Cache Source will never find new data available since it is there only for testing purposes and so you can set up a valid Caching pipeline configuration even if it doesn't do anything (e.g. for use in a hacky manner with DoNothingCacheDestination)",
                CheckResult.Warning));
    }
}