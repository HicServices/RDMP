// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ProjectMenu:RDMPContextMenuStrip
    {
        public ProjectMenu(RDMPContextMenuStripArgs args, Project project)
            : base(args,project)
        {
            Add(new ExecuteCommandRelease(_activator).SetTarget(project));
            Add(new ExecuteCommandExecuteExtractionConfiguration(_activator).SetTarget(project));

            args.SkipCommand<ExecuteCommandCreateNewCatalogueByImportingFile>();

            Add(new ExecuteCommandCreateNewCatalogueByImportingFileUI(_activator) { OverrideCommandName = "New Project Specific Catalogue From File...", SuggestedCategory = AtomicCommandFactory.Add, Weight = -1.9f });
        }

    }
}
