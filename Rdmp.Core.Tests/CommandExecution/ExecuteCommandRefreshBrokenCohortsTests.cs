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
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandRefreshBrokenCohortsTests
{
    [Test]
    public void TestBrokenCohort()
    {
        var repo = new MemoryDataExportRepository();

        var ect = new ExternalCohortTable(repo, "yarg", FAnsi.DatabaseType.MicrosoftSQLServer)
        {
            Server = "IDontExist",
            Database = "fff",
            PrivateIdentifierField = "haha",
            ReleaseIdentifierField = "haha"
        };
        ect.SaveToDatabase();

        var cohort = new ExtractableCohort
        {
            Repository = repo,
            ExternalCohortTable_ID = ect.ID,
            OriginID = 123
        };
        cohort.SaveToDatabase();

        var repoLocator = new RepositoryProvider(repo);

        var activator = new ConsoleInputManager(repoLocator, ThrowImmediatelyCheckNotifier.Quiet)
        {
            DisallowInput = true
        };

        Assert.That(((DataExportChildProvider)activator.CoreChildProvider).ForbidListedSources, Has.Count.EqualTo(1));

        var cmd = new ExecuteCommandRefreshBrokenCohorts(activator)
        {
            // suppress publishing so we don't just go back into a refresh
            // and find it missing again
            NoPublish = true
        };

        Assert.That(cmd.IsImpossible, Is.False);
        cmd.Execute();

        //now no forbidden cohorts
        Assert.That(((DataExportChildProvider)activator.CoreChildProvider).ForbidListedSources, Is.Empty);


        cmd = new ExecuteCommandRefreshBrokenCohorts(activator);
        Assert.Multiple(() =>
        {
            Assert.That(cmd.IsImpossible);
            Assert.That(cmd.ReasonCommandImpossible, Is.EqualTo("There are no broken ExternalCohortTable to clear status on"));
        });

        cmd = new ExecuteCommandRefreshBrokenCohorts(activator, ect);
        Assert.Multiple(() =>
        {
            Assert.That(cmd.IsImpossible);
            Assert.That(cmd.ReasonCommandImpossible, Is.EqualTo("'yarg' is not broken"));
        });
    }
}