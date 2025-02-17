// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Lookup relationships in RDMP are defined using 3 columns, a PrimaryKey from one table and a ForeignKey which appears in the lookup and the Description column
/// which must also appear in the ForeignKey table.  This Enum is used to identify which ColumnInfo you are addressing in this relationship.
/// </summary>
public enum LookupType
{
    /// <summary>
    /// The column in the Lookup table which contains the description of what a code means
    /// </summary>
    Description,

    /// <summary>
    /// Used for Fetching only, this value reflects either the PrimaryKey or the ForeignKey (but not the Description).  Used for example to find out
    /// all the Lookup involvements of a given ColumnInfo.
    /// </summary>
    AnyKey,

    /// <summary>
    /// The column in the lookup table which contains the code
    /// </summary>
    ForeignKey
}

/// <summary>
/// The type of ANSI Sql Join to direction e.g. Left/Right
/// </summary>
public enum ExtractionJoinType
{
    /// <summary>
    /// All records from the table on the left and any matching ones from the table on the right (otherwise null for those fields)
    /// </summary>
    Left,

    /// <summary>
    /// All records from the table on the right and any matching ones from the table on the left (otherwise null for those fields)
    /// </summary>
    Right,

    /// <summary>
    /// Only records where the primary/foreign keys match exactly between both tables (the right and the left)
    /// </summary>
    Inner
}

/// <summary>
/// Persistent reference in the Catalogue database that records how to join two TableInfos.  You can create instances of this class via JoinHelper (which is available as
/// a property on ICatalogueRepository).  JoinInfos are processed by during query building in the following way:
/// 
/// <para>1. Query builder identifies all the TablesUsedInQuery (from the columns selected, forced table inclusions etc)
/// 2. Query builder identifies all available JoinInfos between the TablesUsedInQuery (See SqlQueryBuilderHelper.FindRequiredJoins)
/// 3. Query builder merges JoinInfos that reference the same tables together into Combo Joins (See AddQueryBuildingTimeComboJoinDiscovery)
/// 4. Query builder creates final Join Sql </para>
/// 
/// <para>'Combo Joins' (or ISupplementalJoin) are when you need to use multiple columns to do the join e.g. A Left Join B on A.x = B.x AND A.y = B.y.  You can define
/// these by simply declaring additional JoinInfos for the other column pairings with the same ExtractionJoinType.</para>
/// </summary>
public class JoinInfo : DatabaseEntity, IJoin, IHasDependencies
{
    #region Database Properties

    private int _foreignKeyID;
    private int _primaryKeyID;
    private string _collation;
    private ExtractionJoinType _extractionJoinType;

    /// <inheritdoc cref="IJoin.ForeignKey"/>
    public int ForeignKey_ID
    {
        get => _foreignKeyID;
        set => SetField(ref _foreignKeyID, value);
    }

    /// <inheritdoc cref="IJoin.PrimaryKey"/>
    public int PrimaryKey_ID
    {
        get => _primaryKeyID;
        set => SetField(ref _primaryKeyID, value);
    }

    /// <inheritdoc/>
    public string Collation
    {
        get => _collation;
        set => SetField(ref _collation, value);
    }

    /// <inheritdoc/>
    public ExtractionJoinType ExtractionJoinType
    {
        get => _extractionJoinType;
        set => SetField(ref _extractionJoinType, value);
    }

    #endregion

    //cached answer
    private ColumnInfo _foreignKey;
    private ColumnInfo _primaryKey;


    private List<JoinInfo> _queryTimeComboJoins = new();

    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ColumnInfo ForeignKey => _foreignKey ??= Repository.GetObjectByID<ColumnInfo>(ForeignKey_ID);

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ColumnInfo PrimaryKey => _primaryKey ??= Repository.GetObjectByID<ColumnInfo>(PrimaryKey_ID);

    #endregion

    public JoinInfo()
    {
    }

    /// <summary>
    /// Constructor to be used to create already existing JoinInfos out of the database only.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal JoinInfo(IRepository repository, DbDataReader r) : base(repository, r)
    {
        ForeignKey_ID = Convert.ToInt32(r["ForeignKey_ID"]);
        PrimaryKey_ID = Convert.ToInt32(r["PrimaryKey_ID"]);

        Collation = r["Collation"] as string;

        if (Enum.TryParse(r["ExtractionJoinType"].ToString(), true, out ExtractionJoinType joinType))
            ExtractionJoinType = joinType;
        else
            throw new Exception($"Did not recognise ExtractionJoinType:{r["ExtractionJoinType"]}");

        if (ForeignKey_ID == PrimaryKey_ID)
            throw new Exception("Join key 1 and 2 are the same, lookup is broken");
    }


    public JoinInfo(ICatalogueRepository repository, ColumnInfo foreignKey, ColumnInfo primaryKey,
        ExtractionJoinType type, string collation)
    {
        if (foreignKey.ID == primaryKey.ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

        if (foreignKey.TableInfo_ID == primaryKey.TableInfo_ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 cannot be from the same table");

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ForeignKey_ID", foreignKey.ID },
            { "PrimaryKey_ID", primaryKey.ID },
            { "ExtractionJoinType", type.ToString() },
            { "Collation", collation }
        });
    }

    /// <inheritdoc/>
    public override string ToString() => $" {ForeignKey.Name} = {PrimaryKey.Name}";

    /// <summary>
    /// Notifies the join that other columns also need to be joined at runtime (e.g. when you have 2+ column pairs all of
    /// which have to appear on the SQL ON section of the query
    /// </summary>
    /// <param name="availableJoin"></param>
    public void AddQueryBuildingTimeComboJoinDiscovery(JoinInfo availableJoin)
    {
        if (availableJoin.Equals(this))
            throw new Exception("A JoinInfo cannot add QueryTimeComboJoin to itself");

        if (!_queryTimeComboJoins.Contains(availableJoin))
            _queryTimeComboJoins.Add(availableJoin);
    }

    /// <inheritdoc/>
    public IEnumerable<ISupplementalJoin> GetSupplementalJoins()
    {
        //Supplemental Joins are not currently supported by JoinInfo, only Lookups
        return _queryTimeComboJoins.Select(j => new QueryTimeComboJoin
        {
            Collation = j.Collation,
            PrimaryKey = j.PrimaryKey,
            ForeignKey = j.ForeignKey
        });
    }

    /// <inheritdoc/>
    public ExtractionJoinType GetInvertedJoinType()
    {
        return ExtractionJoinType switch
        {
            ExtractionJoinType.Left => ExtractionJoinType.Right,
            ExtractionJoinType.Right => ExtractionJoinType.Left,
            _ => ExtractionJoinType
        };
    }

    private class QueryTimeComboJoin : ISupplementalJoin
    {
        /// <inheritdoc cref="IJoin.ForeignKey"/>
        public ColumnInfo ForeignKey { get; set; }

        /// <inheritdoc cref="IJoin.PrimaryKey"/>
        public ColumnInfo PrimaryKey { get; set; }

        /// <inheritdoc cref="IJoin.Collation"/>
        public string Collation { get; set; }
    }


    /// <summary>
    /// Tells the the <see cref="JoinInfo"/> what the objects are referenced by <see cref="PrimaryKey_ID"/> and <see cref="ForeignKey_ID"/>
    /// so that it doesn't have to fetch them from the database.
    /// </summary>
    /// <param name="primaryKey"></param>
    /// <param name="foreignKey"></param>
    public void SetKnownColumns(ColumnInfo primaryKey, ColumnInfo foreignKey)
    {
        if (PrimaryKey_ID != primaryKey.ID || ForeignKey_ID != foreignKey.ID)
            throw new Exception("Injected arguments did not match on ID");

        _primaryKey = primaryKey;
        _foreignKey = foreignKey;
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return new[] { PrimaryKey, ForeignKey };
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => Array.Empty<IHasDependencies>();

    /// <inheritdoc/>
    public string GetCustomJoinSql() => CatalogueRepository.GetExtendedProperties(ExtendedProperty.CustomJoinSql, this)
        .FirstOrDefault()?.Value;
}