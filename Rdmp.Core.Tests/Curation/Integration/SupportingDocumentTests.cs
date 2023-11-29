// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class SupportingDocumentTests : DatabaseTests
{
    [Test]
    public void test_SupportingDocument_CreateAndDestroy()
    {
        var cata = new Catalogue(CatalogueRepository, "deleteme");
        var doc = new SupportingDocument(CatalogueRepository, cata, "davesFile");

        Assert.That(doc.Name, Is.EqualTo("davesFile"));

        doc.DeleteInDatabase();
        cata.DeleteInDatabase();
    }

    [Test]
    public void test_SupportingDocument_CreateChangeSaveDestroy()
    {
        var cata = new Catalogue(CatalogueRepository, "deleteme");
        var doc = new SupportingDocument(CatalogueRepository, cata, "davesFile")
        {
            Description = "some exciting file that dave loves"
        };
        doc.SaveToDatabase();

        Assert.That(doc.Name, Is.EqualTo("davesFile"));
        Assert.That(doc.Description, Is.EqualTo("some exciting file that dave loves"));

        var docAfterCommit = CatalogueRepository.GetObjectByID<SupportingDocument>(doc.ID);

        Assert.That(doc.Description, Is.EqualTo(docAfterCommit.Description));

        doc.DeleteInDatabase();
        cata.DeleteInDatabase();
    }
}