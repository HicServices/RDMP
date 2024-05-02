// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes to QueryBuilder a secondary/tertiary etc join requirement when making a Lookup join (see
///     <see cref="Lookup" />)
///     <para>
///         This is only the case if you have a given lookup code which changes meaning based on another column e.g.
///         testcode X means a different thing
///         in healthboard A vs healthboard B
///     </para>
/// </summary>
public class LookupCompositeJoinInfo : DatabaseEntity, ISupplementalJoin
{
    #region Database Properties

    private int _originalLookup_ID;
    private int _foreignKey_ID;
    private int _primaryKey_ID;
    private string _collation;

    /// <summary>
    ///     The Main <see cref="Lookup" /> to which this column pair must also be joined in the ON SQL block
    /// </summary>
    public int OriginalLookup_ID
    {
        get => _originalLookup_ID;
        set => SetField(ref _originalLookup_ID, value);
    }

    /// <inheritdoc cref="IJoin.ForeignKey" />
    public int ForeignKey_ID
    {
        get => _foreignKey_ID;
        set => SetField(ref _foreignKey_ID, value);
    }

    /// <inheritdoc cref="IJoin.PrimaryKey" />
    public int PrimaryKey_ID
    {
        get => _primaryKey_ID;
        set => SetField(ref _primaryKey_ID, value);
    }

    /// <inheritdoc />
    public string Collation
    {
        get => _collation;
        set => SetField(ref _collation, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="IJoin.ForeignKey" />
    [NoMappingToDatabase]
    public ColumnInfo ForeignKey => Repository.GetObjectByID<ColumnInfo>(ForeignKey_ID);

    /// <inheritdoc cref="IJoin.PrimaryKey" />
    [NoMappingToDatabase]
    public ColumnInfo PrimaryKey => Repository.GetObjectByID<ColumnInfo>(PrimaryKey_ID);

    #endregion

    public LookupCompositeJoinInfo()
    {
    }

    /// <inheritdoc cref="LookupCompositeJoinInfo" />
    public LookupCompositeJoinInfo(ICatalogueRepository repository, Lookup parent, ColumnInfo foreignKey,
        ColumnInfo primaryKey, string collation = null)
    {
        if (foreignKey.ID == primaryKey.ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

        if (foreignKey.TableInfo_ID == primaryKey.TableInfo_ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 are from the same table, this is not cool");

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "OriginalLookup_ID", parent.ID },
            { "ForeignKey_ID", foreignKey.ID },
            { "PrimaryKey_ID", primaryKey.ID },
            { "Collation", string.IsNullOrWhiteSpace(collation) ? DBNull.Value : collation }
        });
    }

    internal LookupCompositeJoinInfo(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Collation = r["Collation"] as string;
        OriginalLookup_ID = int.Parse(r["OriginalLookup_ID"].ToString());

        ForeignKey_ID = int.Parse(r["ForeignKey_ID"].ToString());
        PrimaryKey_ID = int.Parse(r["PrimaryKey_ID"].ToString());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ToStringCached();
    }

    private string _cachedToString;

    private string ToStringCached()
    {
        return _cachedToString ??= $"{ForeignKey.Name} = {PrimaryKey.Name}";
    }

    /// <inheritdoc />
    public override void SaveToDatabase()
    {
        if (ForeignKey.ID == PrimaryKey.ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

        if (ForeignKey.TableInfo_ID == PrimaryKey.TableInfo_ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 are from the same table, this is not cool");

        base.SaveToDatabase();
    }
}