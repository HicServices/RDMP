// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FAnsi.Implementations.MicrosoftSQL;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
/// Describes where a PreLoadDiscardedColumn will ultimately end up.
/// </summary>
public enum DiscardedColumnDestination
{
    /// <summary>
    /// Column appears in RAW and might be used in AdjustRaw but is dropped completely prior to migration to Staging
    /// </summary>
    Oblivion = 1,

    /// <summary>
    /// Column appears in RAW but is separated off and stored in an IdentifierDump (See IdentifierDumper) and not passed through to Staging
    /// </summary>
    StoreInIdentifiersDump = 2,

    /// <summary>
    /// Column appears in RAW but is Diluted during AdjustStaging prior to joining the live dataset e.g. by rounding dates to the nearest quarter.  The undilted value may be stored in the IdentifierDump (See IdentifierDumper).
    /// </summary>
    Dilute = 3
}

/// <summary>
/// Describes a column that is provided to your institution by a data provider but which is not loaded into your LIVE database table.  This column might be very sensitive,
/// irrelevant to you etc.  Each discarded column has a destination (DiscardedColumnDestination)  e.g. it might be dropped completely or routed into an identifier dump for
/// when you still want to store information such as Who an MRI was for but do not want it sitting in your live dataset for governance/anonymisation reasons.
/// 
/// <para>Each instance is tied to a specific TableInfo and when a data load occurs from an unstructured format (e.g. CSV) which RequestsExternalDatabaseCreation then not only are the
/// LIVE columns created in the RAW bubble but also the dropped columns described in PreLoadDiscardedColumn instances.  This allows the live system state to drive required formats/fields
/// for data load resulting in a stricter/more maintainable data load model.</para>
/// </summary>
public class PreLoadDiscardedColumn : DatabaseEntity, IPreLoadDiscardedColumn, ICheckable, IInjectKnown<ITableInfo>
{
    #region Database Properties

    private int _tableInfoID;
    private DiscardedColumnDestination _destination;
    private string _runtimeColumnName;
    private string _sqlDataType;
    private int? _duplicateRecordResolutionOrder;
    private bool _duplicateRecordResolutionIsAscending;
    private Lazy<ITableInfo> _knownTableInfo;

    /// <inheritdoc cref="IPreLoadDiscardedColumn.TableInfo"/>
    public int TableInfo_ID
    {
        get => _tableInfoID;
        set => SetField(ref _tableInfoID, value);
    }

    /// <inheritdoc/>
    public DiscardedColumnDestination Destination
    {
        get => _destination;
        set => SetField(ref _destination, value);
    }

    /// <inheritdoc/>
    public string RuntimeColumnName
    {
        get => _runtimeColumnName;
        set => SetField(ref _runtimeColumnName, value);
    }

    /// <inheritdoc/>
    public string SqlDataType
    {
        get => _sqlDataType;
        set => SetField(ref _sqlDataType, value);
    }

    /// <inheritdoc/>
    public int? DuplicateRecordResolutionOrder
    {
        get => _duplicateRecordResolutionOrder;
        set => SetField(ref _duplicateRecordResolutionOrder, value);
    }

    /// <inheritdoc/>
    public bool DuplicateRecordResolutionIsAscending
    {
        get => _duplicateRecordResolutionIsAscending;
        set => SetField(ref _duplicateRecordResolutionIsAscending, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public ITableInfo TableInfo => _knownTableInfo.Value;

    #endregion

    //required for IResolveDuplication
    /// <summary>
    /// For setting, use SqlDataType instead, it is an exact alias to allow for IResolveDuplication interface definition (see the fact that ColumnInfo also uses that interface and is also IMapsDirectlyToDatabaseTable)
    /// </summary>
    [NoMappingToDatabase]
    public string Data_type => SqlDataType;

    public PreLoadDiscardedColumn()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Creates a new virtual column that will be created in RAW during data loads but does not appear in the LIVE table schema.  This allows
    /// identifiable data to be loaded and processed in a data load without ever hitting the live database.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    /// <param name="name"></param>
    public PreLoadDiscardedColumn(ICatalogueRepository repository, ITableInfo parent, string name = null)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "TableInfo_ID", parent.ID },
            { "Destination", DiscardedColumnDestination.Oblivion },
            { "RuntimeColumnName", name ?? $"NewColumn{Guid.NewGuid()}" }
        });

        ClearAllInjections();
    }

    internal PreLoadDiscardedColumn(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        TableInfo_ID = int.Parse(r["TableInfo_ID"].ToString());
        Destination = (DiscardedColumnDestination)r["Destination"];
        RuntimeColumnName = r["RuntimeColumnName"] as string;
        SqlDataType = r["SqlDataType"] as string;

        if (r["DuplicateRecordResolutionOrder"] != DBNull.Value)
            DuplicateRecordResolutionOrder = int.Parse(r["DuplicateRecordResolutionOrder"].ToString());
        else
            DuplicateRecordResolutionOrder = null;

        DuplicateRecordResolutionIsAscending = Convert.ToBoolean(r["DuplicateRecordResolutionIsAscending"]);

        ClearAllInjections();
    }

    /// <inheritdoc/>
    public override string ToString() => $"{RuntimeColumnName} ({Destination})";


    public void Check(ICheckNotifier notifier)
    {
        //if it goes into the identifier dump then the table had better have one
        if (GoesIntoIdentifierDump() && TableInfo.IdentifierDumpServer_ID == null)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Column is set to {Destination}  which means its value should be stored in the IdentifierDump but the parent table '{TableInfo}'  doesn't have a dump server configured",
                    CheckResult.Fail));
        else
            notifier.OnCheckPerformed(new CheckEventArgs("Destination is ok", CheckResult.Success));

        if (
            //if column is not diluted (i.e. oblivion or dumped) then there shouldn't be any other columns with the same name
            Destination != DiscardedColumnDestination.Dilute &&

            //are there duplicate named columns?
            TableInfo.GetColumnsAtStage(LoadStage.AdjustRaw)
                .Except(new[] { this })
                .Any(c => c.GetRuntimeName(LoadStage.AdjustRaw).Equals(GetRuntimeName(LoadStage.AdjustRaw),
                    StringComparison.CurrentCultureIgnoreCase)))
            notifier.OnCheckPerformed(
                new CheckEventArgs($"There are 2+ columns called '{GetRuntimeName(LoadStage.AdjustRaw)}' in this table",
                    CheckResult.Fail));
        else
            notifier.OnCheckPerformed(new CheckEventArgs("Name is unique", CheckResult.Success));
    }

    /// <inheritdoc/>
    public string GetRuntimeName() =>
        //belt and bracers, the user could be typing something mental into this field in his database
        MicrosoftQuerySyntaxHelper.Instance.GetRuntimeName(RuntimeColumnName);

    /// <inheritdoc/>
    public string GetRuntimeName(LoadStage stage) => GetRuntimeName();

    /// <summary>
    /// true if destination for column is to store in identifier dump including undiluted versions of dilutes
    /// (Dilution involves making clean values dirty for purposes of anonymisation and storing the clean values in
    /// the Identifier Dump).
    /// </summary>
    /// <returns></returns>
    public bool GoesIntoIdentifierDump() =>
        Destination == DiscardedColumnDestination.StoreInIdentifiersDump
        ||
        Destination == DiscardedColumnDestination.Dilute;

    public void InjectKnown(ITableInfo instance)
    {
        _knownTableInfo = new Lazy<ITableInfo>(instance);
    }

    public void ClearAllInjections()
    {
        _knownTableInfo = new Lazy<ITableInfo>(() => Repository.GetObjectByID<TableInfo>(TableInfo_ID));
    }
}