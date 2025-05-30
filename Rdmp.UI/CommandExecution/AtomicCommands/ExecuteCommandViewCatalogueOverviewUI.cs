﻿// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Overview;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewCatalogueOverviewUI : BasicUICommandExecution
    {
        private readonly Catalogue _catalogue;

        public ExecuteCommandViewCatalogueOverviewUI(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;

        }

        public override void Execute()
        {
            Activator.Activate<ViewCatalogueOverviewUI, Catalogue>(_catalogue);
        }
    }
}

