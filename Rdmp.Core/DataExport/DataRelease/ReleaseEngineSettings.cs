// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.DataRelease;

/// <summary>
///     Options for configuring ReleaseEngine behaviour (To change where files are released to etc)
/// </summary>
public class ReleaseEngineSettings : ICheckable
{
    [DemandsInitialization("Delete the released files from the origin location if release is succesful",
        DefaultValue = true)]
    public bool DeleteFilesOnSuccess { get; set; }

    public ReleaseEngineSettings()
    {
        DeleteFilesOnSuccess = true;
    }

    public void Check(ICheckNotifier notifier)
    {
        notifier.OnCheckPerformed(new CheckEventArgs("All green!", CheckResult.Success));
    }
}