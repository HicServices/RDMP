// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.DesignPatternTests;

public class DatabaseEntityConventionTests : UnitTests
{
    /// <summary>
    /// This test checks that constructors of <see cref="DatabaseEntity"/> classes know what repo they are supposed to be written into
    /// e.g. <see cref="ICatalogueRepository"/> or <see cref="IDataExportRepository"/> (or a plugin one?) but definitely not just
    /// <see cref="IRepository"/>
    /// </summary>
    [Test]
    public void AllDatabaseEntitiesHaveTypedIRepository()
    {
        var problems = MEF.GetAllTypes()
            .Where(static t => typeof(DatabaseEntity).IsAssignableFrom(t))
            .SelectMany(static type => type.GetConstructors(),
                static (type, constructorInfo) => new { type, constructorInfo })
            .Select(t1 => new { t1, parameters = t1.constructorInfo.GetParameters() })
            .Where(static t1 => t1.parameters.Any(p => p.ParameterType == typeof(IRepository)))
            .Select(static t1 =>
                $"Constructor found on Type {t1.t1.type} that takes {nameof(IRepository)}, it should take either {nameof(IDataExportRepository)} or {nameof(ICatalogueRepository)}")
            .ToList();

        foreach (var problem in problems)
            TestContext.Out.WriteLine(problem);

        Assert.That(problems, Is.Empty);
    }
}