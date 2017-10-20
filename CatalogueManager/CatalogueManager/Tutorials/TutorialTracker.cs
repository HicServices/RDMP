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
using RDMPStartup;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.TransparentHelpSystem;
using ReusableUIComponents.TransparentHelpSystem.ProgressTracking;

namespace CatalogueManager.Tutorials
{
    public class TutorialTracker : IHelpWorkflowProgressProvider
    {
        private readonly IActivateItems _activator;

        #region Instance Setup
        private static TutorialTracker _instance;
        private static object oInstance = new object();

        public static TutorialTracker GetInstance(IActivateItems activator)
        {
            lock (oInstance)
            {
                if(_instance == null)
                    _instance = new TutorialTracker(activator);
            }

            return _instance;
        }
        
        #endregion

        private FileInfo _progressFile;

        private const string AllTutorialsText = "DisableAllTutorials";

        private Dictionary<string, bool> TutorialSeen = new Dictionary<string, bool>();
        
        public List<Tutorial> TutorialsAvailable { get; private set; }

        private TutorialTracker(IActivateItems activator)
        {
            _activator = activator;

            var root = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RDMP"));

            if(!root.Exists)
                root.Create();

            _progressFile = new FileInfo(Path.Combine(root.FullName, "HelpProgress.txt"));

            BuildTutorialList();

            if (_progressFile.Exists)
            {
                var text = File.ReadAllText(_progressFile.FullName);

                //if there is a setting file
                if (!string.IsNullOrWhiteSpace(text))
                {
                    try
                    {
                        //read what they have seen so far
                        PersistStringHelper helper = new PersistStringHelper();
                        TutorialSeen = helper.LoadDictionaryFromString(text).ToDictionary(k=>k.Key,v=>Boolean.Parse(v.Value));
                    }
                    catch (Exception e)
                    {
                        //user has corrupted his save file
                        CreateNewDictionary();
                    }
                }
                else
                    CreateNewDictionary();
                    
            }
            else
                _progressFile.Create();
        }

        private void BuildTutorialList()
        {
            TutorialsAvailable = new List<Tutorial>();
            TutorialsAvailable.Add(new Tutorial("1. Create Platform Databases", new ExecuteCommandChoosePlatformDatabase(_activator)));
            TutorialsAvailable.Add(new Tutorial("2. Generate Test Data", new ExecuteCommandGenerateTestData(_activator)));
            TutorialsAvailable.Add(new Tutorial("3. Import a file",new ExecuteCommandCreateNewCatalogueByImportingFile(_activator)));
        }

        private void CreateNewDictionary()
        {
            TutorialSeen = new Dictionary<string, bool> { { AllTutorialsText, false } };
        }

        public void Save()
        {
            PersistStringHelper helper = new PersistStringHelper();
            var s = helper.SaveDictionaryToString(TutorialSeen.ToDictionary(k=>k.Key,v=>v.Value.ToString()));
            File.WriteAllText(_progressFile.FullName,s);
        }

        public bool ShouldShowUserWorkflow(HelpWorkflow workflow)
        {
            //all tutorials disabled
            if (TutorialSeen.ContainsKey(AllTutorialsText) && TutorialSeen[AllTutorialsText])
                return false;

            string type = GetDictionaryKey(workflow);

            //if user has seen the tutorial then don't show it to him
            if (TutorialSeen.ContainsKey(type))
                return !TutorialSeen[type];

            return true;
        }

        private string GetDictionaryKey(HelpWorkflow workflow)
        {
            return GetDictionaryKey(workflow.Command);
        }

        private string GetDictionaryKey(ICommandExecution command)
        {
            return command.GetType().Name;
        }
        public void Completed(HelpWorkflow helpWorkflow)
        {
            string type = GetDictionaryKey(helpWorkflow);

            if (TutorialSeen.ContainsKey(type))
                TutorialSeen[type] = true;
            else
                TutorialSeen.Add(type,true);

            Save();
        }

        public void ClearCompleted()
        {
            TutorialSeen.Clear();
            TutorialSeen.Add(AllTutorialsText,false);
        }
        public void ClearCompleted(Tutorial tutorial)
        {
            var key = GetDictionaryKey(tutorial.CommandExecution);

            if (TutorialSeen.ContainsKey(key))
                TutorialSeen[key] = false;

            //can't have global disable on either btw
            TutorialSeen[AllTutorialsText] = false;
        }

        public void DisableAllTutorials()
        {
            TutorialSeen[AllTutorialsText] = true;
        }

        public bool HasSeen(Tutorial tutorial)
        {
            var key = GetDictionaryKey(tutorial.CommandExecution);
            
            //if user has seen the tutorial then don't show it to him
            if (TutorialSeen.ContainsKey(key))
                return TutorialSeen[key];

            return false;
        }

        public void LaunchTutorial(Tutorial tutorial)
        {
            tutorial.CommandExecution.Execute();
        }

        public bool IsDisableAllTutorialsOn()
        {
            return TutorialSeen[AllTutorialsText];
        }

        public bool IsClearable()
        {
            //1 is AllTutorialsText 
            if (TutorialSeen.Count == 1)
                return false;

            //any that are true
            return TutorialSeen.Any(kvp => kvp.Value);

        }
    }
}
