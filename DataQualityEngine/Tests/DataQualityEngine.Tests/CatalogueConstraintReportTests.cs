using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Triggers;
using DataExportLibrary.Tests.DataExtraction;
using DataQualityEngine.Data;
using DataQualityEngine.Reports;
using Diagnostics.TestData;
using HIC.Logging;
using HIC.Logging.PastEvents;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataQualityEngine.Tests
{
    public class CatalogueConstraintReportTests : TestsRequiringAnExtractionConfiguration
    {
        
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ValidateBulkTestData(bool testCancellingValiationEarly)
        {
            int numberOfRecordsToGenerate = 10000;
            DateTime startTime = DateTime.Now;

            BulkTestsData testData = new BulkTestsData(CatalogueRepository,DiscoveredDatabaseICanCreateRandomTablesIn,numberOfRecordsToGenerate); 
            testData.SetupTestData();
            testData.ImportAsCatalogue();

            DQERepository dqeRepository = new DQERepository(CatalogueRepository);

            //the shouldn't be any lingering results in the database
            Assert.IsNull(dqeRepository.GetMostRecentEvaluationFor(_catalogue));

            //set some validation rules
            testData.catalogue.ValidatorXML = bulkTestDataValidation;

            //set the time periodicity field
            var toBeTimePeriodicityCol = testData.catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => e.GetRuntimeName().Equals("dtCreated"));
            testData.catalogue.TimeCoverage_ExtractionInformation_ID = toBeTimePeriodicityCol.ID;
            
            //do the validation
            CatalogueConstraintReport report = new CatalogueConstraintReport(testData.catalogue, SpecialFieldNames.DataLoadRunID);
            report.Check(new ThrowImmediatelyCheckNotifier());

            CancellationTokenSource source = new CancellationTokenSource();

            if (testCancellingValiationEarly)
                source.Cancel();

            ToMemoryDataLoadEventListener listener = new ToMemoryDataLoadEventListener(false);
            report.GenerateReport(testData.catalogue, listener, source.Token);

            if(testCancellingValiationEarly)
            {
                Assert.IsTrue(listener.EventsReceivedBySender[report].Count(m=>m.Exception is OperationCanceledException) == 1);
                testData.Destroy();
                testData.DeleteCatalogue();
                return;
            }
            
            Assert.IsTrue(listener.EventsReceivedBySender[report].All(m => m.Exception == null));//all messages must have null exceptions
            
            
            //get the reuslts now
            var results = dqeRepository.GetMostRecentEvaluationFor(testData.catalogue);

            Assert.IsNotNull(results);

            //the sum of all consquences across all data load run ids should be the record count
            Assert.AreEqual(10000,results.RowStates.Sum(r=>r.Missing + r.Invalid + r.Wrong + r.Correct));

            //there should be at least 5 data load run ids (should be around 12 actually - see BulkTestData but theoretically everyone could magically - all 10,000 into 5 decades - or even less but those statistics must be astronomical)
            Assert.GreaterOrEqual(results.RowStates.Count(),5);

            //there should be lots of column results too
            Assert.GreaterOrEqual(results.ColumnStates.Count(),5);

            //Did it log?
            LogManager logManager = new LogManager(new ServerDefaults(CatalogueRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID));
            var log = logManager.GetLoadStatusOf(PastEventType.MostRecent, "DQE");
            Assert.IsNotNull(log);
            Assert.GreaterOrEqual(log.StartTime, startTime);
            Assert.AreEqual(0,log.Errors.Count);
            Assert.AreEqual(numberOfRecordsToGenerate,log.TableLoadInfos.Single().Inserts);

            testData.Destroy();
            
            testData.DeleteCatalogue();

        }


        #region Checkability


        [Test]
        public void SupportsValidation_NoLoggingServer()
        {
            ServerDefaults defaults = new ServerDefaults(CatalogueRepository);
            var before = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

            //cannot run test because it didn't have a value to clear!
            Assert.IsNotNull(before);

            //clear the default value
            defaults.ClearDefault(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

            try
            {
                CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

                var e = Assert.Throws<Exception>(() => report.Check(new ThrowImmediatelyCheckNotifier()));
                Assert.IsTrue(
                    e.Message.StartsWith(
                    "Failed to setup logging of DQE runs")
                );
            }
            finally
            {
                defaults.SetDefault(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID, before);
            }
        }

        [Test]
        public void SupportsValidation_NoDQE()
        {
            ServerDefaults defaults = new ServerDefaults(CatalogueRepository);
            var before = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.DQE);

            //cannot run test because it didn't have a value to clear!
            Assert.IsNotNull(before);

            //clear the default value
            defaults.ClearDefault(ServerDefaults.PermissableDefaults.DQE);
            
            try
            {
                CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

                var e = Assert.Throws<Exception>(()=> report.Check(new ThrowImmediatelyCheckNotifier()));
                Assert.IsTrue(
                    e.Message.StartsWith(
                    "Failed to create DQE Repository, possibly there is no DataQualityEngine Reporting Server (ExternalDatabaseServer).  You will need to create/set one in CatalogueManager")
                );
            }
            finally
            {
                defaults.SetDefault(ServerDefaults.PermissableDefaults.DQE, before);
            }
        }


        [Test]
        public void SupportsValidation_NoValidatorXML()
        {

            CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);
            _catalogue.ValidatorXML = null;

            //it has no validator XML currently 
            Assert.IsFalse(report.CatalogueSupportsReport(_catalogue));

            var ex = Assert.Throws<Exception>(()=>report.Check(new ThrowImmediatelyCheckNotifier()));
            StringAssert.Contains("There is no ValidatorXML specified for the Catalogue TestTable",ex.Message);
        }

        [Test]
        public void SupportsValidation_BadXML()
        {
            CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

            _catalogue.ValidatorXML = "fish";
            //it has no validator XML currently 
            Assert.IsFalse(report.CatalogueSupportsReport(_catalogue));

            var ex = Assert.Throws<Exception>(()=>report.Check(new ThrowImmediatelyCheckNotifier()));
            StringAssert.Contains("ValidatorXML for Catalogue TestTable could not be deserialized into a Validator",ex.Message);
        }

        [Test]
        public void SupportsValidation_MadeUpColumnName()
        {

            CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

            _catalogue.ValidatorXML = dodgyColumnXML;
            //it has no validator XML currently 
            Assert.IsFalse(report.CatalogueSupportsReport(_catalogue));

            var ex = Assert.Throws<Exception>(()=>report.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("Could not find a column in the extraction SQL that would match TargetProperty chi",ex.Message);
        }

        [Test]
        public void SupportsValidation_GoodButNoDataLoadRunID()
        {
            CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);

            _catalogue.ValidatorXML = validColumnXML;
            
            //set the time periodicity field
            var toBeTimePeriodicityCol = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => e.GetRuntimeName().Equals("PrivateID"));
            _catalogue.TimeCoverage_ExtractionInformation_ID = toBeTimePeriodicityCol.ID;

            var notifier = new ToMemoryCheckNotifier();
            report.Check(notifier);

            Assert.AreEqual(CheckResult.Warning, notifier.GetWorst());
            Assert.Contains("Found column in query builder columns which matches TargetProperty Name",notifier.Messages.Select(m=>m.Message).ToArray());
            
            Assert.IsTrue(report.CatalogueSupportsReport(_catalogue));

            var ex = Assert.Throws<Exception>(() => report.Check(new ThrowImmediatelyCheckNotifier() {ThrowOnWarning = true}));
            Assert.IsTrue(ex.Message == "Did not find ExtractionInformation for a column called hic_dataLoadRunID, this will prevent you from viewing the resulting report subdivided by data load batch (make sure you have this column and that it is marked as extractable)");
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
}
