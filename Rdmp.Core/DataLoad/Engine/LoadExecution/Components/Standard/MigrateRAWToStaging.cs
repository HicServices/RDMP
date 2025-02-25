// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Standard;

/// <summary>
/// DLE component responsible for streaming data off the RAW database and writing it to the STAGING database.  Happens one table at a time with the actual
/// implementation of moving data in MigrateRAWTableToStaging (See MigrateRAWTableToStaging).
/// </summary>
public class MigrateRAWToStaging : DataLoadComponent
{
    private readonly HICDatabaseConfiguration _databaseConfiguration;

    private readonly Stack<IDisposeAfterDataLoad> _toDispose = new();

    public MigrateRAWToStaging(HICDatabaseConfiguration databaseConfiguration,
        HICLoadConfigurationFlags loadConfigurationFlags)
    {
        _databaseConfiguration = databaseConfiguration;

        Description = "Migrate RAW to Staging";
        SkipComponent = !loadConfigurationFlags.DoLoadToStaging;
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        if (Skip(job))
            return ExitCodeType.Error;


        // To be on the safe side, we will create/destroy the staging tables on a per-load basis
        if (_databaseConfiguration.RequiresStagingTableCreation)
            CreateStagingTables(job);

        DoMigration(job, cancellationToken);

        return ExitCodeType.Success;
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        while (_toDispose.Any())
            _toDispose.Pop().LoadCompletedSoDispose(exitCode, postLoadEventListener);
    }

    private void DoMigration(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        foreach (var regularTableInfo in job.RegularTablesToLoad)
            MigrateRAWTableToStaging(job, regularTableInfo, false, cancellationToken);

        foreach (var lookupTableInfo in job.LookupTablesToLoad)
            MigrateRAWTableToStaging(job, lookupTableInfo, true, cancellationToken);
    }


    private void MigrateRAWTableToStaging(IDataLoadJob job, ITableInfo tableInfo, bool isLookupTable,
        GracefulCancellationToken cancellationToken)
    {
        var component = new MigrateRAWTableToStaging(tableInfo, isLookupTable, _databaseConfiguration);
        component.Run(job, cancellationToken);
    }

    private void CreateStagingTables(IDataLoadJob job)
    {
        var cloner = new DatabaseCloner(_databaseConfiguration);
        job.CreateTablesInStage(cloner, LoadBubble.Staging);
    }
}