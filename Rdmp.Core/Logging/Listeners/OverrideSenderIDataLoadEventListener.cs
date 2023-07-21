// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Logging.Listeners;

/// <summary>
///     Acts as wrapper for another <see cref="IDataLoadEventListener" /> but changes all messages that flow through to
///     appear to come from the same
///     sender (string).  You can use this to help with distinguishing message dispatchers (senders) between discrete tasks
///     / threads.
/// </summary>
public class OverrideSenderIDataLoadEventListener : IDataLoadEventListener
{
    private readonly string _overridingSender;
    private readonly IDataLoadEventListener _child;

    public OverrideSenderIDataLoadEventListener(string overridingSender, IDataLoadEventListener childToPassTo)
    {
        _overridingSender = overridingSender;
        _child = childToPassTo;
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        _child.OnNotify(_overridingSender, e);
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
        _child.OnProgress(_overridingSender, e);
    }
}