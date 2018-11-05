
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using LoadModules.Generic.DataProvider;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class ImportFilesDataProviderTests:DatabaseTests
    {
        [Test]
        public void CopyFiles()
        {
            var sourceDir = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory).CreateSubdirectory("subdir");
            var targetDir = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory).CreateSubdirectory("loaddir");
            
            //make sure target is empty
            foreach (var f in targetDir.GetFiles())
                f.Delete();
            
            var originpath = Path.Combine(sourceDir.FullName, "myFile.txt");

            File.WriteAllText(originpath,"fish");

            var job = new ThrowImmediatelyDataLoadJob();
            var mockProjectDirectory = MockRepository.GenerateMock<IHICProjectDirectory>();
            mockProjectDirectory.Expect(p => p.ForLoading).Return(targetDir);
            job.HICProjectDirectory = mockProjectDirectory;


            //Create the provider
            var provider = new ImportFilesDataProvider();

            //it doesn't know what to load yet
            Assert.Throws<Exception>(() => provider.Check(new ThrowImmediatelyCheckNotifier()));
            
            //now it does
            provider.DirectoryPath = sourceDir.FullName;

            //but it doesn't have a file pattern
            Assert.Throws<Exception>(() => provider.Check(new ThrowImmediatelyCheckNotifier()));

            //now it does but its not a matching one
            provider.FilePattern = "cannonballs.bat";

            //either way it passes checking
            Assert.DoesNotThrow(() => provider.Check(new ThrowImmediatelyCheckNotifier()));

            //execute the provider
            provider.Fetch(job, new GracefulCancellationToken());

            //destination is empty because nothing matched 
            Assert.IsEmpty(targetDir.GetFiles());

            //give it correct pattern
            provider.FilePattern = "*.txt";

            //execute the provider
            provider.Fetch(job, new GracefulCancellationToken());

            //both files should exist
            Assert.AreEqual(1,targetDir.GetFiles().Count());
            Assert.AreEqual(1, sourceDir.GetFiles().Count());

            //simulate load failure
            provider.LoadCompletedSoDispose(ExitCodeType.Abort, new ThrowImmediatelyDataLoadJob());

            //both files should exist
            Assert.AreEqual(1, targetDir.GetFiles().Count());
            Assert.AreEqual(1, sourceDir.GetFiles().Count());

            //simulate load success
            provider.LoadCompletedSoDispose(ExitCodeType.Success, new ThrowImmediatelyDataLoadJob());

            //both files should exist because Delete on success is false
            Assert.AreEqual(1, targetDir.GetFiles().Count());
            Assert.AreEqual(1, sourceDir.GetFiles().Count());

            //change behaviour to delete on successful data loads
            provider.DeleteFilesOnsuccessfulLoad = true;

            //simulate load failure
            provider.LoadCompletedSoDispose(ExitCodeType.Error, new ThrowImmediatelyDataLoadJob());

            //both files should exist
            Assert.AreEqual(1, targetDir.GetFiles().Count());
            Assert.AreEqual(1, sourceDir.GetFiles().Count());

            //simulate load success
            provider.LoadCompletedSoDispose(ExitCodeType.Success, new ThrowImmediatelyDataLoadJob());

            //only forLoading file should exist (in real life that one would be handled by archivng already)
            Assert.AreEqual(1, targetDir.GetFiles().Count());
            Assert.AreEqual(0, sourceDir.GetFiles().Count());

        }

    }
}
