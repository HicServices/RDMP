// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.PluginChildProvision
{
    public abstract class PluginUserInterface:IPluginUserInterface
    {
        protected readonly IActivateItems ItemActivator;
        
        protected PluginUserInterface(IActivateItems itemActivator)
        {
            ItemActivator = itemActivator;
        }

        public virtual object[] GetChildren(object model)
        {
            return null;
        }

        public virtual ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            return null;
        }

        public virtual IEnumerable<IAtomicCommand> GetAdditionalCommandsForControl(IRDMPSingleDatabaseObjectControl control, DatabaseEntity databaseEntity)
        {
            return null;
        }

        private AtomicCommandUIFactory _atomicCommandUIFactory  = null;
        protected ToolStripMenuItem GetMenuItem(IAtomicCommand cmd)
        {
            if(_atomicCommandUIFactory == null)
                _atomicCommandUIFactory = new AtomicCommandUIFactory(ItemActivator);

            return _atomicCommandUIFactory.CreateMenuItem(cmd);
        }

        protected ToolStripMenuItem[] GetMenuArray(params IAtomicCommand[] commands)
        {
            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();

            foreach (IAtomicCommand command in commands)
                items.Add(GetMenuItem(command));

            return items.ToArray();
        }

        public virtual Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual bool ShowPluginAggregateConfigurationEditWindow(AggregateConfiguration ac)
        {
            return false;
        }
    }
}