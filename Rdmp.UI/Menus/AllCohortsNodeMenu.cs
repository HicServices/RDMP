// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class AllCohortsNodeMenu:RDMPContextMenuStrip
    {
        [UseWithObjectConstructor]
        public AllCohortsNodeMenu(RDMPContextMenuStripArgs args, AllCohortsNode node)
            : base(args, node)
        {
            Add(new ExecuteCommandShowSummaryOfCohorts(_activator));

            Add(new ExecuteCommandCreateNewCohortDatabaseUsingWizard(_activator));

            Items.Add("Create blank cohort database (Not recommended)", _activator.CoreIconProvider.GetImage(RDMPConcept.ExternalCohortTable, OverlayKind.Problem), (s, e) => AddBlankExternalCohortTable());
            
        }
        
        private void AddBlankExternalCohortTable()
        {
            var newExternalCohortTable = new ExternalCohortTable(RepositoryLocator.DataExportRepository,"Blank Cohort Source " + Guid.NewGuid(),DatabaseType.MicrosoftSQLServer);
            Publish(newExternalCohortTable);
            Activate(newExternalCohortTable);
        }
    }
}
