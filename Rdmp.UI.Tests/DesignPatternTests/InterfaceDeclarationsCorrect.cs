// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.UI.Tests.DesignPatternTests;

public class InterfaceDeclarationsCorrect
{
    public void FindProblems(MEF mef)
    {
        List<string> excusables = new List<string>()
        {
            "IPlugin",
            "IDataAccessCredentials",
            "IProcessTask" //this is inherited by IRuntimeTask too which isn't an IMapsDirectlyToDatabaseTable
        };
        List<string> problems = new List<string>();

        foreach (var dbEntities in mef.GetAllTypes().Where(t => typeof(DatabaseEntity).IsAssignableFrom(t)))
        {
            var matchingInterface = typeof(Catalogue).Assembly.GetTypes().SingleOrDefault(t=>t.Name.Equals("I" + dbEntities.Name));

            if (matchingInterface != null)
            {
                if (excusables.Contains(matchingInterface.Name))
                    continue;

                if (!typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(matchingInterface))
                {
                    problems.Add("FAIL: Interface '" + matchingInterface.Name + "' does not inherit IMapsDirectlyToDatabaseTable");
                }
            }

        }

        foreach (string problem in problems)
            Console.WriteLine(problem);

        Assert.AreEqual(0,problems.Count);
    }
}