// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.DataQualityUIs;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.LoadExecutionUIs;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.PluginChildProvision;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Validation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace Rdmp.UI
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
            if(control is DQEExecutionControlUI)
                return new[] {new ExecuteCommandViewDQEResultsForCatalogue(ItemActivator){OverrideCommandName = "View Results..."}.SetTarget(databaseEntity)};

            if (control is ValidationSetupUI)
                return new[]
                {
                    new ExecuteCommandRunDQEOnCatalogue(ItemActivator).SetTarget(control.DatabaseObject),
                    new ExecuteCommandViewDQEResultsForCatalogue(ItemActivator) { OverrideCommandName = "View Results..." }.SetTarget(databaseEntity)
                
                };

            if (control is ExecuteLoadMetadataUI)
                return new[] {new ExecuteCommandViewLoadMetadataLogs(ItemActivator, (LoadMetadata) databaseEntity)};

            return base.GetAdditionalCommandsForControl(control, databaseEntity);
        }
    }
}
