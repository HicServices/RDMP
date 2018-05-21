using System;
using System.Linq;
using CachingEngine.DataRetrievers;
using CatalogueLibrary.Triggers;
using CatalogueLibrary.Triggers.Exceptions;
using CatalogueLibrary.Triggers.Implementations;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Checks.Checkers
{
    class TriggerChecks : ICheckable
    {
        private readonly bool _expectedPresence;
        
        private DiscoveredTable _table;
        private DiscoveredTable _archiveTable;
        private DiscoveredServer _server;

        public bool TriggerCreated { get; private set; }

        public TriggerChecks(DiscoveredTable table, bool expectedPresence)
        {
            _server = table.Database.Server;
            _table = table;
            _archiveTable = table.Database.ExpectTable(_table.GetRuntimeName() + "_Archive");
            _expectedPresence = expectedPresence;
        }


        public void Check(ICheckNotifier notifier)
        {
            if (_table.Exists() && _archiveTable.Exists())
            {
                string[] liveCols = _table.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();
                string[] archiveCols = _archiveTable.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();

                var passed = CheckColumnOrderInTablesAndArchiveMatch(liveCols, archiveCols, notifier);

                if(!passed)
                    return;
            }

            var factory = new TriggerImplementerFactory(_server.DatabaseType);
            var implementer = factory.Create(_table);

            bool present;

            var primaryKeys = _table.DiscoverColumns().Where(c => c.IsPrimaryKey).ToArray();

            //we don't know the primary keys
            if (!primaryKeys.Any())
                try
                {
                    //see if it exists
                    present = implementer.GetTriggerStatus() == TriggerStatus.Enabled;
                }
                catch (TriggerMissingException)
                {
                    //clearly it doesnt exist
                    present = false;
                }
            else
            {
                try
                {
                    //we do know the primary keys
                    present = implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody();
                }
                catch (IrreconcilableColumnDifferencesInArchiveException e)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Archive table for table " + _table +
                            " is corrupt, see inner Exception for specific errors", CheckResult.Fail, e));
                    return;
                }
                catch (Exception e)
                {
                    NotifyFail(e, notifier, implementer);
                    return;
                }
            }

            //we expected it to be missing and it was or we expected it to be enabled and it was
            if(present == _expectedPresence)
                notifier.OnCheckPerformed(new CheckEventArgs("Trigger presence/intactness for table " + _table + " matched expected presence (" + _expectedPresence + ")",CheckResult.Success, null));
            else
            {
                //we did not find what we expected

                //we expected it to be there
                if(_expectedPresence)
                    NotifyFail(null, notifier, implementer); //try creating it
                else
                    //we did not expect it to be there but it was, just fail and don't offer anything crazy like nuking it
                    notifier.OnCheckPerformed(new CheckEventArgs("Trigger presence/intactness for table " + _table + " did not match expected presence (" + _expectedPresence + ")", CheckResult.Fail, null));
                
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
                            "Column " + requiredArchiveColumns + " was not found in " + _archiveTable,
                            CheckResult.Fail));
                    passed = false;
                }


            foreach (var missingColumn in liveCols.Except(archiveCols))
            {

                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Column " + missingColumn + " in table " + _table +
                    " was not found in the  _Archive table",
                    CheckResult.Fail, null));
                passed = false;
            }

            return passed;
        }

        private void NotifyFail(Exception e, ICheckNotifier notifier,ITriggerImplementer microsoftSQLTrigger)
        {
            //if they expected it not to be there then it shouldn't be crashing trying to find it
            if (!_expectedPresence)
                throw e;

            bool shouldCreate = notifier.OnCheckPerformed(new CheckEventArgs(
                    "Trigger error encountered when checking integrity of table " + _table,
                    CheckResult.Warning, e, "Drop and then Re-Create Trigger on table " + _table));

            if (shouldCreate)
            {
                string problemsDroppingTrigger;
                string thingsThatWorkedDroppingTrigger;

                microsoftSQLTrigger.DropTrigger(out problemsDroppingTrigger,out thingsThatWorkedDroppingTrigger);

                if (!string.IsNullOrWhiteSpace(thingsThatWorkedDroppingTrigger))
                    notifier.OnCheckPerformed(new CheckEventArgs(thingsThatWorkedDroppingTrigger, CheckResult.Success, null));

                if (!string.IsNullOrWhiteSpace(problemsDroppingTrigger))
                    notifier.OnCheckPerformed(new CheckEventArgs(problemsDroppingTrigger, CheckResult.Warning, null));

                microsoftSQLTrigger.CreateTrigger(notifier);
                TriggerCreated = true;
            }
        }

        
    }
}
