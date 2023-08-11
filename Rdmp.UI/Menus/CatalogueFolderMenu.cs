// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;

namespace Rdmp.UI.Menus;

[System.ComponentModel.DesignerCategory("")]
internal class CatalogueFolderMenu : RDMPContextMenuStrip
{
    public CatalogueFolderMenu(RDMPContextMenuStripArgs args, string folder) : base(args, folder)
    {
        //Things that are always visible regardless
        Add(new ExecuteCommandCreateNewCatalogueByImportingFileUI(_activator)
        {
            OverrideCommandName = "New Catalogue From File...",
            TargetFolder = folder,
            SuggestedCategory = AtomicCommandFactory.Add,
            Weight = -90.9f
        });

        args.SkipCommand<ExecuteCommandCreateNewCatalogueByImportingFile>();

        Add(new ExecuteCommandGenerateMetadataReport(_activator,
            _activator.CoreChildProvider.GetAllChildrenRecursively(folder).OfType<ICatalogue>().ToArray()));
    }
}