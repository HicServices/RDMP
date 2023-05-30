// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Sources;

[Category("Unit")]
public abstract class DelimitedFileSourceTestsBase
{
    protected FlatFileToLoad CreateTestFile(params string[] contents)
    {
        var filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "DelimitedFileSourceTests.txt");

        if (File.Exists(filename))
            File.Delete(filename);

        File.WriteAllLines(filename, contents);

        return new FlatFileToLoad(new FileInfo(filename));
    }

    protected void AssertDivertFileIsExactly(string expectedContents)
    {
        var filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "DelimitedFileSourceTests_Errors.txt");

        if(!File.Exists(filename))
            Assert.Fail($"No Divert file was generated at expected path {filename}");

        var contents = File.ReadAllText(filename);
        Assert.AreEqual(expectedContents, contents);
    }


    protected DataTable RunGetChunk(FlatFileToLoad file,BadDataHandlingStrategy strategy, bool throwOnEmpty)
    {
        return RunGetChunk(file, s =>
        {
            s.BadDataHandlingStrategy = strategy;
            s.ThrowOnEmptyFiles = throwOnEmpty;
        });
    }

    protected DataTable RunGetChunk(FlatFileToLoad file, Action<DelimitedFlatFileDataFlowSource> adjust = null)
    {
        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(file, new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.StronglyTypeInput = true;//makes the source interpret the file types properly
        source.StronglyTypeInputBatchSize = 100;
        source.AttemptToResolveNewLinesInRecords = true; //maximise potential for conflicts
        if (adjust != null)
            adjust(source);

        try
        {
            return source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }

    }
}