// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.Diagnostics.CodeAnalysis;
using Rdmp.Core.Curation.Data;



namespace Rdmp.Core.Curation.Data;

/// <inheritdoc cref="IRedactedCHI"/>

public class RedactedCHI : DatabaseEntity, IRedactedCHI
{
    private string _potentialCHI;
    private int _replacementIndex;
    private string _table;
    private string _pkValue;
    private string _columnName;

    [NotNull]
    public string PotentialCHI
    {
        get => _potentialCHI;
        set => SetField(ref _potentialCHI, value);
    }

    [NotNull]
    public int ReplacementIndex
    {
        get => _replacementIndex;
        set => SetField(ref _replacementIndex, value);
    }

    [NotNull]
    public string TableName
    {
        get => _table;
        set => SetField(ref _table, value);
    }

    [NotNull]
    public string PKValue
    {
        get => _pkValue;
        set => SetField(ref _pkValue, value);
    }

    [NotNull]
    public string ColumnName
    {
        get => _columnName;
        set => SetField(ref _columnName, value);
    }

    public RedactedCHI(ICatalogueRepository catalogueRepository, string potentialCHI, int replacementIndex, string table, string pkValue,string columnName)
    {
        catalogueRepository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            {"potentialCHI", potentialCHI },{"replacementIndex",replacementIndex},{"tableName",table},{"PKValue",pkValue},{"cColumnName",columnName}
        });
    }

    public RedactedCHI() { }
    public RedactedCHI(ICatalogueRepository repository, DbDataReader r)
       : base(repository, r)
    {
        PotentialCHI = r["PotentialChi"].ToString();
        ReplacementIndex = int.Parse(r["ReplacementIndex"].ToString());
        TableName = r["TableName"].ToString();
        PKValue = r["PKValue"].ToString();
        ColumnName = r["ColumnName"].ToString();

    }
}