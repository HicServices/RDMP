using System;
using System.Diagnostics;
using System.Linq;
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
    public abstract class MatchingTablesMutilator : IPluginMutilateDataTables
    {
        private readonly LoadStage[] _allowedStages;

        [DemandsInitialization("All tables matching this pattern which have a TableInfo defined in the load will be affected by this mutilation", DefaultValue = ".*")]
        public Regex TableRegexPattern { get; set; }

        [DemandsInitialization("Overrides TableRegexPattern.  If this is set then the tables chosen will be mutilated instead")]
        public TableInfo[] OnlyTables { get; set; }

        [DemandsInitialization("How long to allow for each command to execute in seconds", DefaultValue = 600)]
        public int Timeout { get; set; }

        protected DiscoveredDatabase DbInfo;
        private LoadStage _loadStage;

        protected MatchingTablesMutilator(params LoadStage[] allowedStages)
        {
            _allowedStages = allowedStages;
        }

        public virtual void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            
        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            if(_allowedStages!= null && !_allowedStages.Contains(loadStage))
                throw new NotSupportedException("Mutilation " + GetType() + " is not allowed at stage " + loadStage);

            _loadStage = loadStage;
            DbInfo = dbInfo;
        }

        public ExitCodeType Mutilate(IDataLoadJob job)
        {
            if(TableRegexPattern != null)
                TableRegexPattern = new Regex(TableRegexPattern.ToString(),RegexOptions.IgnoreCase);

            foreach (var tableInfo in job.RegularTablesToLoad)
                if (OnlyTables != null && OnlyTables.Any())
                {
                    if (OnlyTables.Contains(tableInfo))
                        FireMutilate(tableInfo,job);
                }
                else
                if (TableRegexPattern == null)
                    throw new Exception("You must specify either TableRegexPattern or OnlyTables");
                  else
                    if( TableRegexPattern.IsMatch(tableInfo.GetRuntimeName()))
                        FireMutilate(tableInfo, job);
            
            return ExitCodeType.Success;
        }

        private void FireMutilate(TableInfo tableInfo, IDataLoadJob job)
        {
            var tbl = DbInfo.ExpectTable(tableInfo.GetRuntimeName(_loadStage, job.Configuration.DatabaseNamer));

            if (!tbl.Exists())
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Expected table " + tbl + " did not exist in RAW"));
            else
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to run " + GetType() + " mutilation on table " + tbl));
                Stopwatch sw = new Stopwatch();
                sw.Start();
                MutilateTable(job, tableInfo, tbl);
                sw.Stop();
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, GetType() + " mutilation on table " + tbl + " completed after " + sw.ElapsedMilliseconds + " ms"));
            }
        }

        protected abstract void MutilateTable(IDataLoadEventListener job, TableInfo tableInfo, DiscoveredTable table);

        public virtual void Check(ICheckNotifier notifier)
        {
            if (TableRegexPattern == null && (OnlyTables == null || OnlyTables.Length == 0))
                notifier.OnCheckPerformed(new CheckEventArgs("You must specify either a regex pattern (TableRegexPattern) or set OnlyTables for identifying tables which need to be processed", CheckResult.Fail));
        }

    }
}