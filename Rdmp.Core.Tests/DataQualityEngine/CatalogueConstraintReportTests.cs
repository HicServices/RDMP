// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Threading;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataQualityEngine.Reports;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataQualityEngine;

public class CatalogueConstraintReportTests : TestsRequiringAnExtractionConfiguration
{
    private DQERepository GetDqeRepository(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType, "DQETempTestDb");
        var patcher = new DataQualityEnginePatcher();

        var mds = new MasterDatabaseScriptExecutor(db);
        mds.CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());

        return new DQERepository(CatalogueRepository, db);
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypesWithBoolFlags))]
    public void ValidateBulkTestData(DatabaseType dbType, bool testCancellingValidationEarly)
    {
        const int numberOfRecordsToGenerate = 10000;
        var startTime = DateTime.Now;

        var testData = new BulkTestsData(CatalogueRepository, GetCleanedServer(DatabaseType.MicrosoftSQLServer),
            numberOfRecordsToGenerate);
        testData.SetupTestData();
        testData.ImportAsCatalogue();

        var dqeRepository = GetDqeRepository(dbType);

        //there shouldn't be any lingering results in the database
        Assert.That(dqeRepository.GetMostRecentEvaluationFor(_catalogue), Is.Null);

        //set some validation rules
        testData.catalogue.ValidatorXML = bulkTestDataValidation;

        //set the time periodicity field
        var toBeTimePeriodicityCol = testData.catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => e.GetRuntimeName().Equals("dtCreated"));
        testData.catalogue.TimeCoverage_ExtractionInformation_ID = toBeTimePeriodicityCol.ID;

        //do the validation
        var report = new CatalogueConstraintReport(testData.catalogue, SpecialFieldNames.DataLoadRunID)
        {
            ExplicitDQERepository = dqeRepository
        };

        report.Check(ThrowImmediatelyCheckNotifier.Quiet);

        var source = new CancellationTokenSource();

        if (testCancellingValidationEarly)
            source.Cancel();

        var listener = new ToMemoryDataLoadEventListener(false);
        report.GenerateReport(testData.catalogue, listener, source.Token);

        if (testCancellingValidationEarly)
        {
            Assert.That(
                listener.EventsReceivedBySender[report].Count(m => m.Exception is OperationCanceledException), Is.EqualTo(1));
            testData.DeleteCatalogue();
            return;
        }

        Assert.That(listener.EventsReceivedBySender[report].All(m => m.Exception == null),
            string.Join(Environment.NewLine,
                listener.EventsReceivedBySender[report].Where(m => m.Exception != null)
                    .Select(m => m.Exception))); //all messages must have null exceptions


        //get the results now
        var results = dqeRepository.GetMostRecentEvaluationFor(testData.catalogue);

        Assert.That(results, Is.Not.Null);

        Assert.Multiple(() =>
        {
            //the sum of all consequences across all data load run ids should be the record count
            Assert.That(results.RowStates.Sum(r => r.Missing + r.Invalid + r.Wrong + r.Correct), Is.EqualTo(10000));

            //there should be at least 5 data load run ids (should be around 12 actually - see BulkTestData but theoretically everyone could magically - all 10,000 into 5 decades - or even less but those statistics must be astronomical)
            Assert.That(results.RowStates, Has.Length.GreaterThanOrEqualTo(5));

            //there should be lots of column results too
            Assert.That(results.ColumnStates, Has.Length.GreaterThanOrEqualTo(5));
        });

        //Did it log?
        var logManager = new LogManager(CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        var log = logManager.GetArchivalDataLoadInfos("DQE").FirstOrDefault();
        Assert.That(log, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(log.StartTime, Is.GreaterThanOrEqualTo(startTime));
            Assert.That(log.Errors, Is.Empty);
            Assert.That(log.TableLoadInfos.Single().Inserts, Is.EqualTo(numberOfRecordsToGenerate));
        });

        testData.DeleteCatalogue();
    }


    #region Checkability

    [Test]
    public void SupportsValidation_NoLoggingServer()
    {
        IServerDefaults defaults = CatalogueRepository;
        var before = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

        //cannot run test because it didn't have a value to clear!
        Assert.That(before, Is.Not.Null);

        //clear the default value
        defaults.ClearDefault(PermissableDefaults.LiveLoggingServer_ID);

        try
        {
            var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

            var e = Assert.Throws<Exception>(() => report.Check(ThrowImmediatelyCheckNotifier.Quiet));
            Assert.That(
                e?.Message, Does.StartWith("Failed to setup logging of DQE runs"));
        }
        finally
        {
            defaults.SetDefault(PermissableDefaults.LiveLoggingServer_ID, before);
        }
    }

    [Test]
    public void SupportsValidation_NoDQE()
    {
        IServerDefaults defaults = CatalogueRepository;
        var before = defaults.GetDefaultFor(PermissableDefaults.DQE);

        //cannot run test because it didn't have a value to clear!
        Assert.That(before, Is.Not.Null);

        //clear the default value
        defaults.ClearDefault(PermissableDefaults.DQE);

        try
        {
            var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

            var e = Assert.Throws<Exception>(() => report.Check(ThrowImmediatelyCheckNotifier.Quiet));
            Assert.That(
                e?.Message, Does.StartWith("Failed to create DQE Repository, possibly there is no DataQualityEngine Reporting Server (ExternalDatabaseServer).  You will need to create/set one in CatalogueManager"));
        }
        finally
        {
            defaults.SetDefault(PermissableDefaults.DQE, before);
        }
    }


    [Test]
    public void SupportsValidation_NoValidatorXML()
    {
        var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);
        _catalogue.ValidatorXML = null;

        //it has no validator XML currently
        Assert.That(report.CatalogueSupportsReport(_catalogue), Is.False);

        var ex = Assert.Throws<Exception>(() => report.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Does.Contain("There is no ValidatorXML specified for the Catalogue TestTable"));
    }

    [Test]
    public void SupportsValidation_BadXML()
    {
        var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

        _catalogue.ValidatorXML = "fish";
        //it has no validator XML currently
        Assert.That(report.CatalogueSupportsReport(_catalogue), Is.False);

        var ex = Assert.Throws<Exception>(() => report.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Does.Contain("ValidatorXML for Catalogue TestTable could not be deserialized into a Validator"));
    }

    [Test]
    public void SupportsValidation_MadeUpColumnName()
    {
        var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

        _catalogue.ValidatorXML = dodgyColumnXML;
        //it has no validator XML currently
        Assert.That(report.CatalogueSupportsReport(_catalogue), Is.False);

        var ex = Assert.Throws<Exception>(() => report.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Is.EqualTo("Could not find a column in the extraction SQL that would match TargetProperty chi"));
    }

    [Test]
    public void SupportsValidation_GoodButNoDataLoadRunID()
    {
        var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

        _catalogue.ValidatorXML = validColumnXML;

        //set the time periodicity field
        var toBeTimePeriodicityCol = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => e.GetRuntimeName().Equals("PrivateID"));
        _catalogue.TimeCoverage_ExtractionInformation_ID = toBeTimePeriodicityCol.ID;

        var notifier = new ToMemoryCheckNotifier();
        report.Check(notifier);

        Assert.Multiple(() =>
        {
            Assert.That(notifier.GetWorst(), Is.EqualTo(CheckResult.Warning));
            Assert.That(notifier.Messages.Select(m => m.Message).ToArray(), Does.Contain("Found column in query builder columns which matches TargetProperty Name"));

            Assert.That(report.CatalogueSupportsReport(_catalogue));
        });

        var ex = Assert.Throws<Exception>(() => report.Check(ThrowImmediatelyCheckNotifier.QuietPicky));
        Assert.That(ex.Message, Is.EqualTo("Did not find ExtractionInformation for a column called hic_dataLoadRunID, this will prevent you from viewing the resulting report subdivided by data load batch (make sure you have this column and that it is marked as extractable)"));
    }

    #endregion


    private string bulkTestDataValidation = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Validator xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <ItemValidators>
    <ItemValidator>
      <TargetProperty>current_gp_accept_date</TargetProperty>
      <SecondaryConstraints>
        <SecondaryConstraint xsi:type=""BoundDate"">
          <Name>bounddate</Name>
          <Consequence>Wrong</Consequence>
          <Rationale>Patient cannot be seen by a GP before they are born!</Rationale>
          <LowerFieldName>date_of_birth</LowerFieldName>
          <UpperFieldName>date_of_death</UpperFieldName>
          <Inclusive>true</Inclusive>
          <Lower xsi:nil=""true"" />
          <Upper xsi:nil=""true"" />
        </SecondaryConstraint>
      </SecondaryConstraints>
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>date_of_death</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
  </ItemValidators>
</Validator>";

    private string validColumnXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Validator xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <ItemValidators>
    <ItemValidator>
      <TargetProperty>Name</TargetProperty>
      <SecondaryConstraints>
        <SecondaryConstraint xsi:type=""NotNull"">
          <Name>not null</Name>
          <Consequence>Wrong</Consequence>
        </SecondaryConstraint>
      </SecondaryConstraints>
    </ItemValidator>
  </ItemValidators>
</Validator>";


    private string dodgyColumnXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Validator xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <ItemValidators>
    <ItemValidator>
      <PrimaryConstraint xsi:type=""Chi"">
        <Name>chi</Name>
        <Consequence>InvalidatesRow</Consequence>
      </PrimaryConstraint>
      <TargetProperty>chi</TargetProperty>
      <SecondaryConstraints>
        <SecondaryConstraint xsi:type=""NotNull"">
          <Name>not null</Name>
          <Consequence>Wrong</Consequence>
        </SecondaryConstraint>
      </SecondaryConstraints>
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>hb_extract</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
  </ItemValidators>
</Validator>";
}