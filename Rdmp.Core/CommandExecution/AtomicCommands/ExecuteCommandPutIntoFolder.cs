// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandPutIntoFolder: BasicCommandExecution
    {
        private readonly IHasFolder[] _toMove;
        private readonly string _folder;
        
        public ExecuteCommandPutIntoFolder(IBasicActivateItems activator, IHasFolderCombineable cmd, string targetModel)
            :this(activator,new []{cmd.Folderable},targetModel)
        {
            
        }
        public ExecuteCommandPutIntoFolder(IBasicActivateItems activator, ManyCataloguesCombineable cmd, string targetModel)
            : this(activator, cmd.Catalogues, targetModel)
        {
            
        }

        [UseWithObjectConstructor]
        public ExecuteCommandPutIntoFolder(IBasicActivateItems activator, IHasFolder[] toMove, string folder) : base(activator)
        {
            _folder = folder;
            _toMove = toMove;
        }

        public override Image<Rgba32> GetImage(IIconProvider iconProvider)
        {
            if (OverrideIcon != null)
                return base.OverrideIcon;

            return Image.Load<Rgba32>(CatalogueIcons.CatalogueFolder);
        }
        public override void Execute()
        {
            base.Execute();

            var f = _folder;
            if(f == null)
            {
                if(BasicActivator.IsInteractive)
                {
                    if (!BasicActivator.TypeText(new DialogArgs
                    {
                        WindowTitle = "Folder",
                        TaskDescription = "Enter a new virtual folder for the object.  Folder names should be lower case and start with a backslash ('\\')",
                        EntryLabel = "New Folder"
                    }, 500, "\\", out f, false))
                        return;
                }
                else
                {
                    throw new Exception("No new folder value was passed and User Interface is not interactive");
                }
            }

            // user entered a blank string, treat that as cancellation
            if (string.IsNullOrWhiteSpace(f))
                return;

            foreach (IHasFolder c in _toMove)
            {
                c.Folder = f;
                c.SaveToDatabase();
            }

            //Folder has changed so publish the change (but only change the last Catalogue so we don't end up subing a million global refreshes changes)
            Publish(_toMove.Last());
        }
    }
}