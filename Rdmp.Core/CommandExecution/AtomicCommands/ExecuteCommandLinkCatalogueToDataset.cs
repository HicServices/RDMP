// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandLinkCatalogueToDataset : BasicCommandExecution, IAtomicCommand
{
    private Catalogue _catalogue;
    private Curation.Data.Dataset _dataset;
    private bool _linkAll;
    public ExecuteCommandLinkCatalogueToDataset(IBasicActivateItems activator, [DemandsInitialization("The catalogue To link")]Catalogue catalogue, [DemandsInitialization("The dataset to link to")]Curation.Data.Dataset dataset, bool linkAllOtherColumns = true) : base(activator)
    {
        _catalogue = catalogue;
        _dataset = dataset;
        _linkAll = linkAllOtherColumns;
    }


    public override void Execute()
    {
        base.Execute();
        if (_catalogue is null) throw new Exception("No Catalogue Selected");
        if (_dataset is null) throw new Exception("No Dataset Selected");
        var items = _catalogue.CatalogueItems.ToList();
        foreach (var item in items)
        {
            var ci = item.ColumnInfo;
            if (ci is null) continue;
            if (ci.Dataset_ID == _dataset.ID)
            {
                continue;
            }

            ci.Dataset_ID = _dataset.ID;
            ci.SaveToDatabase();
            if (_linkAll)
            {
                var databaseName = ci.Name[..ci.Name.LastIndexOf('.')];
                var catalogueItems = ci.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci => ci.Name[..ci.Name.LastIndexOf(".")] == databaseName);
                foreach (var aci in catalogueItems)
                {
                    aci.Dataset_ID = _dataset.ID;
                    aci.SaveToDatabase();
                }
            }

        }

    }
}