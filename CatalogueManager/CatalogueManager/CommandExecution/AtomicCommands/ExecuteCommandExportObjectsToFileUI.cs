using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using Sharing.CommandExecution;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandExportObjectsToFileUI : BasicUICommandExecution, IAtomicCommand
    {
        private readonly IMapsDirectlyToDatabaseTable[] _toExport;
        private readonly ExecuteCommandExportObjectsToFile _cmd;
        public bool ShowInExplorer { get; set; }

        public ExecuteCommandExportObjectsToFileUI(IActivateItems activator, IMapsDirectlyToDatabaseTable[] toExport,DirectoryInfo targetDirectoryInfo = null) : base(activator)
        {
            _toExport = toExport;
            _cmd = new ExecuteCommandExportObjectsToFile(activator.RepositoryLocator, toExport, targetDirectoryInfo);

            if(_cmd.IsImpossible)
                SetImpossible(_cmd.ReasonCommandImpossible);

            ShowInExplorer = true;
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

            if (_cmd.IsSingleObject)
            {
                //Extract a single object (to file)
                if (_cmd.TargetFileInfo == null)
                {
                    var sfd = new SaveFileDialog();
                    sfd.Filter = "Share Definition|*.sd";
                    sfd.FileName = _toExport.Single() +".sd";
                    if(sfd.ShowDialog() == DialogResult.OK)
                        _cmd.TargetFileInfo = new FileInfo(sfd.FileName);
                    else
                        return;
                }
            }
            else
            {
                if (_cmd.TargetDirectoryInfo == null)
                {
                    var fb = new FolderBrowserDialog();
                    if (fb.ShowDialog() == DialogResult.OK)
                        _cmd.TargetDirectoryInfo = new DirectoryInfo(fb.SelectedPath);
                    else
                        return;
                }
            }
            
            _cmd.Execute();

            if (ShowInExplorer && _cmd.TargetDirectoryInfo != null)
                UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(_cmd.TargetDirectoryInfo);
        }

        public override string GetCommandName()
        {
            return "Export Object(s) to File...";
        }
    }
}