﻿// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ExtractionUIs.JoinsAndLookups;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandLinkCatalogueToDatasetUI : BasicUICommandExecution, IAtomicCommand
{
    private readonly Catalogue _catalogue;
    private Dataset _selectedDataset;
    private IActivateItems _activateItems;

    public ExecuteCommandLinkCatalogueToDatasetUI(IActivateItems activator, Catalogue catalogue) : base(activator)
    {
        _catalogue = catalogue;
        _activateItems = activator;
    }

    public override string GetCommandHelp() =>
        "Link all columns of this catalogue to a dataset";

    public override void Execute()
    {
        base.Execute();
        Dataset[] datasets = _activateItems.RepositoryLocator.CatalogueRepository.GetAllObjects<Dataset>();
        DialogArgs da = new()
        {
            WindowTitle = "Link a dataset with this catalogue",
            TaskDescription =
             "Select the Dataset that this catalogue information came from"
        };
        _selectedDataset = SelectOne(da, datasets);
        if (_selectedDataset is not null)
        {
            var backfill = YesNo("Link all other columns that match the source table?", "Do you want to link this dataset to all other columns that reference the same table as this column?");
            var cmd = new ExecuteCommandLinkCatalogueToDataset(_activateItems, _catalogue, _selectedDataset, backfill);
            cmd.Execute();
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Dataset, OverlayKind.Link);
}