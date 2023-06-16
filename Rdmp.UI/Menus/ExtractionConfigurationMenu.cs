// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ProjectUI;

namespace Rdmp.UI.Menus;

[System.ComponentModel.DesignerCategory("")]
internal class ExtractionConfigurationMenu:RDMPContextMenuStrip
{
    public ExtractionConfigurationMenu(RDMPContextMenuStripArgs args, ExtractionConfiguration extractionConfiguration)
        : base(args, extractionConfiguration)
    {
        Items.Add("Edit", null,
            (s, e) => _activator.Activate<ExtractionConfigurationUI, ExtractionConfiguration>(extractionConfiguration));

        Add(new ExecuteCommandRelease(_activator) { Weight = -99.5f }.SetTarget(extractionConfiguration));
        Add(new ExecuteCommandRefreshExtractionConfigurationsCohort(_activator, extractionConfiguration));

        Add(new ExecuteCommandOpenExtractionDirectory(_activator, extractionConfiguration));

        ReBrandActivateAs("Configure/Run Extract...", RDMPConcept.ExtractionConfiguration, OverlayKind.Execute);
    }
}