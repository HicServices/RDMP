// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandRefreshBrokenCohortsTests 
{

    [Test]
    public void TestBrokenCohort()
    {
        var repo = new MemoryDataExportRepository();
            
        var ect = new ExternalCohortTable(repo, "yarg", FAnsi.DatabaseType.MicrosoftSQLServer);
        ect.Server = "IDontExist";
        ect.Database = "fff";
        ect.PrivateIdentifierField = "haha";
        ect.ReleaseIdentifierField = "haha";
        ect.SaveToDatabase();

        var cohort = new ExtractableCohort();
        cohort.Repository = repo;
        cohort.ExternalCohortTable_ID = ect.ID;
        cohort.OriginID = 123;
        cohort.SaveToDatabase();

        var repoLocator = new RepositoryProvider(repo);

        var activator = new ConsoleInputManager(repoLocator, new ThrowImmediatelyCheckNotifier()) {
            DisallowInput = true
        };

        Assert.AreEqual(1,((DataExportChildProvider)activator.CoreChildProvider).ForbidListedSources.Count);

        var cmd = new ExecuteCommandRefreshBrokenCohorts(activator)
        {
            // suppress publishing so we don't just go back into a refresh
            // and find it missing again
            NoPublish = true,
        };
            
        Assert.IsFalse(cmd.IsImpossible);
        cmd.Execute();

        //now no forbidden cohorts
        Assert.IsEmpty(((DataExportChildProvider)activator.CoreChildProvider).ForbidListedSources);


        cmd = new ExecuteCommandRefreshBrokenCohorts(activator);
        Assert.IsTrue(cmd.IsImpossible);
        Assert.AreEqual("There are no broken ExternalCohortTable to clear status on", cmd.ReasonCommandImpossible);
            
        cmd = new ExecuteCommandRefreshBrokenCohorts(activator,ect);
        Assert.IsTrue(cmd.IsImpossible);
        Assert.AreEqual("'yarg' is not broken", cmd.ReasonCommandImpossible);
    }
}