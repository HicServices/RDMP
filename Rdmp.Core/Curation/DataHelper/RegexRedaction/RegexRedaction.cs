// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction;


/// <summary>
/// Stores a redaction
/// </summary>
public class RegexRedaction : DatabaseEntity, IRegexRedaction

{
    private int _redactionConfigurationID;
    private int _startingIndex;
    private string _redactedValue;
    private string _replacementValue;
    private int _columnInfoID;

    #region Database Properties

    public int RedactionConfiguration_ID
    {
        get => _redactionConfigurationID;
        set => SetField(ref _redactionConfigurationID, value);
    }

    [UsefulProperty]
    [NotNull]
    public string RedactedValue
    {
        get => _redactedValue;
        set => SetField(ref _redactedValue, value.ToString());
    }

    [UsefulProperty]
    [NotNull]
    public string ReplacementValue
    {
        get => _replacementValue;
        set => SetField(ref _replacementValue, value.ToString());
    }

    [NotNull]
    public int StartingIndex
    {
        get => _startingIndex;
        set => SetField(ref _startingIndex, value);
    }

    public int ColumnInfo_ID
    {
        get => _columnInfoID;
        set => SetField(ref _columnInfoID, value);
    }
    #endregion

    #region Relationships
    [NoMappingToDatabase]
    public List<RegexRedactionKey> RedactionKeys => [.. CatalogueRepository.GetAllObjectsWhere<RegexRedactionKey>("RegexRedaction_ID", this.ID)];
    #endregion


    public RegexRedaction() { }

    public RegexRedaction(ICatalogueRepository repository, int redactionConfigurationID, int startingIndex, string redactionValue, string replacementValue, int columnInfoID, Dictionary<ColumnInfo,string> pkValues)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object> {
            {"RedactionConfiguration_ID",redactionConfigurationID},
            {"StartingIndex",startingIndex},
            {"RedactedValue", redactionValue },
            {"ReplacementValue", replacementValue },
            {"ColumnInfo_ID", columnInfoID }
        });
        foreach (var tuple in pkValues)
        {
            var key = new RegexRedactionKey(repository, this, tuple.Key, tuple.Value);
            key.SaveToDatabase();
        }
    }

    internal RegexRedaction(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        _redactionConfigurationID = Int32.Parse(r["RedactionConfiguration_ID"].ToString());
        _startingIndex = Int32.Parse(r["StartingIndex"].ToString());
        _replacementValue = r["ReplacementValue"].ToString();
        _redactedValue = r["RedactedValue"].ToString();
        _columnInfoID = Int32.Parse(r["ColumnInfo_ID"].ToString());
    }
}
