// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataQualityUIs;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.LoadExecutionUIs;
using CatalogueManager.PluginChildProvision;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using Dashboard.CommandExecution.AtomicCommands;
using Dashboard.Menus.MenuItems;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace Dashboard
{
    internal class DQEPluginUserInterface : PluginUserInterface
    {

        public DQEPluginUserInterface(IActivateItems itemActivator) : base(itemActivator)
        {

        }

        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            var c = o as Catalogue;

            if (c != null)
                return new[]{ new DQEMenuItem(ItemActivator,c)};

            var lmd = o as LoadMetadata;

            if (lmd != null)
                return GetMenuArray(
                    new ExecuteCommandViewLoadMetadataLogs(ItemActivator).SetTarget(lmd)
                    );

            return null;
        }

        public override IEnumerable<IAtomicCommand> GetAdditionalCommandsForControl(IRDMPSingleDatabaseObjectControl control, DatabaseEntity databaseEntity)
        {
            if(control is DQEExecutionControl)
                return new[] {new ExecuteCommandViewDQEResultsForCatalogue(ItemActivator){OverrideCommandName = "View Results..."}.SetTarget(databaseEntity)};

            if (control is ExecuteLoadMetadataUI)
                return new[] {new ExecuteCommandViewLoadMetadataLogs(ItemActivator, (LoadMetadata) databaseEntity)};

            return base.GetAdditionalCommandsForControl(control, databaseEntity);
        }
    }
}
