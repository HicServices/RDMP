// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Startup;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.ObscureDependencyTests
{
    public class BetweenCatalogueAndDataExportObscureDependencyFinderTests : DatabaseTests
    {
        [Test]
        public void PreventDeletingCatalogueBecauseOfLinkedDatasetTest()
        {
            var obscura = new BetweenCatalogueAndDataExportObscureDependencyFinder(RepositoryLocator);
            var cata = new Catalogue(CatalogueRepository, "MyCata");

            //catalogue exists in isolation so is deletable
            Assert.DoesNotThrow(()=>obscura.ThrowIfDeleteDisallowed(cata));

            //there is a new dataset which is linked to Catalogue
            var dataset = new ExtractableDataSet(DataExportRepository,cata);
            
            //and suddenly we cannot delete the catalogue
            var ex = Assert.Throws<Exception>(() => obscura.ThrowIfDeleteDisallowed(cata));
            Assert.IsTrue(ex.Message.Contains("Cannot delete Catalogue MyCata because there are ExtractableDataSets which depend on them "));

            //also if we try to force through a delete it should behave in identical manner
            var ex2 = Assert.Throws<Exception>(cata.DeleteInDatabase);
            Assert.IsTrue(ex2.Message.Contains("Cannot delete Catalogue MyCata because there are ExtractableDataSets which depend on them "));

            //now we delete the linked dataset
            dataset.DeleteInDatabase();

            //and because there is now no longer a dataset dependency on the catalogue we can delete it
            Assert.DoesNotThrow(() => obscura.ThrowIfDeleteDisallowed(cata));
            
            //and the delete works too
            cata.DeleteInDatabase();

            //both objects still exist in memory of course but we should be able to see they have disapeared 
            Assert.IsTrue(dataset.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted);
            Assert.IsTrue(cata.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted);
        }

        [Test]
        public void AllowDeletingWhenDataExportManagerIsNotSet()
        {
            var noDataExportManagerExists = new LinkedRepositoryProvider(CatalogueTableRepository.ConnectionString,null);

            var obscura1 = new BetweenCatalogueAndDataExportObscureDependencyFinder(RepositoryLocator);
            var obscura2 = new BetweenCatalogueAndDataExportObscureDependencyFinder(noDataExportManagerExists);

            var cata = new Catalogue(CatalogueRepository, "MyCata");
            var dataset = new ExtractableDataSet(DataExportRepository,cata);

            //we cannot delete it because there is a dependency
            var ex = Assert.Throws<Exception>(() => obscura1.ThrowIfDeleteDisallowed(cata));
            Assert.IsTrue(ex.Message.Contains("Cannot delete Catalogue MyCata because there are ExtractableDataSets which depend on them "));

            //the second finder simulates when the repository locator doesn't have a record of the data export repository so it is unable to check it so it will let you delete it just fine
            Assert.DoesNotThrow(() => obscura2.ThrowIfDeleteDisallowed(cata));

            //now delete them in the correct order
            dataset.DeleteInDatabase();
            cata.DeleteInDatabase();

        }
    }
}
