// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;

/// <summary>
///     Engine class for converting a ColumnInfo and all the data in it into ANO equivalents (See
///     ColumnInfoToANOTableConverterUI).
/// </summary>
public class ColumnInfoToANOTableConverter
{
    private readonly ColumnInfo _colToNuke;
    private readonly ANOTable _toConformTo;
    private readonly TableInfo _tableInfo;
    private ColumnInfo _newANOColumnInfo;

    public ColumnInfoToANOTableConverter(ColumnInfo colToNuke, ANOTable toConformTo)
    {
        _tableInfo = colToNuke.TableInfo;
        _colToNuke = colToNuke;
        _toConformTo = toConformTo;
    }

    public bool ConvertEmptyColumnInfo(Func<string, bool> shouldApplySql, ICheckNotifier notifier)
    {
        var tbl = _tableInfo.Discover(DataAccessContext.DataLoad);

        var rowcount = tbl.GetRowCount();

        if (rowcount > 0)
            throw new NotSupportedException(
                $"Table {_tableInfo} contains {rowcount} rows of data, you cannot use ColumnInfoToANOTableConverter.ConvertEmptyColumnInfo on this table");

        using var con = tbl.Database.Server.GetConnection();
        con.Open();

        if (!IsOldColumnDroppable(con, notifier))
            return false;

        EnsureNoTriggerOnTable(tbl);

        AddNewANOColumnInfo(shouldApplySql, con, notifier);

        DropOldColumn(shouldApplySql, con, null);


        //synchronize again
        new TableInfoSynchronizer(_tableInfo).Synchronize(notifier);

        return true;
    }

    public bool ConvertFullColumnInfo(Func<string, bool> shouldApplySql, ICheckNotifier notifier)
    {
        var tbl = _tableInfo.Discover(DataAccessContext.DataLoad);

        using var con = tbl.Database.Server.GetConnection();
        con.Open();

        if (!IsOldColumnDroppable(con, notifier))
            return false;

        EnsureNoTriggerOnTable(tbl);

        AddNewANOColumnInfo(shouldApplySql, con, notifier);

        MigrateExistingData(shouldApplySql, con, notifier, tbl);

        DropOldColumn(shouldApplySql, con, null);

        //synchronize again
        new TableInfoSynchronizer(_tableInfo).Synchronize(notifier);

        return true;
    }

    private void EnsureNoTriggerOnTable(DiscoveredTable tbl)
    {
        var triggerFactory = new TriggerImplementerFactory(tbl.Database.Server.DatabaseType);

        var triggerImplementer = triggerFactory.Create(tbl);

        if (triggerImplementer.GetTriggerStatus() != TriggerStatus.Missing)
            throw new NotSupportedException(
                $"Table {_tableInfo} has a backup trigger on it, this will destroy performance and break when we add the ANOColumn, dropping the trigger is not an option because of the _Archive table still containing identifiable data (and other reasons)");
    }

    private void MigrateExistingData(Func<string, bool> shouldApplySql, DbConnection con, ICheckNotifier notifier,
        DiscoveredTable tbl)
    {
        var from = _colToNuke.GetRuntimeName(LoadStage.PostLoad);
        var to = _newANOColumnInfo.GetRuntimeName(LoadStage.PostLoad);


        //create an empty table for the anonymised data
        using (var cmdCreateTempMap = DatabaseCommandHelper.GetCommand(
                   $"SELECT top 0 {from},{to} into TempANOMap from {tbl.GetFullyQualifiedName()}",
                   con))
        {
            if (!shouldApplySql(cmdCreateTempMap.CommandText))
                throw new Exception("User decided not to create the TempANOMap table");

            cmdCreateTempMap.ExecuteNonQuery();
        }

        try
        {
            using (var dt = new DataTable())
            {
                //get the existing data
                using var cmdGetExistingData =
                    DatabaseCommandHelper.GetCommand(
                        $"SELECT {from},{to} from {tbl.GetFullyQualifiedName()}", con);
                using var da = DatabaseCommandHelper.GetDataAdapter(cmdGetExistingData);
                da.Fill(dt); //into memory

                //transform it in memory
                var transformer =
                    new ANOTransformer(_toConformTo, new FromCheckNotifierToDataLoadEventListener(notifier));
                transformer.Transform(dt, dt.Columns[0], dt.Columns[1]);

                var tempAnoMapTbl = tbl.Database.ExpectTable("TempANOMap");

                using var insert = tempAnoMapTbl.BeginBulkInsert();
                insert.Upload(dt);
            }


            //create an empty table for the anonymised data
            using var cmdUpdateMainTable = DatabaseCommandHelper.GetCommand(
                string.Format(
                    "UPDATE source set source.{1} = map.{1} from {2} source join TempANOMap map on source.{0}=map.{0}",
                    from, to, tbl.GetFullyQualifiedName()), con);
            if (!shouldApplySql(cmdUpdateMainTable.CommandText))
                throw new Exception("User decided not to perform update on table");
            cmdUpdateMainTable.ExecuteNonQuery();
        }
        finally
        {
            //always drop the temp anomap
            using var dropMappingTable = DatabaseCommandHelper.GetCommand("DROP TABLE TempANOMap", con);
            dropMappingTable.ExecuteNonQuery();
        }
    }


    private void DropOldColumn(Func<string, bool> shouldApplySql, DbConnection con, DbTransaction transaction)
    {
        var alterSql = $"ALTER TABLE {_tableInfo.Name} Drop column {_colToNuke.GetRuntimeName(LoadStage.PostLoad)}";

        if (shouldApplySql(alterSql))
        {
            using var cmd = DatabaseCommandHelper.GetCommand(alterSql, con, transaction);
            cmd.ExecuteNonQuery();
        }
        else
        {
            throw new Exception($"User chose not to drop the old column {_colToNuke}");
        }
    }


    private bool IsOldColumnDroppable(DbConnection con, ICheckNotifier notifier)
    {
        try
        {
            var transaction = con.BeginTransaction();

            //try dropping it within a transaction
            DropOldColumn(s => true, con, transaction);

            //it is droppable - rollback that drop!
            transaction.Rollback();
            transaction.Dispose();

            return true;
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Could not perform transformation because column {_colToNuke} is not droppable",
                    CheckResult.Fail, e));
            return false;
        }
    }

    private void AddNewANOColumnInfo(Func<string, bool> shouldApplySql, DbConnection con, ICheckNotifier notifier)
    {
        var anoColumnNameWillBe = $"ANO{_colToNuke.GetRuntimeName(LoadStage.PostLoad)}";

        var alterSql =
            $"ALTER TABLE {_tableInfo.Name} ADD {anoColumnNameWillBe} {_toConformTo.GetRuntimeDataType(LoadStage.PostLoad)}";

        if (shouldApplySql(alterSql))
        {
            using (var cmd = DatabaseCommandHelper.GetCommand(alterSql, con))
            {
                cmd.ExecuteNonQuery();
            }

            var synchronizer = new TableInfoSynchronizer(_tableInfo);
            synchronizer.Synchronize(notifier);

            //now get the new ANO columninfo
            _newANOColumnInfo = _tableInfo.ColumnInfos.Single(c => c.GetRuntimeName().Equals(anoColumnNameWillBe));
            _newANOColumnInfo.ANOTable_ID = _toConformTo.ID;
            _newANOColumnInfo.SaveToDatabase();
        }
        else
        {
            throw new Exception("User chose not to apply part of the operation");
        }
    }
}