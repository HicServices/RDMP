// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Tests.Curation.MemoryRepositoryTests;

[Category("Unit")]
internal class MemoryRepositoryTests
{
    private readonly MemoryCatalogueRepository _repo = new();

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        ImplementationManager.Load<MicrosoftSQLImplementation>();
        ImplementationManager.Load<MySqlImplementation>();
        ImplementationManager.Load<OracleImplementation>();
        ImplementationManager.Load<PostgreSqlImplementation>();
    }

    [Test]
    public void TestMemoryRepository_CatalogueConstructor()
    {
        var memCatalogue = new Catalogue(_repo, "My New Catalogue");

        Assert.That(_repo.GetObjectByID<Catalogue>(memCatalogue.ID), Is.EqualTo(memCatalogue));
    }

    [Test]
    public void TestMemoryRepository_QueryBuilder()
    {
        var memCatalogue = new Catalogue(_repo, "My New Catalogue");

        var myCol = new CatalogueItem(_repo, memCatalogue, "MyCol1");

        var ti = new TableInfo(_repo, "My table");
        var col = new ColumnInfo(_repo, "Mycol", "varchar(10)", ti);

        Assert.That(_repo.GetObjectByID<Catalogue>(memCatalogue.ID), Is.EqualTo(memCatalogue));

        var qb = new QueryBuilder(null, null);
        qb.AddColumnRange(memCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

        Assert.That(qb.SQL, Is.EqualTo(@"
SELECT 

Mycol
FROM 
My table"));
    }
}