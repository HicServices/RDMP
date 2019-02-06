// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class AllExternalServersNodeMenu : RDMPContextMenuStrip
    {
        public AllExternalServersNodeMenu(RDMPContextMenuStripArgs args, AllExternalServersNode node) : base(args,node)
        {
            var assemblyDictionary = new Dictionary<ServerDefaults.PermissableDefaults, Assembly>();

            Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator, null,ServerDefaults.PermissableDefaults.None));

            Items.Add(new ToolStripSeparator());

            //Add(new ExecuteCommandConfigureDefaultServers());

            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.DQE, typeof(DataQualityEngine.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.WebServiceQueryCachingServer_ID, typeof(QueryCaching.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID, typeof(HIC.Logging.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID, typeof(IdentifierDump.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.ANOStore, typeof(ANOStore.Database.Class1).Assembly);
            assemblyDictionary.Add(ServerDefaults.PermissableDefaults.CohortIdentificationQueryCachingServer_ID, typeof(QueryCaching.Database.Class1).Assembly);

            foreach (KeyValuePair<ServerDefaults.PermissableDefaults, Assembly> kvp in assemblyDictionary)
                Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator, kvp.Value, kvp.Key));

            Add(new ExecuteCommandConfigureDefaultServers(_activator));
        }
    }
}