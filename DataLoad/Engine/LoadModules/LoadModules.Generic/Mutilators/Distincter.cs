using System.Text.RegularExpressions;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using DataLoadEngine.Mutilators;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Mutilators
{
    /// <summary>
    /// This component will make all tables matching the <see cref="TableRegexPattern"/> distinct.  It should only be run in RAW.
    /// 
    /// <para>The RAW=>STAGING migration will already take care of identical records.  This means this component is only useful for
    /// debugging failed RAW batches or running SQL that is intented to resolve PK collisions / data integrity issues where identical 
    /// duplicates make the process slow / difficult.</para>
    /// </summary>
    class Distincter:IMutilateDataTables
    {
        private DiscoveredDatabase _dbInfo;

        [DemandsInitialization("All tables in RAW matching this pattern which have a TableInfo defined in the load will be affected by this mutilation", Mandatory = true, DefaultValue = ".*")]
        public Regex TableRegexPattern { get; set; }
        
        [DemandsInitialization("How long to allow for each command to execute in seconds", DefaultValue = 600)]
        public int Timeout { get; set; }

        public void Check(ICheckNotifier notifier)
        {
            
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            
        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            _dbInfo = dbInfo;
        }

        public ExitCodeType Mutilate(IDataLoadEventListener job)
        {
            var j = (IDataLoadJob)job;
            foreach (var tableInfo in j.RegularTablesToLoad)
            {

                var tbl = _dbInfo.ExpectTable(tableInfo.GetRuntimeName());
                var tblName = tbl.GetRuntimeName();

                if (tbl.Exists() && TableRegexPattern.IsMatch(tblName))
                    tbl.MakeDistinct(Timeout);
            }
            
            return ExitCodeType.Success;
        }
    }
}
