// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.DataExport.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.ObscureDependencyTests;

public class ObjectSharingObscureDependencyFinderTests : DatabaseTests
{
    private ShareManager _share;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();
        _share = new ShareManager(RepositoryLocator);
    }

    [Test]
    public void TestPruning()
    {
        var c = new Catalogue(CatalogueRepository, "Catapault");
        var ci = new CatalogueItem(CatalogueRepository, c, "string");

        var c2 = new Catalogue(CatalogueRepository, "Catapault (Import)");
        var ci2 = new CatalogueItem(CatalogueRepository, c2, "string (Import)");

        Assert.That(CatalogueRepository.GetAllObjects<ObjectExport>(), Is.Empty);
        var ec = _share.GetNewOrExistingExportFor(c);
        var eci = _share.GetNewOrExistingExportFor(ci);

        _share.GetImportAs(ec.SharingUID, c2);
        _share.GetImportAs(eci.SharingUID, ci2);

        Assert.Multiple(() =>
        {
            Assert.That(CatalogueRepository.GetAllObjects<ObjectExport>(), Has.Length.EqualTo(2));
            Assert.That(CatalogueRepository.GetAllObjects<ObjectImport>(), Has.Length.EqualTo(2));
        });
        Assert.That(CatalogueRepository.GetAllObjects<ObjectImport>()
, Has.Length.EqualTo(2)); //successive calls shouldn't generate extra entries since they are same obj
        Assert.That(CatalogueRepository.GetAllObjects<ObjectImport>(), Has.Length.EqualTo(2));

        //cannot delete the shared object
        Assert.Throws<Exception>(c.DeleteInDatabase);

        //can delete the import because that's ok
        Assert.DoesNotThrow(c2.DeleteInDatabase);

        //now that we deleted the import it should have deleted everything else including the CatalogueItem import which magically disappeared when we deleted the Catalogue via database level cascade events
        Assert.That(CatalogueRepository.GetAllObjects<ObjectImport>(), Is.Empty);

        _share.GetImportAs(eci.SharingUID, ci2);
    }

    [Test]
    public void CannotDeleteSharedObjectTest()
    {
        //create a test catalogue
        var c = new Catalogue(CatalogueRepository, "blah");

        Assert.That(_share.IsExportedObject(c), Is.False);

        //make it exportable
        var exportDefinition = _share.GetNewOrExistingExportFor(c);

        Assert.That(_share.IsExportedObject(c));

        //cannot delete because object is shared externally
        Assert.Throws<Exception>(c.DeleteInDatabase);

        //no longer exportable
        exportDefinition.DeleteInDatabase();

        //no longer shared
        Assert.That(_share.IsExportedObject(c), Is.False);

        //now we can delete it
        c.DeleteInDatabase();
    }

    [Test]
    public void CascadeDeleteImportDefinitions()
    {
        var p = new Project(DataExportRepository, "prah");

        var exportDefinition = _share.GetNewOrExistingExportFor(p);

        var p2 = new Project(DataExportRepository, "prah2");

        var importDefinition = _share.GetImportAs(exportDefinition.SharingUID, p2);

        //import definition exists
        Assert.That(importDefinition.Exists());

        //delete local import
        p2.DeleteInDatabase();

        //cascade should have deleted the import definition since the imported object version is gone
        Assert.That(importDefinition.Exists(), Is.False);

        //clear SetUp the exported version too
        exportDefinition.DeleteInDatabase();
        p.DeleteInDatabase();
    }
}