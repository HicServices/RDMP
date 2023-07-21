// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NLog;
using Rdmp.Core.Logging.Listeners.Extensions;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Logging.Listeners.NLogListeners;

/// <summary>
///     <see cref="IDataLoadEventListener" /> that passes all events to an <see cref="NLog.LogManager" />.  Optionally
///     throws on Errors (after logging).
/// </summary>
public class NLogIDataLoadEventListener : NLogListener, IDataLoadEventListener
{
    public NLogIDataLoadEventListener(bool throwOnError) : base(throwOnError)
    {
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        Log(sender, e.ToLogLevel(), e.Exception, e.Message);
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
        Log(sender, LogLevel.Trace, null,
            $"Progress: {e.Progress.Value} {e.Progress.UnitOfMeasurement}{(e.Progress.KnownTargetValue == 0 ? "" : $" of {e.Progress.KnownTargetValue}")}"
        );
    }
}