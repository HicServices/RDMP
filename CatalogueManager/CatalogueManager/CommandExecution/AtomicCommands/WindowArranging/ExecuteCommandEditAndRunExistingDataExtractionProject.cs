using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandEditAndRunExistingDataExtractionProject : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        public Project Project { get; set; }

        [ImportingConstructor]
        public ExecuteCommandEditAndRunExistingDataExtractionProject(IActivateItems activator, Project project) : base(activator)
        {
            Project = project;
        }

        public ExecuteCommandEditAndRunExistingDataExtractionProject(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Project, OverlayKind.Edit);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            Project = (Project) target;
            return this;
        }

        public override void Execute()
        {
            if (Project == null)
                SetImpossible("You must choose a Data Extraction Project to edit.");

            base.Execute();
            Activator.WindowArranger.SetupEditDataExtractionProject(this, Project);
        }

        public override string GetCommandHelp()
        {
            return
                "This will take you to the Data Extraction Projects list and allow you to Run the selected project.\r\n" +
                "You must choose a Project from the list before proceeding.";
        }
    }
}