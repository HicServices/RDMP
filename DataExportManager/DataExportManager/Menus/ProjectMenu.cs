// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.CommandExecution.AtomicCommands;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ProjectMenu:RDMPContextMenuStrip
    {
        private readonly Project _project;

        public ProjectMenu(RDMPContextMenuStripArgs args, Project project)
            : base(args,project)
        {
            _project = project;

            Add(new ExecuteCommandRunChecksInPopupWindow(_activator, new ProjectChecker(RepositoryLocator, _project)));
            Add(new ExecuteCommandRelease(_activator).SetTarget(project));
            Add(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator).SetTarget(_project));
        }

    }
}
