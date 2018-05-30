using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
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
        private readonly IProject _project;
        IExtractionConfiguration[] _extractionConfigurations;
        private DirectoryInfo _projectDirectory;

        public bool CheckConfigurations { get; set; }
        public bool CheckDatasets { get; set; }

        public ProjectChecker(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IProject project)
        {
            _repositoryLocator = repositoryLocator;
            _project = project;
            CheckDatasets = true;
            CheckConfigurations = true;
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

            if (CheckConfigurations)
                foreach (IExtractionConfiguration extractionConfiguration in _extractionConfigurations)
                {
                    var extractionConfigurationChecker = new ExtractionConfigurationChecker(_repositoryLocator,extractionConfiguration) {CheckDatasets = CheckDatasets};
                    extractionConfigurationChecker.Check(notifier);
                }
            
            notifier.OnCheckPerformed(new CheckEventArgs("All Project Checks Finished (Not necessarily without errors)", CheckResult.Success));
        }
    }
}
