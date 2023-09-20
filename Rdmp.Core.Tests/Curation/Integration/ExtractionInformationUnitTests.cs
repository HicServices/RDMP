// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class ExtractionInformationUnitTests : UnitTests
{
    [TestCase(null)]
    [TestCase(4)]
    [TestCase(-3)]
    [TestCase(99)]
    public void NewExtractionInformation_OrderShouldBeContiguous(int? explicitOrder)
    {
        //When we have an ExtractionInformation
        var ei = WhenIHaveA<ExtractionInformation>();

        if (explicitOrder.HasValue)
        {
            ei.Order = explicitOrder.Value;
            ei.SaveToDatabase();
        }

        Assert.AreEqual(explicitOrder ?? 1, ei.Order);

        // Newly created ones should have the right Order to not collide
        var cata = ei.CatalogueItem.Catalogue;
        var cataItem = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "ci");
        var ei2 = new ExtractionInformation(RepositoryLocator.CatalogueRepository, cataItem, ei.ColumnInfo, "fff");

        Assert.AreEqual((explicitOrder ?? 1) + 1, ei2.Order);
    }

    [Test]
    public void NewExtractionInformation_OrderShouldBeContiguous_ManyCalls()
    {
        //When we have an ExtractionInformation
        var ei1 = WhenIHaveA<ExtractionInformation>();

        // Newly created ones should have the right Order to not collide
        var cata = ei1.CatalogueItem.Catalogue;
        var cataItem2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "ci");
        var ei2 = new ExtractionInformation(RepositoryLocator.CatalogueRepository, cataItem2, ei1.ColumnInfo, "fff");

        Assert.AreEqual(2, ei2.Order);

        var cataItem3 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "ci");
        var ei3 = new ExtractionInformation(RepositoryLocator.CatalogueRepository, cataItem3, ei1.ColumnInfo, "fff");

        Assert.AreEqual(3, ei3.Order);

        var cataItem4 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "ci");
        var ei4 = new ExtractionInformation(RepositoryLocator.CatalogueRepository, cataItem4, ei1.ColumnInfo, "fff");

        Assert.AreEqual(4, ei4.Order);
    }
}