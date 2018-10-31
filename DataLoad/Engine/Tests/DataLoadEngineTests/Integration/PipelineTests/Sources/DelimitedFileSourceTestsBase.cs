using System;
using System.Data;
using System.IO;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using LoadModules.Generic.DataFlowSources;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;

namespace DataLoadEngineTests.Integration.PipelineTests.Sources
{
    public abstract class DelimitedFileSourceTestsBase
    {
        protected FlatFileToLoad CreateTestFile(params string[] contents)
        {
            var filename = Path.Combine(TestContext.CurrentContext.WorkDirectory, "DelimitedFileSourceTests.txt");

            if (File.Exists(filename))
                File.Delete(filename);

            File.WriteAllLines(filename, contents);

            return new FlatFileToLoad(new FileInfo(filename));
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
            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            source.PreInitialize(file, new ThrowImmediatelyDataLoadEventListener());
            source.Separator = ",";
            source.StronglyTypeInput = true;//makes the source interpret the file types properly
            source.StronglyTypeInputBatchSize = 100;

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
}