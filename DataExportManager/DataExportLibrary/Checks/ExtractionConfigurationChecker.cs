using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Checks
{
    public class ExtractionConfigurationChecker:ICheckable
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private IExtractionConfiguration _config;
        public bool CheckDatasets { get; set; }
        public bool CheckGlobals { get; set; }

        public ExtractionConfigurationChecker(IRDMPPlatformRepositoryServiceLocator repositoryLocator,IExtractionConfiguration config)
        {
            _repositoryLocator = repositoryLocator;
            _config = config;
        }

        public void Check(ICheckNotifier notifier)
        {
            if (_config.IsReleased)
                CheckReleaseConfiguration(notifier);
            else
                CheckInProgressConfiguration(notifier);
        }

        private void CheckReleaseConfiguration(ICheckNotifier notifier)
        {
            var projectDirectory = new DirectoryInfo(_config.Project.ExtractionDirectory);

            notifier.OnCheckPerformed(new CheckEventArgs("Found Frozen/Released configuration '" + _config + "'", CheckResult.Success));

            foreach (DirectoryInfo directoryInfo in projectDirectory.GetDirectories(ExtractionDirectory.GetExtractionDirectoryPrefix(_config) + "*").ToArray())
            {
                string firstFileFound;

                if (DirectoryIsEmpty(directoryInfo, out firstFileFound))
                {
                    bool deleteIt =
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "Found empty folder " + directoryInfo.Name +
                                " which is left over extracted folder after data release", CheckResult.Warning, null,
                                "Delete empty folder"));

                    if (deleteIt)
                        directoryInfo.Delete(true);
                }
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Found non-empty folder " + directoryInfo.Name +
                            " which is left over extracted folder after data release (First file found was '" + firstFileFound + "' but there may be others)", CheckResult.Fail));
            }
        }

        private bool DirectoryIsEmpty(DirectoryInfo d, out string firstFileFound)
        {
            var found = d.GetFiles().FirstOrDefault();
            if (found != null)
            {
                firstFileFound = found.FullName;
                return false;
            }

            foreach (DirectoryInfo directory in d.GetDirectories())
                if (!DirectoryIsEmpty(directory, out firstFileFound))
                    return false;

            firstFileFound = null;
            return true;
        }

        private void CheckInProgressConfiguration(ICheckNotifier notifier)
        {
            var repo = (DataExportRepository)_config.Repository;
            notifier.OnCheckPerformed(new CheckEventArgs("Found configuration '" + _config + "'", CheckResult.Success));

            var datasets = _config.GetAllExtractableDataSets().ToArray();

            foreach (ExtractableDataSet dataSet in datasets)
                if (dataSet.DisableExtraction)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Dataset " + dataSet +
                            " is set to DisableExtraction=true, probably someone doesn't want you extracting this dataset at the moment",
                            CheckResult.Fail));

            if (!datasets.Any())
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "There are no datasets selected for open configuration '" + _config + "'",
                        CheckResult.Fail));

            if (_config.Cohort_ID == null)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Open configuration '" + _config + "' does not have a cohort yet",
                        CheckResult.Fail));
                return;
            }

            var cohort = repo.GetObjectByID<ExtractableCohort>((int)_config.Cohort_ID);

            if (CheckDatasets)
                foreach (ISelectedDataSets s in _config.SelectedDataSets)
                    new SelectedDatasetsChecker(s, _repositoryLocator).Check(notifier);

            //globals
            if (CheckGlobals)
                if (datasets.Any())
                    foreach (SupportingSQLTable table in _config.GetGlobals().OfType<SupportingSQLTable>())
                        new SupportingSQLTableChecker(table).Check(notifier);
        }
    }
}