// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction.Listeners;

/// <summary>
///     A simple DataLoadEventListener to be used during extraction so that the state can be moved to "crashed" if any
///     component raises an error without an exception.
/// </summary>
public class ElevateStateListener : IDataLoadEventListener
{
    private readonly ExtractCommand extractCommand;

    public ElevateStateListener(ExtractCommand extractCommand)
    {
        this.extractCommand = extractCommand;
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        if (e.ProgressEventType == ProgressEventType.Error && extractCommand != null)
            extractCommand.ElevateState(ExtractCommandState.Crashed);
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
    }
}