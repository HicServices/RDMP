// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.DesignPatternTests;

public class DatabaseEntityConventionTests:UnitTests
{
    /// <summary>
    /// This test checks that constructors of <see cref="DatabaseEntity"/> classes know what repo they are supposed to be written into
    /// e.g. <see cref="ICatalogueRepository"/> or <see cref="IDataExportRepository"/> (or a plugin one?) but definitely not just
    /// <see cref="IRepository"/>
    /// </summary>
    [Test]
    public void AllDatabaseEntitiesHaveTypedIRepository()
    {
        SetupMEF();

        var problems = new List<string>();

        foreach (var type in MEF.GetAllTypes().Where(t => typeof(DatabaseEntity).IsAssignableFrom(t)))
        {
            foreach (var constructorInfo in type.GetConstructors())
            {
                var parameters = constructorInfo.GetParameters();
                    
                if (parameters.Any(p=>p.ParameterType == typeof(IRepository)))
                {
                    problems.Add($"Constructor found on Type {type} that takes {nameof(IRepository)}, it should take either {nameof(IDataExportRepository)} or {nameof(ICatalogueRepository)}");
                }
            }
        }

        foreach (var problem in problems)
            TestContext.Out.WriteLine(problem);

        Assert.IsEmpty(problems);
    }
}