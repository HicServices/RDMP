// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
///     Data load component which can delete records in an (unconstrained) RAW table to enforce uniqueness of the primary
///     key field (as it is configured in LIVE).
///     This lets you resolve non-exact duplication based on column order (e.g. if there is a collision where one has an
///     later 'DataAge' field then use the later
///     one and discard the earlier one.
///     <para>
///         This is a very dangerous operation which uses the primary key collision resolution order (Accessible through
///         CatalogueManager by right clicking a
///         TableInfo and choosing 'Configure Primary Key Collision Resolution') to delete records in a preferred order,
///         fully eliminating primary key collisions.
///         It is a very good idea to not have this task until you are absolutely certain that your primary key is correct
///         and that the duplicate records being deleted
///         are the correct decisions e.g. delete an older record in a given load batch and not simply erasing vast swathes
///         of data!.  The Data Load Engine will tell
///         you with a warning when records are deleted and how many.  If you notice a lot of deletion then try removing
///         this component and manually inspecting the data
///         in the RAW database after the data load fails (due to unresolved primary key conflicts)
///     </para>
///     <para>
///         This component requires that a collision resolution order has been configured on the TableInfo (See
///         ConfigurePrimaryKeyCollisionResolution)
///     </para>
/// </summary>
public class PrimaryKeyCollisionResolverMutilation : IPluginMutilateDataTables
{
    [DemandsInitialization(
        "The table on which to resolve primary key collisions, must have PrimaryKeyCollision resolution setup for it in the Data Catalogue",
        Mandatory = true)]
    public TableInfo TargetTable { get; set; }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    private DiscoveredDatabase _dbInfo;

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        if (loadStage != LoadStage.AdjustRaw)
            throw new Exception(
                $"Primary key collisions can only be resolved in a RAW environment, current load stage is:{loadStage} (The reason for this is because there should be primary keys in the database level in STAGING and LIVE making primary key collisions IMPOSSIBLE)");

        _dbInfo = dbInfo;
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        ResolvePrimaryKeyConflicts(job);
        return ExitCodeType.Success;
    }


    private void ResolvePrimaryKeyConflicts(IDataLoadEventListener job)
    {
        using var con = (SqlConnection)_dbInfo.Server.GetConnection();
        con.Open();

        var resolver = new PrimaryKeyCollisionResolver(TargetTable);
        var cmdAreTherePrimaryKeyCollisions = new SqlCommand(resolver.GenerateCollisionDetectionSQL(), con)
        {
            CommandTimeout = 5000
        };

        //if there are no primary key collisions
        if (cmdAreTherePrimaryKeyCollisions.ExecuteScalar().ToString().Equals("0"))
        {
            job.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information, "No primary key collisions detected"));
            return;
        }

        //there are primary key collisions so resolve them
        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Primary key collisions detected"));

        var cmdResolve = new SqlCommand(resolver.GenerateSQL(), con)
        {
            CommandTimeout = 5000
        };
        var affectedRows = cmdResolve.ExecuteNonQuery();

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
            $"Primary key collisions resolved by deleting {affectedRows} rows"));
    }

    public void Check(ICheckNotifier notifier)
    {
        if (TargetTable == null)
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Target table is null, a table must be specified upon which to resolve primary key duplication (that TableInfo must have a primary key collision resolution order)",
                CheckResult.Fail));

        try
        {
            var resolver = new PrimaryKeyCollisionResolver(TargetTable);
            var sql = resolver.GenerateSQL();
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Failed to check PrimaryKeyCollisionResolver on {TargetTable}", CheckResult.Fail, e));
        }
    }
}