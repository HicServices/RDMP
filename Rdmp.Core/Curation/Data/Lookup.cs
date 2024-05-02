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
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes a relationship between 3 ColumnInfos in which 2 are from a lookup table (e.g. z_drugName), these are a
///     primary
///     key (e.g. DrugCode) and a description (e.g. HumanReadableDrugName).  And a third ColumnInfo from a different table
///     (e.g.
///     Prescribing) which is a foreign key (e.g. DrugPrescribed).
///     <para>
///         The QueryBuilder uses this information to work out how to join together various tables in a query.  Note that
///         it is possible
///         to define the same lookup multiple times just with different foreign keys (e.g. Prescribing and DrugAbuse
///         datasets might both
///         share the same lookup table z_drugName).
///     </para>
///     <para>
///         It is not possible to create these lookup dependencies automatically because often an agency won't actually
///         have relationships
///         (referential integrity) between their lookup tables and main datasets due to dirty data / missing lookup
///         values.  These are all
///         concepts which the RDMP is familiar with and built to handle.
///     </para>
///     <para>
///         Note also that you can have one or more LookupCompositeJoinInfo for when you need to join particularly ugly
///         lookups (e.g. if you
///         have the same DrugCode meaning different things based on the prescribing board - you need to join on both
///         drugName and
///         prescriberHealthboard).
///     </para>
/// </summary>
public class Lookup : DatabaseEntity, IJoin, IHasDependencies, ICheckable
{
    //cached answers
    private ColumnInfo _description;
    private ColumnInfo _foreignKey;
    private ColumnInfo _primaryKey;


    #region Database Properties

    private int _description_ID;
    private int _foreignKey_ID;
    private int _primaryKey_ID;
    private string _collation;
    private ExtractionJoinType _extractionJoinType;

    /// <summary>
    ///     The column in the lookup table containing the lookup description for the code e.g.
    ///     [z_GenderLookup].[GenderCodeDescription]
    /// </summary>
    public int Description_ID
    {
        get => _description_ID;
        set => SetField(ref _description_ID, value);
    }

    /// <summary>
    ///     The column in the lookup table containing the lookup code e.g. [z_GenderLookup].[Gender]
    /// </summary>
    public int ForeignKey_ID
    {
        get => _foreignKey_ID;
        set => SetField(ref _foreignKey_ID, value);
    }

    /// <summary>
    ///     The column in the main dataset containing the lookup code e.g. [Prescribing].[Gender]
    /// </summary>
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

    /// <inheritdoc />
    /// <remarks>For <see cref="Lookup" /> this should almost always be LEFT</remarks>
    public ExtractionJoinType ExtractionJoinType
    {
        get => _extractionJoinType;
        set => SetField(ref _extractionJoinType, value);
    }

    #endregion

    #region Relationships

    /// <summary>
    ///     These are dereferenced cached versions of the entities to which the _ID properties refer to, to change them change
    ///     the _ID version
    /// </summary>
    [NoMappingToDatabase]
    public ColumnInfo Description => _description ??= Repository.GetObjectByID<ColumnInfo>(Description_ID);

    /// <summary>
    ///     These are dereferenced cached versions of the entities to which the _ID properties refer to, to change them change
    ///     the _ID version
    /// </summary>
    [NoMappingToDatabase]
    public ColumnInfo ForeignKey => _foreignKey ??= Repository.GetObjectByID<ColumnInfo>(ForeignKey_ID);

    /// <summary>
    ///     These are dereferenced cached versions of the entities to which the _ID properties refer to, to change them change
    ///     the _ID version
    /// </summary>
    [NoMappingToDatabase]
    public ColumnInfo PrimaryKey => _primaryKey ??= Repository.GetObjectByID<ColumnInfo>(PrimaryKey_ID);

    #endregion

    public Lookup()
    {
    }

    /// <summary>
    ///     Declares that the columns provide form a foreign key join to lookup table relationship
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="description">The lookup table description column</param>
    /// <param name="foreignKey">The main dataset column that joins to the lookup e.g. Prescribing.DrugCode</param>
    /// <param name="primaryKey">The lookup table column that contains the code e.g. z_DrugLookup.Code</param>
    /// <param name="type"></param>
    /// <param name="collation"></param>
    public Lookup(ICatalogueRepository repository, ColumnInfo description, ColumnInfo foreignKey, ColumnInfo primaryKey,
        ExtractionJoinType type, string collation)
    {
        //do checks before it hits the database.
        if (foreignKey.ID == primaryKey.ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

        if (foreignKey.TableInfo_ID == primaryKey.TableInfo_ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 are from the same table, this is not cool");

        if (description.TableInfo_ID != primaryKey.TableInfo_ID)
            throw new ArgumentException(
                "Join Key 2 must be in the same table as the Description ColumnInfo (i.e. Primary Key)");

        if (description.ID == primaryKey.ID)
            throw new ArgumentException("Description Column and PrimaryKey Column cannot be the same column!");

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Description_ID", description.ID },
            { "ForeignKey_ID", foreignKey.ID },
            { "PrimaryKey_ID", primaryKey.ID },
            { "ExtractionJoinType", type.ToString() },
            { "Collation", string.IsNullOrWhiteSpace(collation) ? DBNull.Value : collation }
        });
    }

    internal Lookup(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Description_ID = int.Parse(r["Description_ID"].ToString());
        ForeignKey_ID = int.Parse(r["ForeignKey_ID"].ToString());
        PrimaryKey_ID = int.Parse(r["PrimaryKey_ID"].ToString());
        Collation = r["Collation"] as string;

        if (Enum.TryParse(r["ExtractionJoinType"].ToString(), true, out ExtractionJoinType joinType))
            ExtractionJoinType = joinType;
        else
            throw new Exception($"Did not recognise ExtractionJoinType:{r["ExtractionJoinType"]}");

        if (ForeignKey_ID == PrimaryKey_ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return ToStringCached();
    }

    private string _cachedToString;

    private string ToStringCached()
    {
        return _cachedToString ??= $" {ForeignKey.Name} = {PrimaryKey.Name}";
    }

    /// <summary>
    ///     Returns all <see cref="Lookup" /> relationships that exist between the main dataset
    ///     <paramref name="foreignKeyTable" /> and the
    ///     assumed lookup table <paramref name="primaryKeyTable" />
    /// </summary>
    /// <param name="foreignKeyTable">The main dataset table</param>
    /// <param name="primaryKeyTable">The hypothesized lookup table</param>
    /// <returns>
    ///     All lookup relationships, a given table could have 2+ of these e.g. SendingLocation and DischargeLocation
    ///     could both reference z_Location lookup
    /// </returns>
    public static Lookup[] GetAllLookupsBetweenTables(TableInfo foreignKeyTable, TableInfo primaryKeyTable)
    {
        var toReturn = new List<Lookup>();

        if (foreignKeyTable.Equals(primaryKeyTable))
            throw new NotSupportedException("Tables must be different");

        if (!foreignKeyTable.Repository.Equals(primaryKeyTable.Repository))
            throw new NotSupportedException("TableInfos come from different repositories!");

        var repo = (CatalogueRepository)foreignKeyTable.Repository;
        using var con = repo.GetConnection();
        using (var cmd = DatabaseCommandHelper.GetCommand(@"SELECT * FROM [Lookup] 
  WHERE 
  (SELECT TableInfo_ID FROM ColumnInfo where ID = PrimaryKey_ID) = @primaryKeyTableID
  AND
  (SELECT TableInfo_ID FROM ColumnInfo where ID = [ForeignKey_ID]) = @foreignKeyTableID"
                   , con.Connection, con.Transaction))
        {
            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@primaryKeyTableID", cmd));
            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@foreignKeyTableID", cmd));

            cmd.Parameters["@primaryKeyTableID"].Value = primaryKeyTable.ID;
            cmd.Parameters["@foreignKeyTableID"].Value = foreignKeyTable.ID;

            using var r = cmd.ExecuteReader();
            while (r.Read())
                toReturn.Add(new Lookup(repo, r));
        }

        return toReturn.ToArray();
    }

    /// <summary>
    ///     Checks that the Lookup configuration is legal (e.g. not a table linking against itself etc).
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        if (ForeignKey.TableInfo_ID == PrimaryKey.TableInfo_ID)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Foreign Key and Primary Key are from the same table for Lookup {ID}", CheckResult.Fail));

        if (Description.TableInfo_ID != PrimaryKey.TableInfo_ID)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Description Key and Primary Key are from different tables (Not allowed) in Lookup {ID}",
                CheckResult.Fail));
    }

    /// <inheritdoc />
    public override void SaveToDatabase()
    {
        //do checks before it hits the database.
        if (ForeignKey.ID == PrimaryKey.ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

        if (ForeignKey.TableInfo_ID == PrimaryKey.TableInfo_ID)
            throw new ArgumentException("Join Key 1 and Join Key 2 are from the same table, this is not cool");

        if (Description.TableInfo_ID != PrimaryKey.TableInfo_ID)
            throw new ArgumentException(
                "Join Key 2 must be in the same table as the Description ColumnInfo (i.e. Primary Key)");

        base.SaveToDatabase();
    }

    /// <inheritdoc />
    public IEnumerable<ISupplementalJoin> GetSupplementalJoins()
    {
        return Repository.GetAllObjectsWhere<LookupCompositeJoinInfo>("OriginalLookup_ID", ID);
    }

    /// <inheritdoc />
    public ExtractionJoinType GetInvertedJoinType()
    {
        throw new NotSupportedException(
            "Lookup joins should never be inverted... can't see why you would want to do that... they are always LEFT joined ");
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return new[] { Description, ForeignKey, PrimaryKey };
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        return null;
    }

    /// <summary>
    ///     Tells the the <see cref="Lookup" /> what the objects are referenced by <see cref="PrimaryKey_ID" />,
    ///     <see cref="ForeignKey_ID" /> and
    ///     <see cref="Description_ID" /> so that it doesn't have to fetch them from the database.
    /// </summary>
    /// <param name="primaryKey"></param>
    /// <param name="foreignKey"></param>
    /// <param name="descriptionColumn"></param>
    public void SetKnownColumns(ColumnInfo primaryKey, ColumnInfo foreignKey, ColumnInfo descriptionColumn)
    {
        if (PrimaryKey_ID != primaryKey.ID || ForeignKey_ID != foreignKey.ID || Description_ID != descriptionColumn.ID)
            throw new Exception("Injected arguments did not match on ID");

        _primaryKey = primaryKey;
        _foreignKey = foreignKey;
        _description = descriptionColumn;
    }

    /// <inheritdoc />
    public string GetCustomJoinSql()
    {
        return CatalogueRepository.GetExtendedProperties(ExtendedProperty.CustomJoinSql, this)
            .FirstOrDefault()?.Value;
    }
}