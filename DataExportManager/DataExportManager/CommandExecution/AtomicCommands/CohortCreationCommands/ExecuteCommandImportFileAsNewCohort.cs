using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandImportFileAsNewCohort:CohortCreationCommandExecution
    {
        private FileInfo _file;

        public ExecuteCommandImportFileAsNewCohort(IActivateItems activator) : base(activator)
        {
        }
        public ExecuteCommandImportFileAsNewCohort(IActivateItems activator,FileInfo file)
            : base(activator)
        {
            _file = file;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.ImportFile;
        }

        public override void Execute()
        {
            base.Execute();

            FlatFileToLoad flatFile;

            //if no explicit file has been chosen
            if(_file == null)
            {
                //get user to pick one
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                
                flatFile = new FlatFileToLoad(new FileInfo(ofd.FileName));
            }
            else
                flatFile = new FlatFileToLoad(_file);

            var request = GetCohortCreationRequest();
            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            var configureAndExecuteDialog = GetConfigureAndExecuteControl(request, "Uploading File " + flatFile.File.Name);

            //add the flat file to the dialog with an appropriate description of what they are trying to achieve
            configureAndExecuteDialog.AddInitializationObject(flatFile);
            configureAndExecuteDialog.TaskDescription = "You are trying to create a new cohort (list of patient identifiers) by importing a single data table from a file, you have just finished selecting the name/project for the new cohort (although it does not exist just yet).  This dialog requires you to select/create an appropriate pipeline to achieve this goal.  " + TaskDescriptionGenerallyHelpfulText;

            Activator.ShowWindow(configureAndExecuteDialog, true);
        }
    }
}
