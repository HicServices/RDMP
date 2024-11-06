// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using System.Text;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataLoad.Triggers;

/// <summary>
/// Calculates the changes that occured during a given DLE execution (identified by the unique DataLoadRunID of the load).  This involves generating and running SELECT
/// queries that merge _Archive and live tables data to produce 3 DataTables Inserts and Updates (2 tables - New and Replaced).
/// </summary>
public class DiffDatabaseDataFetcher
{
    private readonly int _batchSize;
    private readonly ITableInfo _tableInfo;
    private readonly int _dataLoadRunID;
    private readonly int _timeout;

    public DataTable Inserts { get; private set; }
    public DataTable Updates_New { get; private set; }
    public DataTable Updates_Replaced { get; private set; }

    public ColumnInfo[] _pks;
    private ColumnInfo[] _sharedColumns;

    public DiffDatabaseDataFetcher(int batchSize, ITableInfo tableInfo, int dataLoadRunID, int timeout)
    {
        _batchSize = batchSize;
        _tableInfo = tableInfo;
        _dataLoadRunID = dataLoadRunID;
        _timeout = timeout;
    }

    public void FetchData(ICheckNotifier checkNotifier)
    {
        try
        {
            DiscoveredDatabase database;
            DiscoveredServer server;

            try
            {
                database = DataAccessPortal.ExpectDatabase(_tableInfo, DataAccessContext.InternalDataProcessing);
                server = database.Server;
            }
            catch (Exception ex)
            {
                checkNotifier.OnCheckPerformed(new CheckEventArgs(
                    $"Could not connect to data access point TableInfo {_tableInfo}", CheckResult.Fail, ex));
                return;
            }


            if (database.Exists())
                checkNotifier.OnCheckPerformed(new CheckEventArgs($"Verified database exists {database}",
                    CheckResult.Success));

            var liveTable = _tableInfo.Discover(DataAccessContext.InternalDataProcessing);
            var archiveTable = database.ExpectTable($"{_tableInfo.GetRuntimeName()}_Archive", liveTable.Schema);

            if (liveTable.Exists())
                checkNotifier.OnCheckPerformed(new CheckEventArgs($"Verified table exists {_tableInfo}",
                    CheckResult.Success));

            if (archiveTable.Exists())
                checkNotifier.OnCheckPerformed(new CheckEventArgs($"Verified Archive table exists {archiveTable}",
                    CheckResult.Success));
            else
                checkNotifier.OnCheckPerformed(new CheckEventArgs(
                    $"Did not find an Archive table called {archiveTable}", CheckResult.Fail));

            var allCols = _tableInfo.ColumnInfos.ToArray();
            var allArchiveCols = archiveTable.DiscoverColumns().ToArray();

            _pks = allCols.Where(c => c.IsPrimaryKey).ToArray();

            if (_pks.Any())
                checkNotifier.OnCheckPerformed(new CheckEventArgs(
                    $"Found the following primary keys:{string.Join(",", _pks.Select(p => p.GetRuntimeName()))}",
                    CheckResult.Success));
            else
                checkNotifier.OnCheckPerformed(new CheckEventArgs(
                    "Table does not have any ColumnInfos marked with IsPrimaryKey (try synchronizing the TableInfo if you are sure you have some",
                    CheckResult.Fail));

            _sharedColumns =
                allCols.Where( //from all columns take all columns where
                    c => allArchiveCols.Any(
                        //there is a column with the same name in the archive columns (ignoring case)
                        archiveCol => c.GetRuntimeName().Equals(archiveCol.GetRuntimeName(),
                                          StringComparison.InvariantCultureIgnoreCase)

                                      //but don't care about differences in these columns (e.g. the actual data load run id will obviously be different!)
                                      && !SpecialFieldNames.IsHicPrefixed(c)
                    )).ToArray();

            checkNotifier.OnCheckPerformed(new CheckEventArgs(
                $"Shared columns between the archive and the live table are {string.Join(",", _sharedColumns.Select(c => c.GetRuntimeName()))}",
                CheckResult.Success));

            GetInsertData(server, database, checkNotifier);
            GetUpdatedData(server, database, checkNotifier);
        }
        catch (Exception e)
        {
            checkNotifier.OnCheckPerformed(new CheckEventArgs("Fatal error trying to fetch data", CheckResult.Fail, e));
        }
    }

    private void GetInsertData(DiscoveredServer server, DiscoveredDatabase database, ICheckNotifier checkNotifier)
    {
        var memoryRepository = new MemoryCatalogueRepository();

        var syntaxHelper = server.GetQuerySyntaxHelper();
        var tableName = _tableInfo.Name;
        var archiveTableName = syntaxHelper.EnsureFullyQualified(database.GetRuntimeName(), _tableInfo.Schema,
            $"{_tableInfo.GetRuntimeName()}_Archive");

        var whereStatement = "";

        foreach (var pk in _pks)
            whereStatement += string.Format("{0}.{1} = {2}.{1} AND ",
                tableName,
                syntaxHelper.EnsureWrapped(pk.GetRuntimeName()),
                archiveTableName);

        var qb = new QueryBuilder(null, null, new[] { _tableInfo })
        {
            TopX = _batchSize
        };
        qb.AddColumnRange(_tableInfo.ColumnInfos.Select(c => new ColumnInfoToIColumn(memoryRepository, c)).ToArray());

        //where
        var filter1 = new SpontaneouslyInventedFilter(memoryRepository, null,
            $"{syntaxHelper.EnsureWrapped(SpecialFieldNames.DataLoadRunID)} = {_dataLoadRunID}",
            "DataLoadRunID matches", null, null);
        var filter2 =
            new SpontaneouslyInventedFilter(memoryRepository, null,
                $@" not exists (
select 1 from {archiveTableName} where {whereStatement} {syntaxHelper.EnsureWrapped(SpecialFieldNames.DataLoadRunID)} < {_dataLoadRunID}
)",
                "Record doesn't exist in archive", null, null);

        qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(memoryRepository, null,
            new[] { filter1, filter2 }, FilterContainerOperation.AND);

        Inserts = new DataTable();
        FillTableWithQueryIfUserConsents(Inserts, qb.SQL, checkNotifier, server);
    }


    private void GetUpdatedData(DiscoveredServer server, DiscoveredDatabase database, ICheckNotifier checkNotifier)
    {
        const string archive = "archive";
        const string zzArchive = "zzarchivezz";

        var syntaxHelper = server.GetQuerySyntaxHelper();

        var tableName = _tableInfo.Name;
        var archiveTableName = syntaxHelper.EnsureFullyQualified(database.GetRuntimeName(), _tableInfo.Schema,
            $"{_tableInfo.GetRuntimeName()}_Archive");

        var whereStatement = string.Join(" AND ",
            _pks.Select(pk => string.Format("{0}.{1} = {2}.{1} ", tableName, pk.GetRuntimeName(), archiveTableName)));

        //hold onto your hats ladies and gentlemen, we start by selecting every column twice with a cross apply:
        //once from the main table e.g. Col1,Col2,Col3
        //then once from the archive e.g. zzArchivezzCol1, zzArchivezzCol2, zzArchivezzCol3 -- notice this is a query alias not affecting anything underlying
        //this lets us then fill 2 DataTables from the combo table we get back with absolute assurity of same row semantically by primary key

        var sql = "";

        switch (syntaxHelper.DatabaseType)
        {
            case DatabaseType.MicrosoftSQLServer:
                sql = $@"
--Records which appear in the archive
SELECT top {{0}}
{{6}},
{{7}},
{{8}}
FROM    {{1}}
CROSS APPLY
        (
        SELECT  TOP 1 {{2}}.*
        FROM    {{2}}
        WHERE
         {{3}}
         order by {syntaxHelper.EnsureWrapped(SpecialFieldNames.ValidFrom)} desc
        ) {{9}}
where
{{1}}.{{4}} = {{5}}";
                break;

            case DatabaseType.Oracle:
            case DatabaseType.MySql:
            case DatabaseType.PostgreSql:


                sql = $@"
/*Records which appear in the archive*/
SELECT
{{6}},
{{7}},
{{8}}
FROM
{{1}}
Join
{{2}} {{9}} on {whereStatement.Replace(archiveTableName, archive)}
 AND
 {{9}}.{{10}} = (select max({syntaxHelper.EnsureWrapped(SpecialFieldNames.ValidFrom)}) from {{2}} s where {whereStatement.Replace(archiveTableName, archive).Replace(tableName, "s")})
 where
  {{1}}.{{4}} = {{5}}

";
                sql += syntaxHelper.HowDoWeAchieveTopX(_batchSize).SQL;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        sql = string.Format(sql,
            _batchSize, //{0}
            tableName, //{1}
            archiveTableName, //{2}
            whereStatement, //{3}
            syntaxHelper.EnsureWrapped(SpecialFieldNames.DataLoadRunID), //{4}
            _dataLoadRunID, //{5}
            GetSharedColumnsSQL(tableName), //{6}
            GetSharedColumnsSQLWithColumnAliasPrefix(archive, zzArchive), //{7}
            GetHICSpecialColumns(archive, zzArchive),//{8}
            archive, //{9}
            syntaxHelper.EnsureWrapped(SpecialFieldNames.ValidFrom) //{10}
        );

        var dtComboTable = new DataTable();
        FillTableWithQueryIfUserConsents(dtComboTable, sql, checkNotifier, server);

        Updates_New = new DataTable();
        Updates_Replaced = new DataTable();

        //add the columns from the combo table to both views
        foreach (DataColumn col in dtComboTable.Columns)
            if (!col.ColumnName.StartsWith(zzArchive, StringComparison.InvariantCultureIgnoreCase))
            {
                Updates_New.Columns.Add(col.ColumnName, col.DataType);
                Updates_Replaced.Columns.Add(col.ColumnName, col.DataType);
            }

        foreach (DataRow fromRow in dtComboTable.Rows)
        {
            var newRow = Updates_New.Rows.Add();
            var replacedRow = Updates_Replaced.Rows.Add();

            foreach (DataColumn column in dtComboTable.Columns)
                if (column.ColumnName.StartsWith(zzArchive, StringComparison.InvariantCultureIgnoreCase))
                    replacedRow[column.ColumnName[zzArchive.Length..]] = fromRow[column];
                else
                    newRow[column.ColumnName] = fromRow[column];
        }
    }

    private string GetHICSpecialColumns(string tableName, string columnAliasString)
    {
        return $@"{tableName}.{SpecialFieldNames.DataLoadRunID} as {SpecialFieldNames.DataLoadRunID},
{tableName}.{SpecialFieldNames.ValidFrom} as {SpecialFieldNames.ValidFrom}
";
    }

    private string GetSharedColumnsSQLWithColumnAliasPrefix(string tableName, string columnAliasPrefix)
    {
        var sb = new StringBuilder();

        foreach (var sharedColumn in _sharedColumns)
        {
            sb.AppendLine();
            sb.Append(
                $"{tableName}.{sharedColumn.GetRuntimeName()} {columnAliasPrefix}{sharedColumn.GetRuntimeName()}");
            sb.Append(',');
        }

        return sb.ToString().TrimEnd(',');
    }

    private string GetSharedColumnsSQL(string tableName)
    {
        var sb = new StringBuilder();

        foreach (var sharedColumn in _sharedColumns)
        {
            sb.AppendLine();
            sb.Append($"{tableName}.{sharedColumn.GetRuntimeName()}");
            sb.Append(',');
        }

        return sb.ToString().TrimEnd(',');
    }


    private void FillTableWithQueryIfUserConsents(DataTable dt, string sql, ICheckNotifier checkNotifier,
        DiscoveredServer server)
    {
        var execute = checkNotifier.OnCheckPerformed(
            new CheckEventArgs("About to fetch data, confirming user is happy with SQL", CheckResult.Warning, null,
                sql));

        if (execute)
        {
            using var con = server.GetConnection();
            con.Open();

            using var cmd = server.GetCommand(sql, con);
            cmd.CommandTimeout = _timeout;
            using var da = server.GetDataAdapter(cmd);
            da.Fill(dt);
        }
        else
        {
            checkNotifier.OnCheckPerformed(new CheckEventArgs("User decided not to execute the SQL", CheckResult.Fail));
        }
    }
}