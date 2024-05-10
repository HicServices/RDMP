// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.Setting;

/// <inheritdoc cref="ISetting"/>

public class Setting : DatabaseEntity, ISetting
{
    #region Database Properties

    private string _key;
    private string _value;

    [NotNull]
    [Unique]
    public string Key
    {
        get => _key;
        set => SetField(ref _key, value);
    }

    [NotNull]
    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    #endregion

    public Setting(ICatalogueRepository repository, string key, string value)
    {
        Key = key;
        Value = value;
        Repository = repository;
        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { nameof(Key), key },
            { nameof(Value), value },
        });
    }

    public Setting(ICatalogueRepository repository, DbDataReader r): base(repository,r)
    {
        Key = r["Key"].ToString();
        Value = r["Value"].ToString();
    }
}