// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation;

internal class ExtendedPropertyTests : DatabaseTests
{
    [Test]
    public void ExtendedProperty_Catalogue()
    {
        var cata = new Catalogue(CatalogueRepository, "My cata");
        var prop = new ExtendedProperty(CatalogueRepository, cata, "Fish", 5);

        Assert.Multiple(() =>
        {
            Assert.That(prop.GetValueAsSystemType(), Is.EqualTo(5));
            Assert.That(prop.IsReferenceTo(cata));
        });

        prop.SetValue(10);
        prop.SaveToDatabase();

        Assert.Multiple(() =>
        {
            Assert.That(prop.GetValueAsSystemType(), Is.EqualTo(10));
            Assert.That(prop.IsReferenceTo(cata));
        });

        prop.RevertToDatabaseState();

        Assert.Multiple(() =>
        {
            Assert.That(prop.GetValueAsSystemType(), Is.EqualTo(10));
            Assert.That(prop.IsReferenceTo(cata));
        });

        CatalogueRepository.GetObjectByID<ExtendedProperty>(prop.ID);
        Assert.Multiple(() =>
        {
            Assert.That(prop.GetValueAsSystemType(), Is.EqualTo(10));
            Assert.That(prop.IsReferenceTo(cata));
        });
    }
}