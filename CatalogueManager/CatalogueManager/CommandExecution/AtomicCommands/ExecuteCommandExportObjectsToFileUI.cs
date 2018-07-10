using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using Sharing.CommandExecution;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandExportObjectsToFileUI : BasicUICommandExecution, IAtomicCommand
    {
        private readonly ExecuteCommandExportObjectsToFile _cmd;

        public ExecuteCommandExportObjectsToFileUI(IActivateItems activator, IMapsDirectlyToDatabaseTable[] toExport,DirectoryInfo targetDirectoryInfo = null) : base(activator)
        {
            _cmd = new ExecuteCommandExportObjectsToFile(activator.RepositoryLocator, toExport, targetDirectoryInfo);

            if(_cmd.IsImpossible)
                SetImpossible(_cmd.ReasonCommandImpossible);
        }

        public override string GetCommandHelp()
        {
            return "Creates a share file with definitions for the supplied objects and all children";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_put;
        }

        public override void Execute()
        {
            base.Execute();

            if (_cmd.TargetDirectoryInfo == null)
            {
                var fb = new FolderBrowserDialog();
                if (fb.ShowDialog() == DialogResult.OK)
                    _cmd.TargetDirectoryInfo = new DirectoryInfo(fb.SelectedPath);
                else
                    return;
            }
            
            _cmd.Execute();

            Process.Start("explorer.exe", _cmd.TargetDirectoryInfo.FullName);
        }

        public override string GetCommandName()
        {
            return "Export Object(s) to File...";
        }
    }
}