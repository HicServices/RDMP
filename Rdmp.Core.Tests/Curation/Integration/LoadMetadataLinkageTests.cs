// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using System.Linq;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class LoadMetadataLinkageTests: DatabaseTests
{
    [Test]
    public void BasicLoadMetadataLinkage()
    {
        var loadMetadata = new LoadMetadata(CatalogueRepository);
        var catalogue = new Catalogue(CatalogueRepository, "catalogue");
        loadMetadata.LinkToCatalogue(catalogue);
        Assert.That(catalogue.LoadMetadatas().Length, Is.EqualTo(1));
        Assert.That(catalogue.LoadMetadatas().First(), Is.EqualTo(loadMetadata));
        Assert.That(loadMetadata.GetAllCatalogues().ToArray().Length, Is.EqualTo(1));
    }

    [Test]
    public void BasicLoadMetadataLinkageUnlink()
    {
        var loadMetadata = new LoadMetadata(CatalogueRepository);
        var catalogue = new Catalogue(CatalogueRepository, "catalogue");
        loadMetadata.LinkToCatalogue(catalogue);
        Assert.That(catalogue.LoadMetadatas().Length, Is.EqualTo(1));
        Assert.That(catalogue.LoadMetadatas().First(), Is.EqualTo(loadMetadata));
        loadMetadata.UnlinkFromCatalogue(catalogue);
        Assert.That(catalogue.LoadMetadatas().Length, Is.EqualTo(0));
        Assert.That(loadMetadata.GetAllCatalogues().ToArray().Length , Is.EqualTo(0));
    }

    [Test]
    public void BasicLoadMetadataLinkage_Multiple()
    {
        var loadMetadata = new LoadMetadata(CatalogueRepository);
        var catalogue = new Catalogue(CatalogueRepository, "catalogue");
        var catalogue2 = new Catalogue(CatalogueRepository, "catalogue2");
        loadMetadata.LinkToCatalogue(catalogue);
        loadMetadata.LinkToCatalogue(catalogue2);
        Assert.That(catalogue.LoadMetadatas().Length, Is.EqualTo(1));
        Assert.That(catalogue.LoadMetadatas().First(), Is.EqualTo(loadMetadata));
        Assert.That(catalogue2.LoadMetadatas().Length, Is.EqualTo(1));
        Assert.That(catalogue2.LoadMetadatas().First(), Is.EqualTo(loadMetadata));
        Assert.That(loadMetadata.GetAllCatalogues().ToArray().Length, Is.EqualTo(2));
    }

    [Test]
    public void BasicLoadMetadataLinkage_MultipleUnlink()
    {
        var loadMetadata = new LoadMetadata(CatalogueRepository);
        var catalogue = new Catalogue(CatalogueRepository, "catalogue");
        var catalogue2 = new Catalogue(CatalogueRepository, "catalogue2");
        loadMetadata.LinkToCatalogue(catalogue);
        loadMetadata.LinkToCatalogue(catalogue2);
        loadMetadata.UnlinkFromCatalogue(catalogue2);
        Assert.That(catalogue.LoadMetadatas().Length, Is.EqualTo(1));
        Assert.That(catalogue.LoadMetadatas().First(), Is.EqualTo(loadMetadata));
        Assert.That(catalogue2.LoadMetadatas().Length, Is.EqualTo(0));
        Assert.That(catalogue2.LoadMetadatas(), Is.Empty);
        Assert.That(loadMetadata.GetAllCatalogues().ToArray().Length, Is.EqualTo(1));
    }
}
