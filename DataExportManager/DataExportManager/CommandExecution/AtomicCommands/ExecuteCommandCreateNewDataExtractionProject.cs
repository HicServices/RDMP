using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportManager.ItemActivation;
using DataExportManager.ProjectUI;
using DataExportManager.Wizard;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewDataExtractionProject : BasicCommandExecution, IAtomicCommand
    {
        private readonly IActivateItems _activator;

        public ExecuteCommandCreateNewDataExtractionProject(IActivateItems activator)
        {
            this._activator = activator;
            
        }

        public override void Execute()
        {
            base.Execute();
            var wizard = new CreateNewDataExtractionProjectUI(_activator);
            if(wizard.ShowDialog() == DialogResult.OK && wizard.ExtractionConfigurationCreatedIfAny != null)
            {
                var p = (Project) wizard.ExtractionConfigurationCreatedIfAny.Project;
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(p));
                _activator.RequestItemEmphasis(this, new EmphasiseRequest(p, int.MaxValue));
                ((IActivateDataExportItems)_activator).ExecuteExtractionConfiguration(this, 
                    new ExecuteExtractionUIRequest(wizard.ExtractionConfigurationCreatedIfAny)
                    {
                        AutoStart = true
                    });
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