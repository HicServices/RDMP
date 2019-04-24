// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using Rdmp.Core.CatalogueLibrary.Data.Defaults;
using Rdmp.Core.CatalogueLibrary.Nodes;
using Rdmp.Core.Databases;

namespace CatalogueManager.Menus
{
    class AllANOTablesNodeMenu:RDMPContextMenuStrip
    {
        public AllANOTablesNodeMenu(RDMPContextMenuStripArgs args, AllANOTablesNode node): base(args, node)
        {
            Add(new ExecuteCommandCreateNewANOTable(_activator));
            
            Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                new ANOStorePatcher(), PermissableDefaults.ANOStore) 
                { OverrideCommandName = "Create ANOStore Database" });

            Add(new ExecuteCommandExportObjectsToFileUI(_activator,_activator.CoreChildProvider.AllANOTables));
        }
    }
}
