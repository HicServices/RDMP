// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.Checks;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using System;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.Data;

public class SelectedDataSetsCheckerTests : TestsRequiringAnExtractionConfiguration
{
    [Test]
    public void NormalUseCasePasses()
    {
        // normal checks pass
        var checker = new SelectedDataSetsChecker(new ThrowImmediatelyActivator(RepositoryLocator), _selectedDataSet);
        checker.Check(ThrowImmediatelyCheckNotifier.Quiet());
    }

    [Test]
    public void TestExtractionProgress_MidwayWithNoAuditRecord()
    {
        // normal checks pass
        var checker = new SelectedDataSetsChecker(new ThrowImmediatelyActivator(RepositoryLocator), _selectedDataSet);

        var ep = new ExtractionProgress(DataExportRepository, _selectedDataSet, new DateTime(1990, 1, 1), new DateTime(2001, 1, 1),100,"mybatch",
            _extractionInformations[0].ID)
        {
            ProgressDate = new DateTime(1995, 1, 1) // we are half way through
        };

        ep.SaveToDatabase();

        var ex = Assert.Throws<Exception>(()=>checker.Check(ThrowImmediatelyCheckNotifier.Quiet()));
        Assert.AreEqual("R0016 ExtractionProgress 'mybatch' is 'in progress' (ProgressDate is not null) but there is no audit of previously extracted SQL (needed for checking cohort changes)", ex.Message);
    }

    [Test]
    public void TestExtractionProgress_AuditRecordHasDifferentCohort()
    {
        // normal checks pass
        var checker = new SelectedDataSetsChecker(new ThrowImmediatelyActivator(RepositoryLocator), _selectedDataSet);

        foreach (var r in DataExportRepository.GetAllObjects<CumulativeExtractionResults>()) r.DeleteInDatabase();

        var ep = new ExtractionProgress(DataExportRepository, _selectedDataSet, new DateTime(1990, 1, 1),
            new DateTime(2001, 1, 1), 100, "mybatch",
            _extractionInformations[0].ID)
        {
            ProgressDate = new DateTime(1995, 1, 1) // we are half way through
        };

        var ep = new ExtractionProgress(DataExportRepository, _selectedDataSet, new DateTime(1990, 1, 1), new DateTime(2001, 1, 1), 100, "mybatch",
            _extractionInformations[0].ID)
        {
            ProgressDate = new DateTime(1995, 1, 1) // we are half way through
        };

        ep.SaveToDatabase();

        // audit has SQL that does not contain the cohort ID
        var audit = new CumulativeExtractionResults(DataExportRepository, _configuration,
            _selectedDataSet.ExtractableDataSet, "select * from [yohoho and a bottle of rum]");
        audit.CompleteAudit(typeof(ExecuteFullExtractionToDatabaseMSSql), "[over the hills and far away]", 333, true,
            false);
        audit.SaveToDatabase();

        var ex = Assert.Throws<Exception>(() => checker.Check(ThrowImmediatelyCheckNotifier.Quiet()));
        Assert.AreEqual(
            $"R0017 ExtractionProgress 'mybatch' is 'in progress' (ProgressDate is not null) but we did not find the expected Cohort WHERE Sql in the audit of SQL extracted with the last batch.  Did you change the cohort without resetting the ProgressDate? The SQL we expected to find was '[{TestDatabaseNames.Prefix}CohortDatabase]..[Cohort].[cohortDefinition_id]=-599'",
            ex.Message);

        // tidy up
        ep.DeleteInDatabase();
    }

    [Test]
    public void TestExtractionProgress_AuditRecordIsGood_NoProblems()
    {
        // normal checks pass
        var checker = new SelectedDataSetsChecker(new ThrowImmediatelyActivator(RepositoryLocator), _selectedDataSet);

        var ep = new ExtractionProgress(DataExportRepository, _selectedDataSet, new DateTime(1990, 1, 1), new DateTime(2001, 1, 1), 100, "mybatch",
            _extractionInformations[0].ID)
        {
            ProgressDate = new DateTime(1995, 1, 1) // we are half way through
        };

        ep.SaveToDatabase();

        foreach (var r in DataExportRepository.GetAllObjects<CumulativeExtractionResults>()) r.DeleteInDatabase();


        // audit has SQL is good, it contains the correct cohort
        var audit = new CumulativeExtractionResults(DataExportRepository, _configuration,
            _selectedDataSet.ExtractableDataSet,
            $"select * from [yohoho and a bottle of rum] WHERE [{TestDatabaseNames.Prefix}CohortDatabase]..[Cohort].[cohortDefinition_id]=-599'");

        audit.CompleteAudit(typeof(ExecuteFullExtractionToDatabaseMSSql), "[over the hills and far away]", 333, true,
            false);
        audit.SaveToDatabase();

        Assert.DoesNotThrow(() => checker.Check(ThrowImmediatelyCheckNotifier.Quiet()));

        // tidy up
        ep.DeleteInDatabase();
    }
}