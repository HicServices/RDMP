using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using NUnit.Framework;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    
    public class MEFCheckerTests:DatabaseTests
    {
        [Test]
        public void FindClass_EmptyString()
        {
            MEFChecker m = new MEFChecker(CatalogueRepository.MEF, "", s => Assert.Fail()); 
            var ex =  Assert.Throws<Exception>(()=>m.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("MEFChecker was asked to check for the existence of an Export class but the _classToFind string was empty",ex.Message);
        }

        [Test]
        public void FindClass_CorrectNamespace()
        {
            MEFChecker m = new MEFChecker(CatalogueRepository.MEF, "LoadModules.Generic.Attachers.AnySeparatorFileAttacher", s => Assert.Fail());
            m.Check(new ThrowImmediatelyCheckNotifier());
        }

        [Test]
        public void FindClass_WrongNamespace()
        {
            MEFChecker m = new MEFChecker(CatalogueRepository.MEF, "CatalogueLibrary.AnySeparatorFileAttacher", s => Assert.Pass());
            m.Check(new AcceptAllCheckNotifier());

            Assert.Fail("Expected the class not to be found but to be identified under the correct namespace (above)");
        }

        [Test]
        public void FindClass_NonExistant()
        {
            MEFChecker m = new MEFChecker(CatalogueRepository.MEF, "CatalogueLibrary.UncleSam", s => Assert.Fail());
            var ex = Assert.Throws<Exception>(()=>m.Check(new ThrowImmediatelyCheckNotifier()));
            StringAssert.Contains("Could not find MEF class called CatalogueLibrary.UncleSam in LoadModuleAssembly.GetAllTypes() and couldn't even find any with the same basic name",ex.Message);


        }

        [Test]
        public void FileDuplication()
        {
            var badDir = new DirectoryInfo("Bad");

            if(badDir.Exists)
                badDir.Delete(true);
            
            badDir.Create();

            var dllToCopy = new FileInfo("LoadModules.Generic.dll");

            File.Copy(dllToCopy.FullName, Path.Combine(badDir.FullName,"LoadModules.Generic.dll"));

            var tomem = new ToMemoryCheckNotifier();

            new SafeDirectoryCatalog(tomem, Environment.CurrentDirectory);
            var warnings  = tomem.Messages.Where(m => m.Result == CheckResult.Warning).ToArray();

            Assert.GreaterOrEqual(warnings.Count(m => m.Message.StartsWith("Found 2 copies of")), 1);
            

            badDir.Delete(true);

        }



    }
}
