using NPOI.SS.Formula.Functions;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Checks.Checkers;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.DataProvider;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.DataQualityEngine.Reports;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Tests.DataLoad.Engine.Integration;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

namespace Rdmp.Core.Tests.DataQualityEngine
{
    internal class DQEPartialUpdateTests : DataLoadEngineTestsBase
    {

        readonly string validatorXML = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<Validator xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <ItemValidators>\r\n    <ItemValidator>\r\n      <PrimaryConstraint xsi:type=\"Chi\">\r\n        <Consequence>Wrong</Consequence>\r\n      </PrimaryConstraint>\r\n      <TargetProperty>chi</TargetProperty>\r\n      <SecondaryConstraints />\r\n    </ItemValidator>\r\n    <ItemValidator>\r\n      <TargetProperty>time</TargetProperty>\r\n      <SecondaryConstraints />\r\n    </ItemValidator>\r\n  </ItemValidators>\r\n</Validator>";
        readonly string fileLocation = Path.GetTempPath();
        readonly string fileName = "SteppedDQEPartialUpdates.csv";

        [Test]
        public void SteppedDQEPartialUpdates()
        {
            var server = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("value");
            dt.Columns.Add("time");
            dt.Columns.Add("hic_dataLoadRunID");
            dt.Rows.Add(new object[] { "1111111111", "A", "2024-12-01",10 });
            dt.Rows.Add(new object[] { "1111111112", "A", "2024-11-01",10 });

            var table = server.CreateTable("PartialToaDQE", dt);
            table.CreatePrimaryKey(table.DiscoverColumns().Where(c => c.GetRuntimeName() == "chi").ToArray());
            dt.Dispose();
            var catalogue = new Catalogue(CatalogueRepository, "PartialToaDQE");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            foreach (var columnInfo in _columnInfos)
            {
                var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
                ci.SaveToDatabase();
                var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
                ei.SaveToDatabase();
            }
            var dqeRepository = new DQERepository(CatalogueRepository);

            catalogue.ValidatorXML = validatorXML;
            catalogue.TimeCoverage_ExtractionInformation_ID = catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => e.GetRuntimeName().Equals("time")).ID;

            catalogue.PivotCategory_ExtractionInformation_ID = catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
           .Single(e => e.GetRuntimeName().Equals("value")).ID;

            var report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID)
            {
                ExplicitDQERepository = dqeRepository
            };

            report.Check(ThrowImmediatelyCheckNotifier.Quiet);
            var source = new CancellationTokenSource();

            var listener = new ToMemoryDataLoadEventListener(false);
            report.GenerateReport(catalogue, listener, source.Token);
            var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");
            lmd.LocationOfForLoadingDirectory = Path.GetTempPath();
            lmd.LocationOfForArchivingDirectory = Path.GetTempPath();
            lmd.LocationOfExecutablesDirectory = Path.GetTempPath();
            lmd.LocationOfCacheDirectory = Path.GetTempPath();
            lmd.SaveToDatabase();
            var loggingServer = CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
            var logManager = new Core.Logging.LogManager(loggingServer);
            logManager.CreateNewLoggingTaskIfNotExists(lmd.Name);
            catalogue.LoggingDataTask = lmd.Name;
            catalogue.SaveToDatabase();
            lmd.LinkToCatalogue(catalogue);

            //fetch files
            var fetchDataProcessTask = new ProcessTask(CatalogueRepository, lmd, LoadStage.GetFiles);
            fetchDataProcessTask.ProcessTaskType = ProcessTaskType.DataProvider;
            fetchDataProcessTask.Path = "Rdmp.Core.DataLoad.Modules.DataProvider.ImportFilesDataProvider";
            fetchDataProcessTask.SaveToDatabase();
           
            fetchDataProcessTask.CreateArgumentsForClassIfNotExists<ImportFilesDataProvider>();
            fetchDataProcessTask.SetArgumentValue("DirectoryPath", fileLocation);
            fetchDataProcessTask.SetArgumentValue("FilePattern", fileName);
            fetchDataProcessTask.SaveToDatabase();

            //load file
            var attachProcessTask = new ProcessTask(CatalogueRepository, lmd, LoadStage.Mounting);
            attachProcessTask.ProcessTaskType = ProcessTaskType.Attacher;
            attachProcessTask.Path = "Rdmp.Core.DataLoad.Modules.Attachers.AnySeparatorFileAttacher";
            attachProcessTask.SaveToDatabase();
            attachProcessTask.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>();
            attachProcessTask.SetArgumentValue("Separator", ",");
            attachProcessTask.SetArgumentValue("FilePattern", fileName);
            attachProcessTask.SetArgumentValue("TableToLoad", _tableInfo);
            attachProcessTask.SaveToDatabase();

            var dqeUpdate = new ProcessTask(CatalogueRepository, lmd, LoadStage.PostLoad);
            dqeUpdate.ProcessTaskType = ProcessTaskType.MutilateDataTable;
            dqeUpdate.Path = "Rdmp.Core.DataLoad.Modules.Mutilators.DQEPostLoadRunner";
            dqeUpdate.CreateArgumentsForClassIfNotExists<DQEPostLoadRunner>();
            dqeUpdate.SaveToDatabase();

            //first load
            dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("value");
            dt.Columns.Add("time");
            dt.Rows.Add(new string[] { "1111111111", "A", "2024-12-01" });
            dt.Rows.Add(new string[] { "1111111112", "A", "2024-11-01" });
            dt.Rows.Add(new string[] { "1111111113", "B", "2024-10-01" });
            SetupFile(dt);
            PerformLoad(lmd, logManager);
            //end of first load
            report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID)
            {
                ExplicitDQERepository = dqeRepository
            };

            report.Check(ThrowImmediatelyCheckNotifier.Quiet);
            report.GenerateReport(catalogue, listener, source.Token);

            var evaluations = dqeRepository.GetAllObjectsWhere<Evaluation>("CatalogueID", catalogue.ID).ToList();
            Assert.That(evaluations.Count, Is.EqualTo(3));
            CompareEvaluations(evaluations[1], evaluations[2]);

            //second load
            dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("value");
            dt.Columns.Add("time");
            dt.Rows.Add(new string[] { "1111111111", "A", "2024-12-01" });
            dt.Rows.Add(new string[] { "1111111112", "A", "2024-11-01" });
            dt.Rows.Add(new string[] { "1111111113", "C", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111114", "D", "2024-10-01" });
            SetupFile(dt);

            PerformLoad(lmd, logManager);
            //end of second load
            report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID)
            {
                ExplicitDQERepository = dqeRepository
            };

            report.Check(ThrowImmediatelyCheckNotifier.Quiet);
            report.GenerateReport(catalogue, listener, source.Token);

            evaluations = dqeRepository.GetAllObjectsWhere<Evaluation>("CatalogueID", catalogue.ID).ToList();
            Assert.That(evaluations.Count, Is.EqualTo(5));
            CompareEvaluations(evaluations[3], evaluations[4]);

            //third load
            dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("value");
            dt.Columns.Add("time");
            dt.Rows.Add(new string[] { "1111111111", "C", "2024-12-01" });
            dt.Rows.Add(new string[] { "1111111112", "A", "2024-11-01" });
            dt.Rows.Add(new string[] { "1111111113", "C", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111114", "A", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111115", "B", "2024-09-01" });
            dt.Rows.Add(new string[] { "1111111116", "E", "2024-08-01" });
            SetupFile(dt);

            PerformLoad(lmd, logManager);
            //end of third load
            report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID)
            {
                ExplicitDQERepository = dqeRepository
            };

            report.Check(ThrowImmediatelyCheckNotifier.Quiet);
            report.GenerateReport(catalogue, listener, source.Token);

            evaluations = dqeRepository.GetAllObjectsWhere<Evaluation>("CatalogueID", catalogue.ID).ToList();
            Assert.That(evaluations.Count, Is.EqualTo(7));
            CompareEvaluations(evaluations[6], evaluations[5]);

            //fourth load
            dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("value");
            dt.Columns.Add("time");
            dt.Rows.Add(new string[] { "1111111111", "A", "2024-12-01" });
            dt.Rows.Add(new string[] { "1111111112", "A", "2024-11-01" });
            dt.Rows.Add(new string[] { "1111111113", "A", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111114", "D", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111115", "B", "2024-09-01" });
            dt.Rows.Add(new string[] { "1111111116", "C", "2024-08-01" });
            SetupFile(dt);

            PerformLoad(lmd, logManager);
            //end of fourth load
            report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID)
            {
                ExplicitDQERepository = dqeRepository
            };

            report.Check(ThrowImmediatelyCheckNotifier.Quiet);
            report.GenerateReport(catalogue, listener, source.Token);

            evaluations = dqeRepository.GetAllObjectsWhere<Evaluation>("CatalogueID", catalogue.ID).ToList();//.Where(e => e.CatalogueID == catalogue.ID).ToList();
            Assert.That(evaluations.Count, Is.EqualTo(9));
            CompareEvaluations(evaluations[8], evaluations[7]);

            //fifth load
            dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("value");
            dt.Columns.Add("time");
            dt.Rows.Add(new string[] { "1111111111", "C", "2024-12-01" });
            dt.Rows.Add(new string[] { "1111111112", "B", "2024-11-01" });
            dt.Rows.Add(new string[] { "1111111113", "D", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111114", "A", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111115", "A", "2024-09-01" });
            dt.Rows.Add(new string[] { "1111111116", "A", "2024-08-01" });
            SetupFile(dt);

            PerformLoad(lmd, logManager);
            //end of fifth load
            report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID)
            {
                ExplicitDQERepository = dqeRepository
            };

            report.Check(ThrowImmediatelyCheckNotifier.Quiet);
            report.GenerateReport(catalogue, listener, source.Token);

            evaluations = dqeRepository.GetAllObjectsWhere<Evaluation>("CatalogueID", catalogue.ID).ToList();
            Assert.That(evaluations.Count, Is.EqualTo(11));
            CompareEvaluations(evaluations[10], evaluations[9]);

            //sixth load
            dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("value");
            dt.Columns.Add("time");
            dt.Rows.Add(new string[] { "1111111111", "C", "2024-12-01" });
            dt.Rows.Add(new string[] { "1111111112", "B", "2024-11-01" });
            dt.Rows.Add(new string[] { "1111111113", "B", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111114", "C", "2024-10-01" });
            dt.Rows.Add(new string[] { "1111111115", "D", "2024-09-01" });
            dt.Rows.Add(new string[] { "1111111116", "A", "2024-08-01" });
            dt.Rows.Add(new string[] { "1111111117", "A", "2024-07-01" });
            dt.Rows.Add(new string[] { "1111111118", "B", "2024-06-01" });
            dt.Rows.Add(new string[] { "1111111119", "C", "2024-05-01" });
            dt.Rows.Add(new string[] { "1111111120", "D", "2024-04-01" });
            dt.Rows.Add(new string[] { "1111111121", "E", "2024-03-01" });
            SetupFile(dt);

            PerformLoad(lmd, logManager);
            //end of sixth load
            report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID)
            {
                ExplicitDQERepository = dqeRepository
            };

            report.Check(ThrowImmediatelyCheckNotifier.Quiet);
            report.GenerateReport(catalogue, listener, source.Token);
            source.Dispose();
            evaluations = dqeRepository.GetAllObjectsWhere<Evaluation>("CatalogueID", catalogue.ID).ToList();
            Assert.That(evaluations.Count, Is.EqualTo(13));
            CompareEvaluations(evaluations[12], evaluations[11]);
        }

        private void SetupFile(DataTable dt)
        {
            if (File.Exists(Path.Combine(fileLocation, fileName)))
            {
                File.Delete(Path.Combine(fileLocation, fileName));
            }
            var fs = File.Create(Path.Combine(fileLocation, fileName));
            fs.Close();
            var lines = new List<string>() {
             string.Join(',',dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName))
            };
            foreach (var row in dt.AsEnumerable())
            {
                lines.Add(string.Join(',', row.ItemArray.Select(i => i.ToString())));
            }
            File.AppendAllLines(Path.Combine(fileLocation, fileName), lines);
            dt.Dispose();
        }

        private void CompareEvaluations(Evaluation e1, Evaluation e2)
        {
            Assert.That(e1.ColumnStates.Length, Is.EqualTo(e2.ColumnStates.Length));
            Assert.That(e1.RowStates.Length, Is.EqualTo(e2.RowStates.Length));
            List<ColumnState> columnStateDiff = e1.ColumnStates.Except(e2.ColumnStates, new ColumnStateCompare()).ToList();
            Assert.That(columnStateDiff.Count, Is.EqualTo(0));
            columnStateDiff = e2.ColumnStates.Except(e1.ColumnStates, new ColumnStateCompare()).ToList();
            Assert.That(columnStateDiff.Count, Is.EqualTo(0));
            List<RowState> rowStateDiff = e1.RowStates.Except(e2.RowStates, new RowStateCompare()).ToList();
            Assert.That(rowStateDiff.Count, Is.EqualTo(0));
            rowStateDiff = e2.RowStates.Except(e1.RowStates, new RowStateCompare()).ToList();
            Assert.That(rowStateDiff.Count, Is.EqualTo(0));

            Assert.That(e1.GetPivotCategoryValues(), Is.EqualTo(e2.GetPivotCategoryValues()));
            foreach (var category in e1.GetPivotCategoryValues())
            {
                var e1Periodicity = PeriodicityState.GetPeriodicityForDataTableForEvaluation(e1, category, false);
                e1Periodicity.Columns.Remove("Evaluation_ID");
                var e2Periodicity = PeriodicityState.GetPeriodicityForDataTableForEvaluation(e2, category, false);
                e2Periodicity.Columns.Remove("Evaluation_ID");
                var differences =
    e1Periodicity.AsEnumerable().Except(e2Periodicity.AsEnumerable(),
                                            DataRowComparer.Default);
                Assert.That(differences.Any(), Is.False);

            }
        }

        private class ColumnStateCompare : IEqualityComparer<ColumnState>
        {
            public ColumnStateCompare()
            {
            }
            public bool Equals(ColumnState x, ColumnState y)
            {
                return x.TargetProperty == y.TargetProperty &&
          x.PivotCategory == y.PivotCategory &&
          x.CountCorrect == y.CountCorrect &&
          x.CountMissing == y.CountMissing &&
          x.CountWrong == y.CountWrong &&
          x.CountInvalidatesRow == y.CountInvalidatesRow &&
          x.CountDBNull == y.CountDBNull;
            }
            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }

            public int GetHashCode([DisallowNull] ColumnState obj)
            {
                return 1;
            }
        }

        private class RowStateCompare : IEqualityComparer<RowState>
        {
            public RowStateCompare()
            {
            }
            public bool Equals(RowState x, RowState y)
            {
                return x.Correct == y.Correct &&
                    x.Missing == y.Missing &&
                    x.Wrong == y.Wrong &&
                    x.Invalid == y.Invalid &&
                    x.PivotCategory == y.PivotCategory;

            }
            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }

            public int GetHashCode([DisallowNull] RowState obj)
            {
                return 1;
            }
        }


        private void PerformLoad(LoadMetadata lmd, Core.Logging.LogManager logManager)
        {
            var dbConfig = new HICDatabaseConfiguration(lmd, null);
            var projectDirectory = SetupLoadDirectory(lmd);
            var job = new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
              ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig);

            new PreExecutionChecker(lmd, dbConfig).Check(
               new AcceptAllCheckNotifier());

            var loadFactory = new HICDataLoadFactory(
                lmd,
                dbConfig,
                new HICLoadConfigurationFlags(),
                CatalogueRepository,
                logManager
            );

            var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

            var exitCode = exe.Run(
              job,
                new GracefulCancellationToken());

            Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));
        }

    }
}
