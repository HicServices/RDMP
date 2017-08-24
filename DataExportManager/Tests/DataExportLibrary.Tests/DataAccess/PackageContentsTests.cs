using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using NUnit.Framework;
using Tests.Common;

namespace DataExportLibrary.Tests.DataAccess
{
    public class PackageContentsTests:DatabaseTests
    {
        [Test]
        public void AddAndRemove()
        {
            var cata = new Catalogue(CatalogueRepository, "PackageContentsTests");

            var ds = new ExtractableDataSet(DataExportRepository,cata);

            var package = new ExtractableDataSetPackage(DataExportRepository, "My Cool Package");
            try
            {
                Assert.AreEqual("My Cool Package",package.Name);
                package.Name = "FishPackage";
                package.SaveToDatabase();


                var packageContents = new ExtractableDataSetPackageContents(DataExportRepository);

                var results = packageContents.GetAllDataSets(package, null);
                Assert.AreEqual(0,results.Length);

                packageContents.AddDataSetToPackage(package,ds);

                results = packageContents.GetAllDataSets(package, DataExportRepository.GetAllObjects<ExtractableDataSet>());
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(ds,results[0]);

                packageContents.RemoveDataSetFromPackage(package,ds);

                //cannot delete the relationship twice
                Assert.Throws<ArgumentException>(() => packageContents.RemoveDataSetFromPackage(package, ds));
            }
            finally
            {
                ds.DeleteInDatabase();
                package.DeleteInDatabase();
                cata.DeleteInDatabase();
            }
        }
    }
}
