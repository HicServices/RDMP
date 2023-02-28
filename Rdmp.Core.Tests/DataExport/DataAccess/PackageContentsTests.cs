// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport.DataAccess
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


                var packageContents = DataExportRepository;

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
