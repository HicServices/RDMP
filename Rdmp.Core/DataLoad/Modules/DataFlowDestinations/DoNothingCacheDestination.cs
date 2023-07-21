// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Threading;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowDestinations;

/// <summary>
///     Cache destination component which creates 10 files, one per minute, in the CacheDirectory. It can use a
///     DoNothingCacheChunk if told to run multiple runs.
///     It will respect Stop and Abort commands. Can be used by user to build a caching pipeline even when there is nothing
///     to do.  Basically wraps
///     BasicCacheLayout so that you can read from the cache even though you have no valid pipeline for writing to it.  Use
///     this destination only if you have
///     some bespoke process for populating / updating the cache progress and you only want a caching pipeline to exist for
///     validation reasons not to actually
///     run it.
/// </summary>
public class DoNothingCacheDestination : CacheFilesystemDestination
{
    public override ICacheChunk ProcessPipelineData(ICacheChunk toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        //if(toProcess != null)
        //    throw new NotSupportedException("Expected only to be passed null chunks or never to get called, this destination is not valid for use when sources are actually sending/reading data");


        var run = 0;
        if (toProcess is DoNothingCacheChunk chunk) run = chunk.RunIteration;

        for (var i = 0; i < 10; i++)
        {
            File.WriteAllText(Path.Combine(CacheDirectory.FullName, $"run {run} - loop {i}.txt"),
                DateTime.Now.ToString("O"));
            if (cancellationToken.IsCancellationRequested)
                return null;

            Thread.Sleep(60000);
        }

        return null;
    }

    public override ICacheLayout CreateCacheLayout()
    {
        return new BasicCacheLayout(CacheDirectory);
    }

    public override void Abort(IDataLoadEventListener listener)
    {
    }
}