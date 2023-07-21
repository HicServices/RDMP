// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
///     Creates a database backup of the LIVE database which contains the specified TableInfo.  Do a test of this component
///     with your server/user configuration
///     before assuming it will simply work and writing anything drastic.
///     <para>
///         This mutilation should only be put into AdjustStaging otherwise it will fill up your backup storage as debug
///         load errors in RAW and Migration to STAGING
///     </para>
/// </summary>
public class BackupDatabaseMutilation : IMutilateDataTables
{
    [DemandsInitialization(
        "The database to backup, just select any TableInfo that is part of your load and the entire database will be backed up",
        Mandatory = true)]
    public TableInfo DatabaseToBackup { get; set; }

    [DemandsInitialization("The number of months the backup will expire after", Mandatory = true)]
    public int MonthsTillExpiry { get; set; }


    public void Check(ICheckNotifier notifier)
    {
        if (DatabaseToBackup == null)
            notifier.OnCheckPerformed(new CheckEventArgs("No TableInfo is set, don't know what to backup",
                CheckResult.Fail));
    }


    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        if (loadStage != LoadStage.AdjustStaging && loadStage != LoadStage.PostLoad)
            throw new Exception(
                $"{nameof(BackupDatabaseMutilation)} can only be done in AdjustStaging or PostLoad (this minimises redundant backups that would otherwise be created while you attempt to fix RAW / constraint related load errors)");
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        var db = DataAccessPortal.ExpectDatabase(DatabaseToBackup, DataAccessContext.DataLoad);
        db.CreateBackup("DataLoadEngineBackup");
        return ExitCodeType.Success;
    }
}