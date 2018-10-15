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

        [DemandsInitialization("All tables in RAW matching this pattern which have a TableInfo defined in the load will be affected by this mutilation", Mandatory = true, DefaultValue = ".*")]
        public Regex TableRegexPattern { get; set; }

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

        public ExitCodeType Mutilate(IDataLoadEventListener job)
        {
            var j = (IDataLoadJob) job;

            TableRegexPattern = new Regex(TableRegexPattern.ToString(),RegexOptions.IgnoreCase);
            
            foreach (var tableInfo in j.RegularTablesToLoad)
                if (TableRegexPattern.IsMatch(tableInfo.GetRuntimeName()))
                {
                    var tbl = DbInfo.ExpectTable(tableInfo.GetRuntimeName(_loadStage,j.Configuration.DatabaseNamer));
                    
                    if(!tbl.Exists())
                        job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Expected table "+ tbl + " did not exist in RAW"));
                    else
                    {
                        job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "About to run " + GetType() + " mutilation on table " + tbl));
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        MutilateTable(job, tableInfo, tbl);    
                        sw.Stop();
                        job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, GetType() + " mutilation on table " + tbl + " completed after " + sw.ElapsedMilliseconds + " ms"));
                    }
                }

            return ExitCodeType.Success;
        }

        protected abstract void MutilateTable(IDataLoadEventListener job, TableInfo tableInfo, DiscoveredTable table);

        public virtual void Check(ICheckNotifier notifier)
        {
            if (TableRegexPattern == null)
                notifier.OnCheckPerformed(new CheckEventArgs("You must specify a regex pattern for identifying tables in RAW which need to be processed", CheckResult.Fail));
        }

    }
}