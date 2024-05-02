// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.ANOEngineering;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataLoad.Modules.Mutilators.Dilution;

/// <summary>
///     Creates IDilutionOperations by reflection based on Type name and hydrates with the target IPreLoadDiscardedColumn.
///     See Dilution.
/// </summary>
public class DilutionOperationFactory
{
    private readonly IPreLoadDiscardedColumn _targetColumn;

    public DilutionOperationFactory(IPreLoadDiscardedColumn targetColumn)
    {
        _targetColumn = targetColumn ?? throw new ArgumentNullException(nameof(targetColumn));
    }

    public IDilutionOperation Create(Type operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (!typeof(IDilutionOperation).IsAssignableFrom(operation))
            throw new ArgumentException($"Requested operation Type {operation} did was not an IDilutionOperation");

        var instance = MEF.CreateA<IDilutionOperation>(operation.FullName);
        instance.ColumnToDilute = _targetColumn;

        return instance;
    }
}