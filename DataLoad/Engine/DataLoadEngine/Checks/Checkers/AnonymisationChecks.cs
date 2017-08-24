using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.Checks.Checkers
{
    public class AnonymisationChecks : ICheckable
    {
        private readonly TableInfo _tableInfo;

        public AnonymisationChecks(TableInfo tableInfo)
        {
            _tableInfo = tableInfo;
        }

        

        public void Check(ICheckNotifier notifier)
        {
            //check ANO stuff is synchronized
            notifier.OnCheckPerformed(new CheckEventArgs("Preparing to synchronize ANO configuration", CheckResult.Success, null));

            ANOTableInfoSynchronizer synchronizer = new ANOTableInfoSynchronizer(_tableInfo);

            try
            {
                synchronizer.Synchronize(notifier);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Synchronization of Anonymsiation configurations of table " + _tableInfo.GetRuntimeName() + " failed with Exception", CheckResult.Fail, e, null));
            }

            if(_tableInfo.IdentifierDumpServer_ID != null)
            {
                var identifierDumper = new IdentifierDumper(_tableInfo);
                identifierDumper.Check(notifier);
            }
        }
    }
}
