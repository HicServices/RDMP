// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Linq;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;


/// <summary>
/// Deletes entries that exist in the catalogue already that are not being updated
/// Used to save moving large amounts of data between Raw/Staging/Live when we can figure out which entries already exist
/// </summary>
public class DistinctAgainstCatalogueMutilation : IMutilateDataTables
{
    private DiscoveredDatabase _db;

    //TODO add a timeout for use in the command

    public void Check(ICheckNotifier notifier)
    {
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        _db = dbInfo;
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    private string MySQLGeneration(DiscoveredTable table, TableInfo matchingTable)
    {
        //TODO this needs tested
        var tableColumnNames = table.DiscoverColumns().Select(c => c.GetRuntimeName());
        var pks = matchingTable.ColumnInfos.Where(ci => ci.IsPrimaryKey);
        var nonPks = matchingTable.ColumnInfos.Where(ci => !ci.IsPrimaryKey).Select(c => c.GetRuntimeName()).Where(c => tableColumnNames.Contains(c));
        var pkMatchClause = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} = t2.{pk.GetRuntimeName()}"));
        var nonPksNoMatch = String.Join(" OR ", nonPks.Select(pk => $"t1.{pk} != t2.{pk}"));
        var subqueryPksNull = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} is null"));
        return $@"
                DELETE t2 from {table.GetFullyQualifiedName()} t2
                LEFT JOIN(
                     SELECT * FROM {table.GetFullyQualifiedName()} as t1
                     WHERE NOT EXISTS( SELECT *
                     FROM {matchingTable.GetFullyQualifiedName()} as t2
                     WHERE {pkMatchClause} )
                     UNION
                     SELECT t1.* FROM {table.GetFullyQualifiedName()} as t1
                     INNER JOIN {matchingTable.GetFullyQualifiedName()} as t2
                     on {pkMatchClause}
                     where {nonPksNoMatch}
                ) t1 on {pkMatchClause}
                where {subqueryPksNull}
            ";
    }

    private string PostgreSQLGeneration(DiscoveredTable table, TableInfo matchingTable)
    {
        //TODO this needs tested
        var tableColumnNames = table.DiscoverColumns().Select(c => c.GetRuntimeName());
        var pks = matchingTable.ColumnInfos.Where(ci => ci.IsPrimaryKey);
        var nonPks = matchingTable.ColumnInfos.Where(ci => !ci.IsPrimaryKey).Select(c => c.GetRuntimeName()).Where(c => tableColumnNames.Contains(c));
        var pkMatchClause = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} = t2.{pk.GetRuntimeName()}"));
        var nonPksNoMatch = String.Join(" OR ", nonPks.Select(pk => $"t1.{pk} != t2.{pk}"));
        var subqueryPksNull = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} is null"));
        return $@"
                DELETE t2 from {table.GetFullyQualifiedName()} t2
                LEFT JOIN(
                     SELECT * FROM {table.GetFullyQualifiedName()} as t1
                     WHERE NOT EXISTS( SELECT *
                     FROM {matchingTable.GetFullyQualifiedName()} as t2
                     WHERE {pkMatchClause} )
                     UNION
                     SELECT t1.* FROM {table.GetFullyQualifiedName()} as t1
                     INNER JOIN {matchingTable.GetFullyQualifiedName()} as t2
                     on {pkMatchClause}
                     where {nonPksNoMatch}
                ) t1 on {pkMatchClause}
                where {subqueryPksNull}
            ";
    }
    private string OracleSQLGeneration(DiscoveredTable table, TableInfo matchingTable)
    {
        //TODO this needs tested
        var tableColumnNames = table.DiscoverColumns().Select(c => c.GetRuntimeName());
        var pks = matchingTable.ColumnInfos.Where(ci => ci.IsPrimaryKey);
        var nonPks = matchingTable.ColumnInfos.Where(ci => !ci.IsPrimaryKey).Select(c => c.GetRuntimeName()).Where(c => tableColumnNames.Contains(c));
        var pkMatchClause = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} = t2.{pk.GetRuntimeName()}"));
        var nonPksNoMatch = String.Join(" OR ", nonPks.Select(pk => $"t1.{pk} != t2.{pk}"));
        var subqueryPksNull = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} is null"));
        return $@"
                DELETE t2 from {table.GetFullyQualifiedName()} t2
                LEFT JOIN(
                     SELECT * FROM {table.GetFullyQualifiedName()} as t1
                     WHERE NOT EXISTS( SELECT *
                     FROM {matchingTable.GetFullyQualifiedName()} as t2
                     WHERE {pkMatchClause} )
                     UNION
                     SELECT t1.* FROM {table.GetFullyQualifiedName()} as t1
                     INNER JOIN {matchingTable.GetFullyQualifiedName()} as t2
                     on {pkMatchClause}
                     where {nonPksNoMatch}
                ) t1 on {pkMatchClause}
                where {subqueryPksNull}
            ";
    }

    private static string MicrosoftSQLGeneration(DiscoveredTable table, TableInfo matchingTable)
    {
        var tableColumnNames = table.DiscoverColumns().Select(c => c.GetRuntimeName());
        var pks = matchingTable.ColumnInfos.Where(ci => ci.IsPrimaryKey);
        var nonPks = matchingTable.ColumnInfos.Where(ci => !ci.IsPrimaryKey).Select(c => c.GetRuntimeName()).Where(c => tableColumnNames.Contains(c));
        var pkMatchClause = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} = t2.{pk.GetRuntimeName()}"));
        var nonPksNoMatch = String.Join(" OR ", nonPks.Select(pk => $"t1.{pk} != t2.{pk}"));
        var subqueryPksNull = String.Join(" AND ", pks.Select(pk => $"t1.{pk.GetRuntimeName()} is null"));
        return  $@"
                DELETE t2 from {table.GetFullyQualifiedName()} t2
                LEFT JOIN(
                     SELECT * FROM {table.GetFullyQualifiedName()} as t1
                     WHERE NOT EXISTS( SELECT *
                     FROM {matchingTable.GetFullyQualifiedName()} as t2
                     WHERE {pkMatchClause} )
                     UNION
                     SELECT t1.* FROM {table.GetFullyQualifiedName()} as t1
                     INNER JOIN {matchingTable.GetFullyQualifiedName()} as t2
                     on {pkMatchClause}
                     where {nonPksNoMatch}
                ) t1 on {pkMatchClause}
                where {subqueryPksNull}
            ";
    }

    //what happens if the catalogue table is on a different server?

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        var catalogues = job.LoadMetadata.GetAllCatalogues();
        var tableInfos = catalogues.SelectMany(c => c.CatalogueItems).Select(c => c.ColumnInfo.TableInfo).ToList();
        var tables = _db.DiscoverTables(false);
        foreach (var table in tables)
        {
            var matchingTables = tableInfos.Where(t => t.GetRuntimeName() == table.GetRuntimeName()).Distinct();
            if (matchingTables.Count() > 1)
            {
                throw new Exception("too Many tables");
            }
            if (!matchingTables.Any())
            {
                throw new Exception("no table found!");
            }
            var matchingTable = matchingTables.First();

            var sql = "";
            switch (_db.Server.DatabaseType)
            {
                case FAnsi.DatabaseType.MicrosoftSQLServer:
                    sql = MicrosoftSQLGeneration(table, matchingTable);
                    break;
                case FAnsi.DatabaseType.MySql:
                    sql = MySQLGeneration(table, matchingTable);
                    break;
                case FAnsi.DatabaseType.PostgreSql:
                    sql = PostgreSQLGeneration(table, matchingTable);
                    break;
                case FAnsi.DatabaseType.Oracle:
                    sql = OracleSQLGeneration(table, matchingTable);
                    break;
                default:
                    throw new Exception("Unknown Database Type");
            }
           

            var conn = _db.Server.GetConnection();
            conn.Open();
            using var cmd = _db.Server.GetCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        return ExitCodeType.Success;
    }
}
