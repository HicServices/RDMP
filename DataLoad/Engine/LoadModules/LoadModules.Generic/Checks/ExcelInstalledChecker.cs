// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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