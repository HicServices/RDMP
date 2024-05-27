// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.IO;
using System.Linq;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandSetExtractionConfigurationCohortTests : TestsRequiringACohort
{

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();
    }

    [Test]
    public void UpdateCohortForExtractionTest()
    {
        var proj = new Project(DataExportRepository, "Some Proj")
        {
            ProjectNumber = 999
        };
        proj.SaveToDatabase();

        // we are replacing this imaginary cohort
        var definition998 = new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable);
        // with this one (v2)
        var definition999 = new CohortDefinition(null, "CommittingNewCohorts", 2, 999, _externalCohortTable);

        // Create a basic cohort first
        var request1 = new CohortCreationRequest(proj, definition998, DataExportRepository, "fish");
        request1.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var con = _cohortDatabase.Server.GetManagedConnection();
        request1.PushToServer(con);
        request1.ImportAsExtractableCohort(true, false);

        // the definition was imported and should now be a saved ExtractableCohort
        var cohort998 = request1.CohortCreatedIfAny;
        Assert.That(cohort998, Is.Not.Null);
        Assert.That(cohort998.IsDeprecated, Is.False);

        //create second cohort 
        // Create a basic cohort first
        var request2 = new CohortCreationRequest(proj, definition999, DataExportRepository, "fish");
        request2.Check(ThrowImmediatelyCheckNotifier.Quiet);

        request2.PushToServer(con);
        request2.ImportAsExtractableCohort(true, false);

        // the definition was imported and should now be a saved ExtractableCohort
        var cohort999 = request2.CohortCreatedIfAny;
        Assert.That(cohort999, Is.Not.Null);
        Assert.That(cohort999.IsDeprecated, Is.False);

        // legit user 1
        var ec1 = new ExtractionConfiguration(DataExportRepository, proj)
        {
            IsReleased = false,
            Cohort_ID = cohort998.ID
        };
        ec1.SaveToDatabase();
        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
        { DisallowInput = true };
        var cmd = new ExecuteCommandSetExtractionConfigurationCohort(activator, ec1, cohort999);
        Assert.DoesNotThrow(() => cmd.Execute());
        var updatedExt = DataExportRepository.GetAllObjects<ExtractionConfiguration>().Where(ei => ei.ID == ec1.ID).ToList();
        Assert.That(updatedExt.Count, Is.EqualTo(1));
        Assert.That(updatedExt.First().Cohort_ID, Is.EqualTo(cohort999.ID));
    }

    [Test]
    public void UpdateCohortForExtractionTest_BadCohort()
    {
        var proj = new Project(DataExportRepository, "Some Proj")
        {
            ProjectNumber = 999
        };
        proj.SaveToDatabase();

        // we are replacing this imaginary cohort
        var definition998 = new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable);
        // with this one (v2)
        var definition999 = new CohortDefinition(null, "CommittingNewCohorts", 2, 999, _externalCohortTable);

        // Create a basic cohort first
        var request1 = new CohortCreationRequest(proj, definition998, DataExportRepository, "fish");
        request1.Check(ThrowImmediatelyCheckNotifier.Quiet);

        using var con = _cohortDatabase.Server.GetManagedConnection();
        request1.PushToServer(con);
        request1.ImportAsExtractableCohort(true, false);

        // the definition was imported and should now be a saved ExtractableCohort
        var cohort998 = request1.CohortCreatedIfAny;
        Assert.That(cohort998, Is.Not.Null);
        Assert.That(cohort998.IsDeprecated, Is.False);

        // legit user 1
        var ec1 = new ExtractionConfiguration(DataExportRepository, proj)
        {
            IsReleased = false,
            Cohort_ID = cohort998.ID
        };
        ec1.SaveToDatabase();
        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
        { DisallowInput = true };
        var cmd = new ExecuteCommandSetExtractionConfigurationCohort(activator, ec1, new ExtractableCohort()
        {
            ID = -1
        });
        Assert.Throws<SqlException>(() => cmd.Execute());
    }
}
