// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using System.IO;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandOpenInExplorer:BasicUICommandExecution,IAtomicCommand
    {
        private readonly FileInfo _file;
        private readonly DirectoryInfo _dir;
        
        public ExecuteCommandOpenInExplorer(IActivateItems activator,DirectoryInfo dir) : base(activator)
        {
            _dir = dir; 

            if (_dir == null || !_dir.Exists)
                SetImpossible("Directory not found");
        }
        public ExecuteCommandOpenInExplorer(IActivateItems activator, FileInfo file): base(activator)
        {
            _file = file;

            if(_file == null || !_file.Exists)
                SetImpossible("File not found");
        }

        public override void Execute()
        {
            base.Execute();

            if(_file != null)
                UsefulStuff.GetInstance().ShowFileInWindowsExplorer(_file);

            if(_dir != null)
                UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(_dir);
        }

        public override Image<Argb32> GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CatalogueFolder);
        }
    }
}
