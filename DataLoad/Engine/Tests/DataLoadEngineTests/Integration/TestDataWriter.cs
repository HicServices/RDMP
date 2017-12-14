using System.IO;
using CachingEngine.BasicCache;
using CachingEngine.Layouts;
using CachingEngine.PipelineExecution.Destinations;
using CachingEngine.Requests;
using CatalogueLibrary.DataFlowPipeline;
using RDMPAutomationServiceTests.AutomationLoopTests.FictionalCache;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngineTests.Integration
{
    public class TestDataWriter : CacheFilesystemDestination
    {
        public TestDataWritterChunk ProcessPipelineData(TestDataWritterChunk toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            var layout = CreateCacheLayout();

            var toCreateFilesIn = layout.Resolver.GetLoadCacheDirectory(CacheDirectory);
            
            foreach (FileInfo file in toProcess.Files)
            {
                string destination = Path.Combine(toCreateFilesIn.FullName, file.Name);

                if(File.Exists(destination))
                    File.Delete(destination);

                file.MoveTo(destination);
            }

            return null;
        }

        public override ICacheChunk ProcessPipelineData(ICacheChunk toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            return ProcessPipelineData((TestDataWritterChunk)toProcess, listener, cancellationToken);
        }

        public override ICacheLayout CreateCacheLayout()
        {
            return new BasicCacheLayout(CacheDirectory);
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            
        }

        public override void Check(ICheckNotifier notifier)
        {
            if (CacheDirectory == null)
                notifier.OnCheckPerformed(new CheckEventArgs("PreInitialize was not called? (CacheDirectory == null)", CheckResult.Fail));
        }

    }
}
