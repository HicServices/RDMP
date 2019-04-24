// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using CatalogueManager.DataViewing;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.Data;

namespace CatalogueManager.Menus
{
    class FilterMenu : RDMPContextMenuStrip
    {
        public FilterMenu(RDMPContextMenuStripArgs args, IFilter filter): base(args, (DatabaseEntity)filter)
        {
            Add(new ExecuteCommandViewFilterMatchData(args.ItemActivator, filter, ViewType.TOP_100));
            Add(new ExecuteCommandViewFilterMatchData(args.ItemActivator, filter, ViewType.Aggregate));
            Add(new ExecuteCommandViewFilterMatchGraph(_activator, filter));

            Items.Add(new ToolStripSeparator());

            var dis = filter as IDisableable;
            if (dis != null)
                Add(new ExecuteCommandDisableOrEnable(_activator, dis));

            Add(new ExecuteCommandExportObjectsToFileUI(_activator, new[] {filter}));
            Add(new ExecuteCommandImportFilterDescriptionsFromShare(_activator, filter));
        }
    }
}