using System;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Checks
{
    public class GlobalsReleaseChecker:ICheckable
    {
        private readonly IExtractionConfiguration[] _configurations;

        public GlobalsReleaseChecker(IExtractionConfiguration[] configurations)
        {
            _configurations = configurations;
        }

        public void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Globals might not exist yet, not sure", CheckResult.Fail,new NotImplementedException()));
        }
    }
}