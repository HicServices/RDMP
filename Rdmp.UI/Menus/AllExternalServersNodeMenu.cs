// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.CatalogueLibrary.Data.Defaults;
using Rdmp.Core.CatalogueLibrary.Nodes;
using Rdmp.Core.Databases;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    internal class AllExternalServersNodeMenu : RDMPContextMenuStrip
    {
        public AllExternalServersNodeMenu(RDMPContextMenuStripArgs args, AllExternalServersNode node) : base(args,node)
        {
            var assemblyDictionary = new Dictionary<PermissableDefaults, IPatcher>();

            Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator, null,PermissableDefaults.None));

            Items.Add(new ToolStripSeparator());

            //Add(new ExecuteCommandConfigureDefaultServers());

            assemblyDictionary.Add(PermissableDefaults.DQE, new DataQualityEnginePatcher());
            assemblyDictionary.Add(PermissableDefaults.WebServiceQueryCachingServer_ID, new QueryCachingPatcher());
            assemblyDictionary.Add(PermissableDefaults.LiveLoggingServer_ID, new LoggingDatabasePatcher());
            assemblyDictionary.Add(PermissableDefaults.IdentifierDumpServer_ID, new IdentifierDumpDatabasePatcher());
            assemblyDictionary.Add(PermissableDefaults.ANOStore, new ANOStorePatcher());
            assemblyDictionary.Add(PermissableDefaults.CohortIdentificationQueryCachingServer_ID, new QueryCachingPatcher());

            foreach (var kvp in assemblyDictionary)
                Add(new ExecuteCommandCreateNewExternalDatabaseServer(_activator, kvp.Value, kvp.Key));

            Add(new ExecuteCommandConfigureDefaultServers(_activator));
        }
    }
}