// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using System.Collections.Generic;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
/// Allows you to route IDataLoadEventListener messages to an ICheckNotifier.  All OnNotify events will be translated into a similar OnCheckPerformed event.
/// For OnProgress events there is a loss of granularity since ICheckNotifiers are not designed to record incremental progress messages.  Therefore the
/// ICheckNotifier will only be given one OnCheckPerformed event for each novel OnProgress Tasks seen with the message 'Started progress on x'.
/// </summary>
public class FromCheckNotifierToDataLoadEventListener : IDataLoadEventListener
{
    private readonly ICheckNotifier _checker;

    public FromCheckNotifierToDataLoadEventListener(ICheckNotifier checker)
    {
        _checker = checker;
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        _checker.OnCheckPerformed(e.ToCheckEventArgs());
    }

    private readonly HashSet<string> _progressMessagesReceived = new();

    public void OnProgress(object sender, ProgressEventArgs e)
    {
        //only tell the user once about each progress message because these can come 100 a second and there's no finished flag so all we can do is tell them it is happening
        if (_progressMessagesReceived.Add(e.TaskDescription))
            _checker.OnCheckPerformed(new CheckEventArgs($"Started progress on {e.TaskDescription}",
                CheckResult.Success));
    }
}