// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Logging.Listeners;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
/// IDataLoadEventListener which routes events to multiple other IDataLoadEventListeners.  Listeners are called in the order they appear in the array
/// therefore if you get an error (e.g. OnNotify event with ProgressEventType.Error) and the first listener decides to respond by raising it as an
/// Exception then the second listener will not get called (since at this point you will have entered Exception handling).
/// </summary>
public class ForkDataLoadEventListener : IDataLoadEventListener
{
    private readonly IDataLoadEventListener[] _listeners;

    public ForkDataLoadEventListener(params IDataLoadEventListener[] listeners)
    {
        _listeners = listeners;
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        foreach (var listener in _listeners)
            listener.OnNotify(sender, e);
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
        foreach (var listener in _listeners)
            listener.OnProgress(sender, e);
    }


    public List<ToLoggingDatabaseDataLoadEventListener> GetToLoggingDatabaseDataLoadEventListenersIfany()
    {
        return _listeners.Where(l => l.GetType() == typeof(ToLoggingDatabaseDataLoadEventListener)).Select(l => (ToLoggingDatabaseDataLoadEventListener)l).ToList();
    }
}