using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandSetProjectExtractionDirectory : BasicUICommandExecution,IAtomicCommand
    {
        private readonly Project _project;

        public ExecuteCommandSetProjectExtractionDirectory(IActivateItems activator, Project project) : base(activator)
        {
            _project = project;
        }

        public override string GetCommandHelp()
        {
            return "Change the location on disk where extracted artifacts are put when you run extraction configurations of this project";
        }

        public override void Execute()
        {
            base.Execute();

            using (var ofd = new FolderBrowserDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _project.ExtractionDirectory = ofd.SelectedPath;
                    _project.SaveToDatabase();
                    Publish(_project);
                }
            }

        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractionDirectoryNode,OverlayKind.Edit);
        }
    }
}