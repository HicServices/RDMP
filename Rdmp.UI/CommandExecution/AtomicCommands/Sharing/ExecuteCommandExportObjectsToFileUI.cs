// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands.Sharing
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

        public override Image GetImage(IIconProvider iconProvider)
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