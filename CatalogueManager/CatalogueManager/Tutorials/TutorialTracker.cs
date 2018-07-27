using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.SimpleDialogs.NavigateTo;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportManager.CommandExecution.AtomicCommands;
using RDMPStartup;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.Settings;
using ReusableUIComponents.Settings;
using ReusableUIComponents.TransparentHelpSystem;
using ReusableUIComponents.TransparentHelpSystem.ProgressTracking;

namespace CatalogueManager.Tutorials
{
    public class TutorialTracker : IHelpWorkflowProgressProvider
    {
        private readonly IActivateItems _activator;
        
        public List<Tutorial> TutorialsAvailable { get; private set; }

        public TutorialTracker(IActivateItems activator)
        {
            _activator = activator;
            
            BuildTutorialList();
        }

        private void BuildTutorialList()
        {
            TutorialsAvailable = new List<Tutorial>();
            TutorialsAvailable.Add(new Tutorial("1. Create Platform Databases", new ExecuteCommandChoosePlatformDatabase(_activator), new Guid("6e08b525-073d-46bb-ae4f-f152603fb0af")));
            TutorialsAvailable.Add(new Tutorial("2. Generate Test Data", new ExecuteCommandGenerateTestData(_activator), new Guid("8255fb4e-94a4-4bbc-9e8d-edec5ecebab0")));
            TutorialsAvailable.Add(new Tutorial("3. Import a file", new ExecuteCommandCreateNewCatalogueByImportingFile(_activator), new Guid("5d71a169-5c08-4c33-8f88-8ee123222a3b")));

            var executeExtraction = new Tutorial("4. Execute DataSet Extraction",
                                                 new ExecuteCommandExecuteExtractionConfiguration(_activator),
                                                 new Guid("ee8c290e-7905-4241-9b9a-0ba944fd1582"))
                {
                    UserHasSeen = true // this tutorial is only available on demand
                };
            TutorialsAvailable.Add(executeExtraction);
        }

        public bool ShouldShowUserWorkflow(HelpWorkflow workflow)
        {
            //all tutorials disabled
            if (UserSettings.DisableTutorials)
                return false;
            
            return !UserSettings.GetTutorialDone(GetTutorialGuidFromWorkflow(workflow));
        }

        private Guid GetTutorialGuidFromWorkflow(HelpWorkflow workflow)
        {
            //if the workflow has a guid then it isn't associated with a specific command
            if (workflow.WorkflowGuid != Guid.Empty)
                return workflow.WorkflowGuid;

            //workflow is associated with a specific Command, so it should have a Tutorial Available
            var tutorial = TutorialsAvailable.FirstOrDefault(t => t.CommandType == workflow.Command.GetType());

            if (tutorial == null)
                throw new Exception("Unexpected HelpWorkflow encountered, it doesn't have a tutorial description in TutorialsAvailable");

            return tutorial.Guid;
        }

        public void Completed(HelpWorkflow helpWorkflow)
        {
            UserSettings.SetTutorialDone(GetTutorialGuidFromWorkflow(helpWorkflow),true);
        }

        public void ClearCompleted()
        {
            foreach (Tutorial tutorial in TutorialsAvailable)
                UserSettings.SetTutorialDone(tutorial.Guid, false);
        }

        public void ClearCompleted(Tutorial tutorial)
        {
            UserSettings.SetTutorialDone(tutorial.Guid, false);
            UserSettings.DisableTutorials = false;
        }

        public void DisableAllTutorials()
        {
            UserSettings.DisableTutorials = true;
        }

        public bool HasSeen(Tutorial tutorial)
        {
            return UserSettings.GetTutorialDone(tutorial.Guid);
        }

        public void LaunchTutorial(Tutorial tutorial)
        {
            tutorial.CommandExecution.Execute();
        }

        public bool IsClearable()
        {
            //any that are true
            return TutorialsAvailable.Any(t=>UserSettings.GetTutorialDone(t.Guid));
        }
    }
}
