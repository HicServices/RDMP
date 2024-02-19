using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
