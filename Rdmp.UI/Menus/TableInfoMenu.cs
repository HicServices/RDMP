// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Alter;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;
using Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.Checks;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class TableInfoMenu : RDMPContextMenuStrip
    {

        public TableInfoMenu(RDMPContextMenuStripArgs args, TableInfo tableInfo)
            : base(args, tableInfo)
        {

            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, null, tableInfo),Keys.None,"New");
            Add(new ExecuteCommandAddJoinInfo(_activator, tableInfo),Keys.None,"New");
                                    
            Items.Add("Configure Primary Key Collision Resolution ", CatalogueIcons.CollisionResolution, delegate { ConfigurePrimaryKeyCollisionResolution_Click(tableInfo); });

            Items.Add(new ToolStripSeparator());
            Items.Add(new SetDumpServerMenuItem(_activator, tableInfo));
            Add(new ExecuteCommandCreateNewPreLoadDiscardedColumn(_activator, tableInfo));
            Items.Add(new ToolStripSeparator());

            if (tableInfo != null && tableInfo.IsTableValuedFunction)
                Items.Add("Configure Parameters...", _activator.CoreIconProvider.GetImage(RDMPConcept.ParametersNode), delegate { ConfigureTableInfoParameters(tableInfo); });
        }

        private void ConfigurePrimaryKeyCollisionResolution_Click(TableInfo tableInfo)
        {
            var dialog = new ConfigurePrimaryKeyCollisionResolverUI(tableInfo,_activator);
            dialog.ShowDialog(this);
        }


        
        private void ConfigureTableInfoParameters(TableInfo tableInfo)
        {
            ParameterCollectionUI.ShowAsDialog(_activator,new ParameterCollectionUIOptionsFactory().Create(tableInfo));
        }

    }
}
