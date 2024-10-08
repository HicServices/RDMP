// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAnsi;
using FAnsi.Connections;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Migration.QueryBuilding;

/// <summary>
///     Migrates from STAGING to LIVE a single table (with a MigrationColumnSet).  This is an UPSERT (new replaces old)
///     operation achieved (in SQL) with MERGE and
///     UPDATE (based on primary key).  Both tables must be on the same server.  A MERGE sql statement will be created
///     using LiveMigrationQueryHelper and executed
///     within a transaction.
/// </summary>
public class OverwriteMigrationStrategy : DatabaseMigrationStrategy
{
    public OverwriteMigrationStrategy(IManagedConnection managedConnection)
        : base(managedConnection)
    {
    }

    public override void MigrateTable(IDataLoadJob job, MigrationColumnSet columnsToMigrate, int dataLoadInfoID,
        GracefulCancellationToken cancellationToken, ref int inserts, ref int updates)
    {
        var server = columnsToMigrate.DestinationTable.Database.Server;

        //see CrossDatabaseMergeCommandTest

        /*          ------------MIGRATE NEW RECORDS (novel by primary key)--------
         *

INSERT INTO CrossDatabaseMergeCommandTo..ToTable (Name,Age,Postcode,hic_dataLoadRunID)
SELECT
[CrossDatabaseMergeCommandFrom]..CrossDatabaseMergeCommandTo_ToTable_STAGING.Name,
[CrossDatabaseMergeCommandFrom]..CrossDatabaseMergeCommandTo_ToTable_STAGING.Age,
[CrossDatabaseMergeCommandFrom]..CrossDatabaseMergeCommandTo_ToTable_STAGING.Postcode,
1
FROM
[CrossDatabaseMergeCommandFrom]..CrossDatabaseMergeCommandTo_ToTable_STAGING
left join
CrossDatabaseMergeCommandTo..ToTable
on
[CrossDatabaseMergeCommandFrom]..CrossDatabaseMergeCommandTo_ToTable_STAGING.Age = CrossDatabaseMergeCommandTo..ToTable.Age
AND
[CrossDatabaseMergeCommandFrom]..CrossDatabaseMergeCommandTo_ToTable_STAGING.Name = CrossDatabaseMergeCommandTo..ToTable.Name
WHERE
CrossDatabaseMergeCommandTo..ToTable.Age is null
*/

        var sbInsert = new StringBuilder();
        var syntax = server.GetQuerySyntaxHelper();


        sbInsert.AppendLine(
            $"INSERT INTO {columnsToMigrate.DestinationTable.GetFullyQualifiedName()} ({string.Join(",", columnsToMigrate.FieldsToUpdate.Select(c => syntax.EnsureWrapped(c.GetRuntimeName())))}");

        //if we are not ignoring the trigger then we should record the data load run ID
        if (!job.LoadMetadata.IgnoreTrigger)
            sbInsert.AppendLine($",{syntax.EnsureWrapped(SpecialFieldNames.DataLoadRunID)}");

        sbInsert.AppendLine(")");

        sbInsert.AppendLine("SELECT");

        // Add the columns we are migrating
        sbInsert.AppendLine(string.Join($",{Environment.NewLine}",
            columnsToMigrate.FieldsToUpdate.Select(c => c.GetFullyQualifiedName())));

        // If we are using trigger also add the run ID e.g. ",50"
        if (!job.LoadMetadata.IgnoreTrigger)
            sbInsert.AppendLine($",{dataLoadInfoID}");

        sbInsert.AppendLine("FROM");
        sbInsert.AppendLine(columnsToMigrate.SourceTable.GetFullyQualifiedName());
        sbInsert.AppendLine("LEFT JOIN");
        sbInsert.AppendLine(columnsToMigrate.DestinationTable.GetFullyQualifiedName());
        sbInsert.AppendLine("ON");

        sbInsert.AppendLine(
            string.Join($" AND {Environment.NewLine}",
                columnsToMigrate.PrimaryKeys.Select(
                    pk =>
                        string.Format("{0}.{1}={2}.{1}", columnsToMigrate.SourceTable.GetFullyQualifiedName(),
                            syntax.EnsureWrapped(pk.GetRuntimeName()),
                            columnsToMigrate.DestinationTable.GetFullyQualifiedName()))));

        sbInsert.AppendLine("WHERE");
        sbInsert.AppendLine(string.Join(" AND ",
            columnsToMigrate.PrimaryKeys.Select(pk =>
                $"{columnsToMigrate.DestinationTable.GetFullyQualifiedName()}.{syntax.EnsureWrapped(pk.GetRuntimeName())} IS NULL")));

        //sbInsert.AppendLine(
        //    $"{columnsToMigrate.DestinationTable.GetFullyQualifiedName()}.{syntax.EnsureWrapped(columnsToMigrate.PrimaryKeys.First().GetRuntimeName())} IS NULL");

        //right at the end of the SELECT
        if (columnsToMigrate.DestinationTable.Database.Server.DatabaseType == DatabaseType.MySql)
            sbInsert.Append(" FOR UPDATE");

        var insertSql = sbInsert.ToString();

        var cmd = server.GetCommand(insertSql, _managedConnection);
        cmd.CommandTimeout = Timeout;

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"INSERT query: {Environment.NewLine}{insertSql}"));

        cancellationToken.ThrowIfCancellationRequested();


        try
        {
            inserts = cmd.ExecuteNonQuery();

            var sqlLines = new List<CustomLine>();

            var toSet = columnsToMigrate.FieldsToUpdate.Where(c => !c.IsPrimaryKey).Select(c =>
                string.Format("t1.{0} = t2.{0}", syntax.EnsureWrapped(c.GetRuntimeName()))).ToArray();

            if (!toSet.Any())
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"Table {columnsToMigrate.DestinationTable} is entirely composed of PrimaryKey columns or hic_ columns so UPDATE will NOT take place"));
                return;
            }

            var toDiff = columnsToMigrate.FieldsToDiff.Where(c => !c.IsPrimaryKey).ToArray();

            if (!toDiff.Any())
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"Table {columnsToMigrate.DestinationTable} is entirely composed of PrimaryKey columns or hic_ columns/ other non DIFF columns that will not result in an UPDATE will NOT take place"));
                return;
            }

            //t1.Name = t2.Name, t1.Age=T2.Age etc
            sqlLines.Add(new CustomLine(string.Join(",", toSet), QueryComponent.SET));

            //also update the hic_dataLoadRunID field
            if (!job.LoadMetadata.IgnoreTrigger)
                sqlLines.Add(new CustomLine(
                    $"t1.{syntax.EnsureWrapped(SpecialFieldNames.DataLoadRunID)}={dataLoadInfoID}",
                    QueryComponent.SET));

            //t1.Name <> t2.Name AND t1.Age <> t2.Age etc
            sqlLines.Add(new CustomLine(string.Join(" OR ", toDiff.Select(c => GetORLine(c, syntax))),
                QueryComponent.WHERE));

            //the join
            sqlLines.AddRange(columnsToMigrate.PrimaryKeys.Select(p =>
                new CustomLine(string.Format("t1.{0} = t2.{0}", syntax.EnsureWrapped(p.GetRuntimeName())),
                    QueryComponent.JoinInfoJoin)));

            var updateHelper = columnsToMigrate.DestinationTable.Database.Server.GetQuerySyntaxHelper().UpdateHelper;

            var updateQuery = updateHelper.BuildUpdate(
                columnsToMigrate.DestinationTable,
                columnsToMigrate.SourceTable,
                sqlLines);

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Update query:{Environment.NewLine}{updateQuery}"));

            var updateCmd = server.GetCommand(updateQuery, _managedConnection);
            updateCmd.CommandTimeout = Timeout;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                updates = updateCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Did not successfully perform the update queries: {updateQuery}", e));
                throw new Exception($"Did not successfully perform the update queries: {updateQuery} - {e}");
            }
        }
        catch (OperationCanceledException)
        {
            throw; // have to catch and rethrow this because of the catch-all below
        }
        catch (Exception e)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Failed to migrate {columnsToMigrate.SourceTable} to {columnsToMigrate.DestinationTable}", e));
            throw new Exception(
                $"Failed to migrate {columnsToMigrate.SourceTable} to {columnsToMigrate.DestinationTable}: {e}");
        }
    }

    private static string GetORLine(DiscoveredColumn c, IQuerySyntaxHelper syntax)
    {
        return string.Format(
            "(t1.{0} <> t2.{0} OR (t1.{0} is null AND t2.{0} is not null) OR (t2.{0} is null AND t1.{0} is not null))",
            syntax.EnsureWrapped(c.GetRuntimeName()));
    }
}