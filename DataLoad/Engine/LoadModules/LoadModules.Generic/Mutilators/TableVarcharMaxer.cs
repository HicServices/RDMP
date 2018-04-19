using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Widens columns on the specified table to be varchar(max).  This is useful for delaying truncation errors till later in the load.  For example if you
    /// apply a TableVarcharMaxer as a Mounting operation before loading data into it with an IAttacher you can have all the data loaded succesfully into RAW where
    /// it can be interrogated with SQL to find out what the truncated fields are and what to do about them.
    /// 
    /// <para>Remember that RAW and STAGING are created based on the LIVE table schema (but that RAW has no column constraints like pks or not null fields).  This
    /// component lets you further relax the structure of RAW to have varchar(max) column datatypes.  The load will still crash when it comes to migration to 
    /// STAGING or merging with LIVE because the datatypes are not valid according to LIVE but you will have an easier time debugging than trying to look through
    /// a flat file for problematic values.</para>
    /// </summary>
    [Description("Runs in in Mounting or RAW (ideally as the first module in Mounting) which turns all columns in all tables in the bubble into a different data type (e.g. varchar(max)).")]
    public class TableVarcharMaxer : IPluginMutilateDataTables
    {
        private DiscoveredDatabase _dbInfo;
        private LoadStage _loadStage;

        [DemandsInitialization("When true the mutilator will expand for all tables found in staging e.g. RAW regardless of whether they are part of the load.  Only makes a difference if you have table creation scripts in RAW before this component", defaultValue: true)]
        public bool AllTables { get; set; }

        [DemandsInitialization("By default (false) the mutilator will only expand columns with an SQL Type containing 'char', if set to true then this will do all columns including decimals, dates etc", defaultValue: false)]
        public bool AllDataTypes { get; set; }

        [DemandsInitialization(description: "The type that all matching columns will be converted into", defaultValue: "varchar(max)", typeOf: null, mandatory: true)]
        public string DestinationType { get; set; }

        public void Check(ICheckNotifier notifier)
        {
            if (_loadStage > LoadStage.AdjustRaw)
                notifier.OnCheckPerformed(new CheckEventArgs("This module should only run in RAW bubble",CheckResult.Fail));
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            
        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            _dbInfo = dbInfo;
            _loadStage = loadStage;
        }

        public ExitCodeType Mutilate(IDataLoadEventListener job)
        {
            List<DiscoveredTable> tablesToEdit = new List<DiscoveredTable>();
            
            tablesToEdit.AddRange(_dbInfo.DiscoverTables(false));

            if (!AllTables)
            {
                List<DiscoveredTable> expectedToFind = new List<DiscoveredTable>();

                foreach (TableInfo ti in ((IDataLoadJob)job).RegularTablesToLoad)
                {
                    var tiRuntimeName = ti.GetRuntimeName(_loadStage);
                    var found = tablesToEdit.SingleOrDefault(t => t.GetRuntimeName().Equals(tiRuntimeName));

                    if (found == null)
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Did not find expected Table " + tiRuntimeName + " in stage " + _loadStage));


                    expectedToFind.Add(found);
                }

                tablesToEdit = expectedToFind.Intersect(tablesToEdit).ToList();
            }

            foreach (DiscoveredTable discoveredTable in tablesToEdit)
                foreach (var col in discoveredTable.DiscoverColumns())
                {
                    if (AllDataTypes || col.DataType.GetLengthIfString()>=1)
                    {
                        try
                        {
                            col.DataType.AlterTypeTo(DestinationType);
                            job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "converted column " + col + " to " + DestinationType));
                        }
                        catch (Exception e)
                        {
                            job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Failed to convert column " + col + " of data type " + col.DataType + " to destination type " + DestinationType,e));
                        }
                    }
                }

            return ExitCodeType.Success;
        }
    }
}
