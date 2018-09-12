using System;
using System.ComponentModel;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Mutilators;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Mutilators
{
    /// <summary>
    /// Creates a database backup of the LIVE database which contains the specified TableInfo.  Do a test of this component with your server/user configuration
    /// before assuming it will simply work and writing anything drastic.
    ///
    /// <para>This mutilation should only be put into AdjustSTAGING otherwise it will fill up your backup storage as debug load errors in RAW and Migration to STAGING</para>
    /// </summary>
    public class BackupDatabaseMutilation:IMutilateDataTables
    {
        [DemandsInitialization("The database to backup, just select any TableInfo that is part of your load and the entire database will be backed up", Mandatory = true)]
        public TableInfo DatabaseToBackup { get; set; }

        [DemandsInitialization("The number of months the backup will expire after", Mandatory = true)]
        public int MonthsTillExpiry { get; set; }

        
        public void Check(ICheckNotifier notifier)
        {
            if (DatabaseToBackup == null)
                notifier.OnCheckPerformed(new CheckEventArgs("No TableInfo is set, don't know what to backup", CheckResult.Fail, null));
        }

        
        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            
        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            if(loadStage != LoadStage.AdjustStaging && loadStage != LoadStage.PostLoad)
                throw new Exception(typeof(BackupDatabaseMutilation).Name + " can only be done in AdjustStaging or PostLoad (this minimises redundant backups that would otherwise be created while you attempt to fix RAW / constraint related load errors)");
        }

        public ExitCodeType Mutilate(IDataLoadEventListener job)
        {
            var db = DataAccessPortal.GetInstance().ExpectDatabase(DatabaseToBackup, DataAccessContext.DataLoad);
            db.CreateBackup("DataLoadEngineBackup");
            return ExitCodeType.Success;
        }
    }
}
