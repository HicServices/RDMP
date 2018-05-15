using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Checks
{
    /// <summary>
    /// Runs checks on a Project to make sure it has an extraction folder, project number etc.  Also checks ExtractionConfigurations that are part of the project
    /// to see if valid queries can be built and rows read.
    /// </summary>
    public class ProjectChecker:ICheckable
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly Project _project;
        IExtractionConfiguration[] _extractionConfigurations;
        private DirectoryInfo _projectDirectory;

        public ProjectChecker(IRDMPPlatformRepositoryServiceLocator repositoryLocator, Project project)
        {
            _repositoryLocator = repositoryLocator;
            _project = project;
        }

        public void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("About to check project " + _project.Name + " (ID="+_project.ID+")", CheckResult.Success));

            _extractionConfigurations = _project.ExtractionConfigurations;

            if (!_extractionConfigurations.Any())
                notifier.OnCheckPerformed(new CheckEventArgs("Project does not have any ExtractionConfigurations yet",CheckResult.Fail));

            if (_project.ProjectNumber == null)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Project does not have a Project Number, this is a number which is meaningful to you (as opposed to ID which is the systems number for the Project) and must be unique.",
                        CheckResult.Fail));

            //check to see if there is an extraction directory
            if (string.IsNullOrWhiteSpace(_project.ExtractionDirectory))
                notifier.OnCheckPerformed(new CheckEventArgs("Project does not have an ExtractionDirectory",CheckResult.Fail));
            try
            {
                //see if they punched the keyboard instead of typing a legit folder
                _projectDirectory = new DirectoryInfo(_project.ExtractionDirectory);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Project ExtractionDirectory ('" + _project.ExtractionDirectory +"') is not a valid directory name ", CheckResult.Fail, e));
                return;
            }
            
            //tell them whether it exists or not
            notifier.OnCheckPerformed(new CheckEventArgs("Project ExtractionDirectory ('" + _project.ExtractionDirectory+"') " + (_projectDirectory.Exists ? "Exists" : "Does Not Exist"), _projectDirectory.Exists?CheckResult.Success : CheckResult.Fail));
            
            //complain if it is Z:\blah because not all users/services might have this mapped
            if (new DriveInfo(_projectDirectory.Root.ToString()).DriveType == DriveType.Network)
                notifier.OnCheckPerformed(new CheckEventArgs("Project ExtractionDirectory is on a mapped network drive (" + _projectDirectory.Root +") which might not be accessible to other data analysts",CheckResult.Warning));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Project ExtractionDirectory is not a network drive (which is a good thing)", CheckResult.Success));
            
            foreach (ExtractionConfiguration config in _extractionConfigurations)
            {
                if (config.IsReleased)
                    CheckReleaseConfiguration(config,notifier);
                else
                    CheckInProgressConfiguration(config,notifier);
            }
            
            notifier.OnCheckPerformed(new CheckEventArgs("All Project Checks Finished (Not necessarily without errors)", CheckResult.Success));
        }

        private void CheckInProgressConfiguration(ExtractionConfiguration config, ICheckNotifier notifier)
        {
            var repo = (DataExportRepository)config.Repository;
            notifier.OnCheckPerformed(new CheckEventArgs("Found configuration '" + config +"'", CheckResult.Success));

            var datasets = config.GetAllExtractableDataSets().ToArray();

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
                        "There are no datasets selected for open configuration '" + config +"'",
                        CheckResult.Fail));

            if(config.Cohort_ID == null)
            {

                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Open configuration '" + config + "' does not have a cohort yet",
                        CheckResult.Fail));
                return;
            }


            var cohort = repo.GetObjectByID<ExtractableCohort>((int) config.Cohort_ID);


            foreach (SelectedDataSets s in config.SelectedDataSets)
                new SelectedDatasetsChecker(s, _repositoryLocator).Check(notifier);

            //globals
            if(datasets.Any())
                foreach (SupportingSQLTable table in datasets.First().Catalogue.GetAllSupportingSQLTablesForCatalogue(FetchOptions.ExtractableGlobals))
                    new SupportingSQLTableChecker(table).Check(notifier);
        }

        

        

        private void CheckReleaseConfiguration(ExtractionConfiguration config, ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Found Frozen/Released configuration '" + config +"'",CheckResult.Success));

            foreach (DirectoryInfo directoryInfo in _projectDirectory.GetDirectories(ExtractionDirectory.GetExtractionDirectoryPrefix(config) + "*").ToArray())
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

    }
}
