// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Tests.Common.Helpers;

public sealed class TestDataWriter : CacheFilesystemDestination
{
    private TestDataWriterChunk ProcessPipelineData(TestDataWriterChunk toProcess)
    {
        var layout = CreateCacheLayout();

        var toCreateFilesIn = layout.Resolver.GetLoadCacheDirectory(CacheDirectory);

        foreach (var file in toProcess.Files)
        {
            var destination = Path.Combine(toCreateFilesIn.FullName, file.Name);

            if (File.Exists(destination))
                File.Delete(destination);

            file.MoveTo(destination);
        }

        return null;
    }

    public override ICacheChunk ProcessPipelineData(ICacheChunk toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken) =>
        ProcessPipelineData((TestDataWriterChunk)toProcess);

    public override ICacheLayout CreateCacheLayout() => new BasicCacheLayout(CacheDirectory);

    public override void Abort(IDataLoadEventListener listener)
    {
    }

    public override void Check(ICheckNotifier notifier)
    {
        if (CacheDirectory == null)
            notifier.OnCheckPerformed(new CheckEventArgs("PreInitialize was not called? (CacheDirectory == null)",
                CheckResult.Fail));
    }
}