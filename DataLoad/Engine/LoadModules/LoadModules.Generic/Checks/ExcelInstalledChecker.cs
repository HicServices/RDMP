using System;
using System.Diagnostics;
using CatalogueLibrary.Reports;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace LoadModules.Generic.Checks
{
    /// <summary>
    /// Checker for ensuring that the current PC has Microsoft Excel installed.  This is needed for some file reading operations which rely on Interop for reading.
    /// </summary>
    public class ExcelInstalledChecker:ICheckable
    {
        public void Check(ICheckNotifier notifier)
        {
            FileVersionInfo version = OfficeVersionFinder.GetVersion(OfficeVersionFinder.OfficeComponent.Excel);
            if (version != null)
                notifier.OnCheckPerformed(new CheckEventArgs("Found Excel:" + Environment.NewLine + OfficeVersionFinder.GetVersion(OfficeVersionFinder.OfficeComponent.Excel), CheckResult.Success));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Could not find installed Microsoft Excel application", CheckResult.Fail));
        }
    }
}