using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using DataExportLibrary.Data.DataTables;
using DataExportManager.Wizard;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewDataExtractionProject : BasicUICommandExecution, IAtomicCommand
    {
        public ExecuteCommandCreateNewDataExtractionProject(IActivateItems activator) : base(activator)
        {
        }

        public override void Execute()
        {
            base.Execute();
            var wizard = new CreateNewDataExtractionProjectUI(Activator);
            if(wizard.ShowDialog() == DialogResult.OK && wizard.ExtractionConfigurationCreatedIfAny != null)
            {
                var p = (Project) wizard.ExtractionConfigurationCreatedIfAny.Project;
                Publish(p);
                Activator.RequestItemEmphasis(this, new EmphasiseRequest(p, int.MaxValue));
                
                //now execute it
                var executeCommand = new ExecuteCommandExecuteExtractionConfiguration(Activator).SetTarget(wizard.ExtractionConfigurationCreatedIfAny);
                if(!executeCommand.IsImpossible)
                    executeCommand.Execute(); 

            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Project, OverlayKind.Add);
        }

        public override string GetCommandHelp()
        {
            return
                "This will open a window which will guide you in the steps for creating a Data Extraction Project.\r\n" +
                "You will be asked to choose a Cohort, the Catalogues to extract and the destination folder.";
        }
    }
}