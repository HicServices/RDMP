// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.SimpleFileImporting;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewCatalogueByImportingFileUI : ExecuteCommandCreateNewCatalogueByImportingFile
{
    private readonly IActivateItems _activator;

    public ExecuteCommandCreateNewCatalogueByImportingFileUI(IActivateItems activator, FileInfo file = null) : base(
        activator, file)
    {
        _activator = activator;
    }

    public override void Execute()
    {
        if (IsImpossible)
            throw new ImpossibleCommandException(this, ReasonCommandImpossible);

        var ui = new CreateNewCatalogueByImportingFileUI(_activator, this);
        ui.SetProjectSpecific(ProjectSpecific);
        ui.ShowDialog();
    }
}