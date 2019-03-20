// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Windows.Forms;
using CatalogueManager.Menus;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Providers.Nodes.UsedByProject;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExternalCohortTableMenu : RDMPContextMenuStrip
    {
        private readonly ExternalCohortTable _externalCohortTable;
        private Project _project;
        
        public ExternalCohortTableMenu(RDMPContextMenuStripArgs args, ExternalCohortTable externalCohortTable): base(args, externalCohortTable)
        {
            _externalCohortTable = externalCohortTable;

            Items.Add(new ToolStripSeparator());
            Add(new ExecuteCommandCreateNewCohortFromFile(_activator,_externalCohortTable));
            Add(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator,_externalCohortTable));
            Add(new ExecuteCommandCreateNewCohortFromCatalogue(_activator,externalCohortTable));
            Items.Add(new ToolStripSeparator());

            var projectOnlyNode = args.Masquerader as CohortSourceUsedByProjectNode;

            if (projectOnlyNode != null)
                Add(new ExecuteCommandShowSummaryOfCohorts(_activator, projectOnlyNode));
            else
                Add(new ExecuteCommandShowSummaryOfCohorts(_activator, externalCohortTable));

            Add(new ExecuteCommandImportAlreadyExistingCohort(_activator, _externalCohortTable));
        }
    }
}
