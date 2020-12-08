// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Providers;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.DataViewing;
using Rdmp.UI.DataViewing.Collections;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExtractableCohortMenu:RDMPContextMenuStrip
    {
        private readonly ExtractableCohort _cohort;

        public ExtractableCohortMenu(RDMPContextMenuStripArgs args, ExtractableCohort cohort)
            : base(args,cohort)
        {
            _cohort = cohort;
            Items.Add("View TOP 100 identifiers",null, (s, e) => ViewTop100());

            Add(new ExecuteCommandDeprecate(args.ItemActivator, cohort, !cohort.IsDeprecated));

        }
        
        private void ViewTop100()
        {
            _activator.Activate<ViewSQLAndResultsWithDataGridUI>(new ViewCohortExtractionUICollection(_cohort));
        }

    }
}
