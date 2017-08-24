using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Mutilators;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Mutilators
{
    
    [Description("Backs up the LIVE database of the supplied TableInfo (This mutilation should only be put into AdjustSTAGING otherwise it will fill up your backup storage as debug load errors in RAW and Migration to STAGING)")]
    public class BackupDatabaseMutilation:IMutilateDataTables
    {
        private DiscoveredDatabase _dbInfo;

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

            _dbInfo = dbInfo;
        }

        public ExitCodeType Mutilate(IDataLoadEventListener job)
        {
            UsefulStuff.BackupSqlServerDatabase(_dbInfo.Server.Builder.ConnectionString, _dbInfo.GetRuntimeName(), "DataLoadEngineBackup", MonthsTillExpiry);
            return ExitCodeType.Success;
        }
    }
}
