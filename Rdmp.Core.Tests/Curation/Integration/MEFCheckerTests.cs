// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using NUnit.Framework;
using Rdmp.Core.Curation.Checks;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    
    public class MEFCheckerTests:UnitTests
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            SetupMEF();
        }

        [Test]
        public void FindClass_WrongCase_FoundAnyway()
        {
            Assert.AreEqual(typeof(Catalogue),Repository.MEF.GetType("catalogue"));
        }

        [Test]
        public void FindClass_EmptyString()
        {
            MEFChecker m = new MEFChecker(Repository.MEF, "", s => Assert.Fail()); 
            var ex =  Assert.Throws<Exception>(()=>m.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("MEFChecker was asked to check for the existence of an Export class but the _classToFind string was empty",ex.Message);
        }

        [Test]
        public void FindClass_CorrectNamespace()
        {
            MEFChecker m = new MEFChecker(Repository.MEF, "Rdmp.Core.DataLoad.Modules.Attachers.AnySeparatorFileAttacher", s => Assert.Fail());
            m.Check(new ThrowImmediatelyCheckNotifier());
        }

        [Test]
        public void FindClass_WrongNamespace()
        {
            MEFChecker m = new MEFChecker(Repository.MEF, "CatalogueLibrary.AnySeparatorFileAttacher", s => Assert.Pass());
            m.Check(new AcceptAllCheckNotifier());

            Assert.Fail("Expected the class not to be found but to be identified under the correct namespace (above)");
        }

        [Test]
        public void FindClass_NonExistant()
        {
            MEFChecker m = new MEFChecker(Repository.MEF, "CatalogueLibrary.UncleSam", s => Assert.Fail());
            var ex = Assert.Throws<Exception>(()=>m.Check(new ThrowImmediatelyCheckNotifier()));
            StringAssert.Contains("Could not find MEF class called CatalogueLibrary.UncleSam in LoadModuleAssembly.GetAllTypes() and couldn't even find any with the same basic name",ex.Message);
        }

        [Test]
        public void DllFileDuplication_Ignored()
        {
            // Setup 2 directories that will contain duplicate copies of the same dll
            var badDir1 = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,"Bad1"));
            var badDir2 = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Bad2"));
            
            if (badDir1.Exists)
                badDir1.Delete(true);
            
            badDir1.Create();

            if (badDir2.Exists)
                badDir2.Delete(true);

            badDir2.Create();

            var dllToCopy = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,"Rdmp.Core.dll"));

            // copy the dll to both folders
            File.Copy(dllToCopy.FullName, Path.Combine(badDir1.FullName,"Rdmp.Core.dll"));
            File.Copy(dllToCopy.FullName, Path.Combine(badDir2.FullName, "Rdmp.Core.dll"));

            var tomem = new ToMemoryCheckNotifier();

            var sdc = new SafeDirectoryCatalog(tomem, badDir1.FullName,badDir2.FullName);

            Assert.AreEqual(sdc .DuplicateDllsIgnored, 1);
            
            badDir1.Delete(true);
            badDir2.Delete(true);

        }



    }
}
