// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using System.Linq;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandLinkCatalogueToDataset : BasicCommandExecution
{
    private readonly Catalogue _catalogue;
    private readonly Curation.Data.Datasets.Dataset _dataset;
    private readonly bool _linkAll;
    public ExecuteCommandLinkCatalogueToDataset(IBasicActivateItems activator, [DemandsInitialization("The catalogue To link")]Catalogue catalogue, [DemandsInitialization("The dataset to link to")]Curation.Data.Datasets.Dataset dataset, bool linkAllOtherColumns = true) : base(activator)
    {
        _catalogue = catalogue;
        _dataset = dataset;
        _linkAll = linkAllOtherColumns;

        if (_catalogue is null) SetImpossible("No Catalogue Selected");
        if (_dataset is null) SetImpossible("No Dataset Selected");
    }


    public override void Execute()
    {
        base.Execute();
        var linkage = new CatalogueDatasetLinkage(_catalogue.CatalogueRepository, _catalogue, _dataset);
        linkage.SaveToDatabase();
        Publish(linkage);
    }
}