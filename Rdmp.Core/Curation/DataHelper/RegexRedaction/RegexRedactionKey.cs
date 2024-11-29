// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction;

/// <summary>
/// Stores the Pks of an data entry that has a redaction
/// </summary>
public class RegexRedactionKey : DatabaseEntity, IRegexRedactionKey
{
    private int _redaction;
    private int _columnInfo;
    private string _value;

    public int RegexRedaction_ID { get => _redaction; set => SetField(ref _redaction, value); }
    public int ColumnInfo_ID { get => _columnInfo; set => SetField(ref _columnInfo, value); }

    public string Value { get => _value; set => SetField(ref _value, value); }

    public RegexRedactionKey() { }

    public RegexRedactionKey(ICatalogueRepository repository, RegexRedaction redaction, ColumnInfo pkColumn, string value)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"RegexRedaction_ID", redaction.ID },
            {"ColumnInfo_ID", pkColumn.ID },
            {"Value", value },
        });
    }

    internal RegexRedactionKey(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        _redaction = Int32.Parse(r["RegexRedaction_ID"].ToString());
        _columnInfo = Int32.Parse(r["ColumnInfo_ID"].ToString());
        _value = r["Value"].ToString();
    }

}
