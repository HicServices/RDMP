using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Checks;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    
    public class MEFCheckerTests:DatabaseTests
    {
        [Test]
        [ExpectedException(ExpectedMessage = "MEFChecker was asked to check for the existence of an Export class but the _classToFind string was empty")]
        public void FindClass_EmptyString()
        {
            MEFChecker m = new MEFChecker(CatalogueRepository.MEF, "", s => Assert.Fail()); 
            m.Check(new ThrowImmediatelyCheckNotifier());
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
        [ExpectedException(ExpectedMessage = "Could not find MEF class called CatalogueLibrary.UncleSam in LoadModuleAssembly.GetAllTypes() and couldn't even find any with the same basic name",MatchType = MessageMatch.Contains)]
        public void FindClass_NonExistant()
        {
            MEFChecker m = new MEFChecker(CatalogueRepository.MEF, "CatalogueLibrary.UncleSam", s => Assert.Fail());
            m.Check(new ThrowImmediatelyCheckNotifier());

        }


    }
}
