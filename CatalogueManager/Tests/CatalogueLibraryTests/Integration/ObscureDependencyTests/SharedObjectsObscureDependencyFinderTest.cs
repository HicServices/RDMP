using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.ObscureDependencyTests
{
    public class SharedObjectsObscureDependencyFinderTest:DatabaseTests
    {
        [Test]
        public void CannotDeleteSharedObjectTest()
        {
            //create a test catalogue
            Catalogue c = new Catalogue(CatalogueRepository,"blah");
            
            Assert.IsFalse(CatalogueRepository.IsExportedObject(c));

            //make it exportable
            var exportDefinition = CatalogueRepository.GetExportFor(c);
            
            Assert.IsTrue(CatalogueRepository.IsExportedObject(c));

            //cannot delete because object is shared externally
            Assert.Throws<Exception>(c.DeleteInDatabase);

            //no longer exportable
            exportDefinition.DeleteInDatabase();

            //no longer shared
            Assert.IsFalse(CatalogueRepository.IsExportedObject(c));

            //now we can delete it
            c.DeleteInDatabase();
        }

        [Test]
        public void CascadeDeleteImportDefinitions()
        {
            Project p = new Project(DataExportRepository, "prah");

            var exportDefinition = CatalogueRepository.GetExportFor(p);

            Project p2 = new Project(DataExportRepository, "prah2");

            var importDefinition = new ObjectImport(CatalogueRepository, exportDefinition.SharingUID, p2);

            //import definition exists
            Assert.IsTrue(importDefinition.Exists());

            //delete local import
            p2.DeleteInDatabase();

            //cascade should have deleted the import definition since the imported object version is gone
            Assert.IsFalse(importDefinition.Exists());

            //clear up the exported version too 
            exportDefinition.DeleteInDatabase();
            p.DeleteInDatabase();
        }
    }
}
