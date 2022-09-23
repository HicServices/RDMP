// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;

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
        public override void Execute()
        {
            base.Execute();

            foreach (IHasFolder c in _toMove)
            {
                c.Folder = _folder;
                c.SaveToDatabase();
            }

            //Folder has changed so publish the change (but only change the last Catalogue so we don't end up subing a million global refreshes changes)
            Publish(_toMove.Last());
        }
    }
}