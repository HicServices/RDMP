// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.SubComponents;

namespace Rdmp.UI.Menus
{

    [System.ComponentModel.DesignerCategory("")]
    class CohortIdentificationConfigurationMenu :RDMPContextMenuStrip
    {
        public CohortIdentificationConfigurationMenu(RDMPContextMenuStripArgs args, CohortIdentificationConfiguration cic): base(args, cic)
        {
            Items.Add("View SQL", _activator.CoreIconProvider.GetImage(RDMPConcept.SQL), (s, e) => _activator.Activate<ViewCohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(cic));
            Add(new ExecuteCommandCreateNewQueryCacheDatabase(_activator,cic));
        }

        public CohortIdentificationConfigurationMenu(RDMPContextMenuStripArgs args, ProjectCohortIdentificationConfigurationAssociation association) : this(args, association.CohortIdentificationConfiguration)
        {
        }
    }
}
