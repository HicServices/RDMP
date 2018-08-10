using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandCreateNewCohortFromFile:CohortCreationCommandExecution
    {
        private FileInfo _file;

        public ExecuteCommandCreateNewCohortFromFile(IActivateItems activator, ExternalCohortTable externalCohortTable = null) : base(activator)
        {
            ExternalCohortTable = externalCohortTable;
        }
        public ExecuteCommandCreateNewCohortFromFile(IActivateItems activator, FileInfo file, ExternalCohortTable externalCohortTable = null)
            : base(activator)
        {
            _file = file;
            ExternalCohortTable = externalCohortTable;
        }

        public override string GetCommandHelp()
        {
            return "Create a cohort containing ALL the patient identifiers in the chosen file";
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
            
            var request = GetCohortCreationRequest("Patient identifiers in file '" + flatFile.File.FullName +"'");

            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            request.FileToLoad = flatFile;

            var configureAndExecuteDialog = GetConfigureAndExecuteControl(request, "Uploading File " + flatFile.File.Name);

            //add the flat file to the dialog with an appropriate description of what they are trying to achieve
            
            Activator.ShowWindow(configureAndExecuteDialog, true);
        }
    }
}
