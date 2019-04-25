// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Nodes;
using Rdmp.UI.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ParametersNodeMenu : RDMPContextMenuStrip
    {
        public ParametersNodeMenu(RDMPContextMenuStripArgs args, ParametersNode parameterNode): base(args, parameterNode)
        {
            var filter = parameterNode.Collector as ExtractionFilter;

            if (filter != null)
                Items.Add(new ToolStripMenuItem("Add New 'Known Good Value(s) Set'", GetImage(RDMPConcept.ExtractionFilterParameterSet, OverlayKind.Add), (s, e) => AddParameterValueSet(filter)));
        }

        private void AddParameterValueSet(ExtractionFilter filter)
        {
            var parameterSet = new ExtractionFilterParameterSet(RepositoryLocator.CatalogueRepository,filter);
            parameterSet.CreateNewValueEntries();
            Publish(filter);
            Activate(parameterSet);
        }
    }
}
