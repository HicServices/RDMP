// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.ANOEngineering;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.Mutilators.Dilution;
using Rdmp.Core.DataLoad.Modules.Mutilators.Dilution.Operations;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.DilutionTests;

public class DilutionOperationFactoryTests : DatabaseTests
{
    [Test]
    public void NullColumn_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DilutionOperationFactory(null));
    }

    [Test]
    public void NullOperation_Throws()
    {
        var col = Substitute.For<IPreLoadDiscardedColumn>(p => p.Repository == CatalogueRepository);

        var factory = new DilutionOperationFactory(col);
        Assert.Throws<ArgumentNullException>(() => factory.Create(null));
    }

    [Test]
    public void UnexpectedType_Throws()
    {
        var col = Substitute.For<IPreLoadDiscardedColumn>(p => p.Repository == CatalogueRepository);

        var factory = new DilutionOperationFactory(col);
        Assert.Throws<ArgumentException>(() => factory.Create(typeof(Catalogue)));
    }

    [Test]
    public void ExpectedType_Created()
    {
        var col = Substitute.For<IPreLoadDiscardedColumn>(p => p.Repository == CatalogueRepository);

        var factory = new DilutionOperationFactory(col);
        var i = factory.Create(typeof(ExcludeRight3OfUKPostcodes));
        Assert.IsNotNull(i);
        Assert.IsInstanceOf<IDilutionOperation>(i);
    }
}