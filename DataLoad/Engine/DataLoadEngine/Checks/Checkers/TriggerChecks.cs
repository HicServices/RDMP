using System;
using System.Linq;
using CatalogueLibrary.Triggers;
using CatalogueLibrary.Triggers.Exceptions;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Checks.Checkers
{
    class TriggerChecks : ICheckable
    {
        private readonly DiscoveredDatabase _dbInfo;
        private readonly string _tableName;
        private readonly bool _expectedPresence;
        private readonly string[] _primaryKeys;

        public bool TriggerCreated { get; private set; }

        public TriggerChecks(DiscoveredDatabase dbInfo,string tableName, bool expectedPresence,string[] primaryKeys)
        {
            _dbInfo = dbInfo;
            _tableName = tableName;
            _expectedPresence = expectedPresence;
            _primaryKeys = primaryKeys ?? new string[0];
        }


        public void Check(ICheckNotifier notifier)
        {


            DiscoveredTable liveTable = _dbInfo.ExpectTable(_tableName);
            DiscoveredTable archiveTable = _dbInfo.ExpectTable(_tableName +"_Archive");


            if(liveTable.Exists() && archiveTable.Exists())
            {
                string[] liveCols = liveTable.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();
                string[] archiveCols = archiveTable.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();

                var passed = CheckColumnOrderInTablesAndArchiveMatch(liveCols, archiveCols, notifier);

                if(!passed)
                    return;
            }


            TriggerImplementer trigger = new TriggerImplementer(_dbInfo, _tableName);

            bool present;

            //we don't know the primary keys
            if (!_primaryKeys.Any())
                try
                {
                    //see if it exists
                    present = trigger.CheckUpdateTriggerIsEnabledOnServer() == TriggerImplementer.TriggerStatus.Enabled;
                }
                catch (MissingObjectException)
                {
                    //clearly it doesnt exist
                    present = false;
                }
            else
            {
                try
                {
                    //we do know the primary keys
                    present = trigger.CheckUpdateTriggerIsEnabled_Advanced(_primaryKeys);
                }
                catch (IrreconcilableColumnDifferencesInArchiveException e)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Archive table for table " + _tableName +
                            " is corrupt, see inner Exception for specific errors", CheckResult.Fail, e));
                    return;
                }
                catch (Exception e)
                {
                    NotifyFail(e,notifier,trigger);
                    return;
                }
            }

            //we expected it to be missing and it was or we expected it to be enabled and it was
            if(present == _expectedPresence)
                notifier.OnCheckPerformed(new CheckEventArgs("Trigger presence/intactness for table " + _tableName + " matched expected presence (" + _expectedPresence + ")",CheckResult.Success, null));
            else
            {
                //we did not find what we expected

                //we expected it to be there
                if(_expectedPresence)
                    NotifyFail(null,notifier,trigger); //try creating it
                else
                    //we did not expect it to be there but it was, just fail and don't offer anything crazy like nuking it
                    notifier.OnCheckPerformed(new CheckEventArgs("Trigger presence/intactness for table " + _tableName + " did not match expected presence (" + _expectedPresence + ")", CheckResult.Fail, null));
                
            }
        }


        private bool CheckColumnOrderInTablesAndArchiveMatch(string[] liveCols, string[] archiveCols, ICheckNotifier notifier)
        {
            bool passed = true;

            foreach (var requiredArchiveColumns in new[] { "hic_validTo", "hic_userID", "hic_status" })
                if (!archiveCols.Contains(requiredArchiveColumns))
                {

                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Column " + requiredArchiveColumns + " was not found in " + _tableName + "_Archive",
                            CheckResult.Fail));
                    passed = false;
                }


            foreach (var missingColumn in liveCols.Except(archiveCols))
            {

                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Column " + missingColumn + " in table " + _tableName +
                    " was not found in the  _Archive table",
                    CheckResult.Fail, null));
                passed = false;
            }

            return passed;
        }
        private void NotifyFail(Exception e, ICheckNotifier notifier,TriggerImplementer trigger)
        {
            //if they expected it not to be there then it shouldn't be crashing trying to find it
            if (!_expectedPresence)
                throw e;

            bool shouldCreate = notifier.OnCheckPerformed(new CheckEventArgs(
                    "Trigger error encountered when checking integrity of table " + _tableName,
                    CheckResult.Warning, e, "Drop and then Re-Create Trigger on table " + _tableName));

            if (shouldCreate)
            {
                string problemsDroppingTrigger;
                string thingsThatWorkedDroppingTrigger;

                trigger.DropTrigger(out problemsDroppingTrigger,out thingsThatWorkedDroppingTrigger);

                if (!string.IsNullOrWhiteSpace(thingsThatWorkedDroppingTrigger))
                    notifier.OnCheckPerformed(new CheckEventArgs(thingsThatWorkedDroppingTrigger, CheckResult.Success, null));

                if (!string.IsNullOrWhiteSpace(problemsDroppingTrigger))
                    notifier.OnCheckPerformed(new CheckEventArgs(problemsDroppingTrigger, CheckResult.Warning, null));

                trigger.CreateTrigger(_primaryKeys,notifier);
                TriggerCreated = true;
            }
        }

        
    }
}
