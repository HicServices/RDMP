using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
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

        /// <summary>
        /// True to fetch all <see cref="IExtractionConfiguration"/> and check with <see cref="ExtractionConfigurationChecker"/>
        /// </summary>
        public bool CheckConfigurations { get; set; }

        /// <summary>
        /// True to fetch all <see cref="ISelectedDataSets"/> and check with <see cref="SelectedDataSetsChecker"/>
        /// </summary>
        public bool CheckDatasets { get; set; }

        /// <summary>
        /// Sets up the class to check the state of the <paramref name="project"/>
        /// </summary>
        /// <param name="repositoryLocator"></param>
        /// <param name="project"></param>
        public ProjectChecker(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IProject project)
        {
            _repositoryLocator = repositoryLocator;
            _project = project;
            CheckDatasets = true;
            CheckConfigurations = true;
        }

        /// <summary>
        /// Checks the <see cref="IProject"/> has a valid <see cref="IProject.ExtractionDirectory"/>, <see cref="IProject.ProjectNumber"/> etc and runs additional 
        /// checkers (See <see cref="CheckConfigurations"/> and <see cref="CheckDatasets"/>).
        /// </summary>
        /// <param name="notifier"></param>
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
