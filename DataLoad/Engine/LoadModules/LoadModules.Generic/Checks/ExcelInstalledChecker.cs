using System;
using CatalogueLibrary.Reports;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace LoadModules.Generic.Checks
{
    public class ExcelInstalledChecker:ICheckable
    {
        public void Check(ICheckNotifier notifier)
        {
            var requirement = new RequiresMicrosoftOffice();
            if (requirement.RequirementsMet())
                notifier.OnCheckPerformed(new CheckEventArgs("Found Excel:" + Environment.NewLine + OfficeVersionFinder.GetVersion(OfficeVersionFinder.OfficeComponent.Excel), CheckResult.Success));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Could not find installed Microsoft Excel application", CheckResult.Fail));
        }
    }
}