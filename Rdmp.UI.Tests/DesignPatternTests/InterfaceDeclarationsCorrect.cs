// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.UI.Tests.DesignPatternTests;

public class InterfaceDeclarationsCorrect
{
    private static readonly List<string> Exemptions = new()
    {
        "IPlugin",
        "IDataAccessCredentials",
        "IProcessTask" //this is inherited by IRuntimeTask too which isn't an IMapsDirectlyToDatabaseTable
    };

    public static void FindProblems()
    {
        var problems =
            MEF.GetAllTypes()
                .Where(static t => typeof(DatabaseEntity).IsAssignableFrom(t))
                .Select(static dbEntities => typeof(Catalogue).Assembly.GetTypes()
                    .SingleOrDefault(t => t.Name.Equals($"I{dbEntities.Name}")))
                .Where(static matchingInterface => matchingInterface != null &&
                                                   !Exemptions.Contains(matchingInterface.Name) &&
                                                   !typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(
                                                       matchingInterface))
                .Select(static matchingInterface =>
                    $"FAIL: Interface '{matchingInterface.Name}' does not inherit IMapsDirectlyToDatabaseTable")
                .ToList();
        Assert.That(problems, Is.Empty);
    }
}