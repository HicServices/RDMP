using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Rhino.Mocks.Constraints;
using Tests.Common;

namespace DataExportLibrary.Tests
{
    public class ProjectChecksTestsSimple:DatabaseTests
    {
        [Test]
        public void Project_NoConfigurations()
        {
            Project p = new Project(DataExportRepository, "Fish");

            try
            {
                var ex = Assert.Throws<Exception>(()=>new ProjectChecker(RepositoryLocator,p).Check(new ThrowImmediatelyCheckNotifier()));
                Assert.AreEqual("Project does not have any ExtractionConfigurations yet",ex.Message);

            }
            finally
            {
                p.DeleteInDatabase();
            }
        }

        [Test]
        public void Project_NoDirectory()
        {
            ExtractionConfiguration config;
            Project p = GetProjectWithConfig(out config);
            var ex = Assert.Throws<Exception>(()=>RunTestWithCleanup(p, config));
            Assert.AreEqual("Project does not have an ExtractionDirectory", ex.Message);
            
        }

        [Test]
        [TestCase(@"C:\asdlfasdjlfhasjldhfljh")]
        [TestCase(@"\\MyMakeyUpeyServer\Where")]
        [TestCase(@"Z:\WizardOfOz")]
        public void Project_NonExistentDirectory(string dir)
        {
            ExtractionConfiguration config;
            Project p = GetProjectWithConfig(out config);
           
            p.ExtractionDirectory = dir;
            var ex = Assert.Throws<Exception>(()=>RunTestWithCleanup(p, config));
            StringAssert.Contains(@"Project ExtractionDirectory .* Does Not Exist",ex.Message);

        }

        [Test]
        public void Project_DodgyCharactersInExtractionDirectoryName()
        {
            ExtractionConfiguration config;
            Project p = GetProjectWithConfig(out config);
            p.ExtractionDirectory = @"C:\|||";

            var ex = Assert.Throws<Exception>(()=>RunTestWithCleanup(p,config));
            Assert.AreEqual(@"Project ExtractionDirectory ('C:\|||') is not a valid directory name ", ex.Message);
        }

        [Test]
        public void ConfigurationFrozen_Remnants()
        {
            DirectoryInfo dir;
            ExtractionConfiguration config;
            var p = GetProjectWithConfigDirectory(out config, out dir);

            //create remnant directory (empty)
            var remnantDir = dir.CreateSubdirectory("Extr_" + config.ID + "20011225");
                
            //with empty subdirectories
            remnantDir.CreateSubdirectory("DMPTestCatalogue").CreateSubdirectory("Lookups");

            config.IsReleased = true;//make environment think config is released
            config.SaveToDatabase();

            try
            {
                //remnant exists
                Assert.IsTrue(dir.Exists);
                Assert.IsTrue(remnantDir.Exists);

                //resolve accepting deletion
                new ProjectChecker(RepositoryLocator,p).Check(new AcceptAllCheckNotifier());

                //boom remnant doesnt exist anymore (but parent does obviously)
                Assert.IsTrue(dir.Exists);
                Assert.IsFalse(Directory.Exists(remnantDir.FullName));//cant use .Exists for some reason, c# caches answer?

            }
            finally
            {
                config.DeleteInDatabase();
                p.DeleteInDatabase();
            }
        }


        [Test]
        public void ConfigurationFrozen_RemnantsWithFiles()
        {
            DirectoryInfo dir;
            ExtractionConfiguration config;
            var p = GetProjectWithConfigDirectory(out config, out dir);

            //create remnant directory (empty)
            var remnantDir = dir.CreateSubdirectory("Extr_" + config.ID + "20011225");

            //with empty subdirectories
            var lookupDir = remnantDir.CreateSubdirectory("DMPTestCatalogue").CreateSubdirectory("Lookups");

            //this time put a file in 
            File.AppendAllLines(Path.Combine(lookupDir.FullName,"Text.txt"),new string[]{"Amagad"});
            
            config.IsReleased = true;//make environment think config is released
            config.SaveToDatabase();
            try
            {
                var notifier = new ToMemoryCheckNotifier();
                RunTestWithCleanup(p,config,notifier);

                Assert.IsTrue(notifier.Messages.Any(
                    m=>m.Result == CheckResult.Fail &&
                    Regex.IsMatch(m.Message,@"Found non-empty folder .* which is left over extracted folder after data release \(First file found was '.*\\DMPTestCatalogue\\Lookups\\Text.txt' but there may be others\)")));
            }
            finally
            {
                remnantDir.Delete(true);
            }
        }

        [Test]
        public void Configuration_NoDatasets()
        {
            DirectoryInfo dir;
            ExtractionConfiguration config;
            var p = GetProjectWithConfigDirectory(out config, out dir);
            var ex = Assert.Throws<Exception>(()=>RunTestWithCleanup(p,config));
            Assert.IsTrue(ex.Message.StartsWith("There are no datasets selected for open configuration 'New ExtractionConfiguration"));

        }


        [Test]
        public void Configuration_NoProjectNumber()
        {
            DirectoryInfo dir;
            ExtractionConfiguration config;
            var p = GetProjectWithConfigDirectory(out config, out dir);
            p.ProjectNumber = null;
            var ex = Assert.Throws<Exception>(()=>RunTestWithCleanup(p, config));
            StringAssert.Contains("Project does not have a Project Number, this is a number which is meaningful to you (as opposed to ID which is the ",ex.Message);
        }

        private void RunTestWithCleanup(Project p,ExtractionConfiguration config, ICheckNotifier notifier = null)
        {
            try
            {
                new ProjectChecker(RepositoryLocator,p).Check(notifier??new ThrowImmediatelyCheckNotifier() { ThrowOnWarning = true });
            }
            finally
            {
                config.DeleteInDatabase();
                p.DeleteInDatabase();
            }
        }

        private Project GetProjectWithConfig(out ExtractionConfiguration config)
        {
            var p = new Project(DataExportRepository, "Fish");
            p.ProjectNumber = -5000;
            config = new ExtractionConfiguration(DataExportRepository,p);
            return p;
        }

        private Project GetProjectWithConfigDirectory(out ExtractionConfiguration config,out DirectoryInfo dir)
        {
            var p = new Project(DataExportRepository, "Fish");
            config = new ExtractionConfiguration(DataExportRepository, p);

            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            assemblyFolder = Path.Combine(assemblyFolder, @"\ProjectCheckerTestDir");

            dir = new DirectoryInfo(assemblyFolder );
            p.ExtractionDirectory = assemblyFolder;
            p.ProjectNumber = -5000;
            p.SaveToDatabase();

            return p;
        }
    }
}
