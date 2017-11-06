using System;
using System.ComponentModel;
using System.Linq;
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
    [Description("Conditionally ends the data load early if a given set of circumstances occurs e.g. you might choose to return LoadNotRequired if there are no records in RAW")]
    public class PrematureLoadEnder : IPluginMutilateDataTables
    {
        private DiscoveredDatabase _databaseInfo;

        [DemandsInitialization("An exit code that reflects the nature off the stop.  Do not set Success because loads do not stop halfway through Successfully.  If you do not want an error use LoadNotRequired")]
        public ExitCodeType ExitCodeToReturnIfConditionMet { get; set; }
   
        [DemandsInitialization("Condition under which to return the exit code.  Use cases for Always are few and far between I guess if you have a big configuration but you want to stop it running ever you could put an Always abort step in")]
        public PrematureLoadEndCondition ConditionsToTerminateUnder { get; set; }

        public void Check(ICheckNotifier notifier)
        {
            if (ExitCodeToReturnIfConditionMet == ExitCodeType.Success)
                notifier.OnCheckPerformed(new CheckEventArgs("You cannot return Success if you are anticipating terminating the load early, you must choose LoadNotRequired or Error",CheckResult.Fail));

            if (ConditionsToTerminateUnder == PrematureLoadEndCondition.Always)
                notifier.OnCheckPerformed(new CheckEventArgs("ConditionsToTerminateUnder is Always.  This means that the load will not complete if executed",CheckResult.Warning));
        }

        
        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {

        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            _databaseInfo = dbInfo;
        }

        public ExitCodeType Mutilate(IDataLoadEventListener job)
        {
            if (ConditionsToTerminateUnder == PrematureLoadEndCondition.Always)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "ConditionsToTerminateUnder is " + ConditionsToTerminateUnder + " so terminating load with " +ExitCodeToReturnIfConditionMet));
                return ExitCodeToReturnIfConditionMet;
            }

            if(ConditionsToTerminateUnder == PrematureLoadEndCondition.NoRecordsInAnyTablesInDatabase)
            {
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "About to inspect what tables have rows in them in database " + _databaseInfo.GetRuntimeName()));

                foreach (var t in _databaseInfo.DiscoverTables(false))
                {
                    int rowCount = t.GetRowCount();

                    job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Found table " + t.GetRuntimeName() + " with row count " + rowCount));

                    if(rowCount > 0)
                    {
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Found at least 1 record in 1 table so condition " + ConditionsToTerminateUnder + " is not met.  Therefore returning Success so the load can continue normally."));
                        return ExitCodeType.Success;
                    }
                }

                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "No tables had any rows in them so returning " + ExitCodeToReturnIfConditionMet + " which should terminate the load here"));
                return ExitCodeToReturnIfConditionMet;
            }

            if (ConditionsToTerminateUnder == PrematureLoadEndCondition.NoFilesInForLoading)
            {
                var dataLoadJob = job as IDataLoadJob;

                if(dataLoadJob == null)
                    throw new Exception("IDataLoadEventListener " + job + " was not an IDataLoadJob (very unexpected)");

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to check ForLoading directory for files, the directory is:" + dataLoadJob.HICProjectDirectory.ForLoading.FullName));

                var files = dataLoadJob.HICProjectDirectory.ForLoading.GetFiles();

                if (!files.Any())
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No files in ForLoading so returning " + ExitCodeToReturnIfConditionMet + " which should terminate the load here"));
                    return ExitCodeToReturnIfConditionMet;
                }

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Found " + files.Length + " files in ForLoading so not terminating (" + string.Join(",", files.Select(f => f.Name)) + ")"));
                
                //There were 
                return ExitCodeType.Success;
            }
                
            throw new Exception("Didn't know how to handle condition:" + ConditionsToTerminateUnder);
        }
    }
}