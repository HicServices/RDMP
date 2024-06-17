// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;

namespace Rdmp.UI.Menus;

[System.ComponentModel.DesignerCategory("")]
internal sealed class ColumnInfoMenu : RDMPContextMenuStrip
{
    public ColumnInfoMenu(RDMPContextMenuStripArgs args, ColumnInfo columnInfo) : base(args, columnInfo)
    {
        Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, null, columnInfo.TableInfo));
        Add(new ExecuteCommandLinkColumnInfoToDataSetUI(_activator, columnInfo));
        Items.Add(new ToolStripSeparator());

        Add(new ExecuteCommandAddJoinInfo(_activator, columnInfo.TableInfo));

        Add(new ExecuteCommandAnonymiseColumnInfo(_activator, columnInfo));
        Add(new ExecuteCommandUpdateCatalogueDataLocationUI(_activator, columnInfo));
    }
}