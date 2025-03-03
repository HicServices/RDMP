// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.DataLoad.Engine.Migration;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Standard;

/// <summary>
/// DLE component responsible for merging records in the STAGING database into the LIVE database table(s) during a Data Load Engine execution.  The actual
/// implementation of migrating records done by MigrationHost and MigrationConfiguration.
/// </summary>
public class MigrateStagingToLive : DataLoadComponent
{
    private readonly HICDatabaseConfiguration _databaseConfiguration;

    public MigrateStagingToLive(HICDatabaseConfiguration databaseConfiguration,
        HICLoadConfigurationFlags loadConfigurationFlags)
    {
        _databaseConfiguration = databaseConfiguration;

        Description = "Migrate Staging to Live";
        SkipComponent = !loadConfigurationFlags.DoMigrateFromStagingToLive;
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        if (Skip(job)) return ExitCodeType.Error;

        //if(_migrationHost != null)
        //    throw new Exception("Load stage already started once");

        // After the user-defined load process, the framework handles the insert into staging and resolves any conflicts
        var stagingDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Staging];
        var liveDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Live];

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Migrating '{stagingDbInfo}' to '{liveDbInfo}'"));

        var migrationConfig = new MigrationConfiguration(stagingDbInfo, LoadBubble.Staging, LoadBubble.Live,
            _databaseConfiguration.DatabaseNamer);
        var migrationHost = new MigrationHost(stagingDbInfo, liveDbInfo, migrationConfig, _databaseConfiguration);
        migrationHost.Migrate(job, cancellationToken);

        return ExitCodeType.Success;
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }
}