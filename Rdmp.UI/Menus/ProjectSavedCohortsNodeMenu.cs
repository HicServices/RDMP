// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.CohortCreationCommands;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.UI.CohortUI.ImportCustomData;

namespace Rdmp.UI.Menus
{
    class ProjectSavedCohortsNodeMenu:RDMPContextMenuStrip
    {
        public ProjectSavedCohortsNodeMenu(RDMPContextMenuStripArgs args, ProjectSavedCohortsNode savedCohortsNode): base(args, savedCohortsNode)
        {
            Add(new ExecuteCommandCreateNewCohortFromFile(_activator,null).SetTarget(savedCohortsNode.Project));
            Add(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator,null).SetTarget(savedCohortsNode.Project));
            Add(new ExecuteCommandCreateNewCohortFromCatalogue(_activator).SetTarget(savedCohortsNode.Project));
        }
    }
}
