using System;
using System.Linq;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Checks
{
    /// <summary>
    /// Checks the release state of the Globals that should have been extracted as part of the given <see cref="ExtractionConfiguration"/>.  If they
    /// are missing then the overall release should not be run.
    /// </summary>
    public class GlobalsReleaseChecker:ICheckable
    {
        private readonly IExtractionConfiguration[] _configurations;

        public GlobalsReleaseChecker(IExtractionConfiguration[] configurations)
        {
            _configurations = configurations;
        }

        public void Check(ICheckNotifier notifier)
        {
            if(_configurations.Any(c=>c.GetGlobals().Any()))
                notifier.OnCheckPerformed(new CheckEventArgs("Globals might not exist yet, not sure", CheckResult.Warning,new NotImplementedException()));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("There are no globals", CheckResult.Success));
            
        }
    }
}