// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandLinkColumnInfoToDataset : BasicCommandExecution
{
    private readonly ColumnInfo _columnInfo;
    private readonly Curation.Data.Dataset _dataset;
    private readonly bool _linkAll;

    public ExecuteCommandLinkColumnInfoToDataset(IBasicActivateItems activator,
        [DemandsInitialization("The column to link")] ColumnInfo columnInfo,
        [DemandsInitialization("The dataset to link to")] Curation.Data.Dataset dataset,
        bool linkAllOtherColumns = true) : base(activator)
    {
        _columnInfo = columnInfo;
        _dataset = dataset;
        _linkAll = linkAllOtherColumns;
    }


    public override void Execute()
    {
        base.Execute();
        _columnInfo.Dataset_ID = _dataset.ID;
        _columnInfo.SaveToDatabase();
        if (!_linkAll) return;

        var databaseName = _columnInfo.Name[.._columnInfo.Name.LastIndexOf('.')];
        var catalogueItems = _columnInfo.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci =>
            ci.Name[..ci.Name.LastIndexOf(".", StringComparison.Ordinal)] == databaseName);
        foreach (var ci in catalogueItems)
        {
            ci.Dataset_ID = _dataset.ID;
            ci.SaveToDatabase();
        }
    }
}