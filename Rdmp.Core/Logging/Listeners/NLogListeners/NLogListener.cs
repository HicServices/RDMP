// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NLog;

namespace Rdmp.Core.Logging.Listeners.NLogListeners;

/// <summary>
///     Base class for all RDMP Listeners (e.g. <see cref="ReusableLibraryCode.Checks.ICheckNotifier" />) which handle
///     events by writing to an NLog.LogManager
/// </summary>
public abstract class NLogListener
{
    public LogLevel Worst { get; set; }
    public bool ThrowOnError { get; set; }

    public NLogListener(bool throwOnError)
    {
        ThrowOnError = throwOnError;
        Worst = LogLevel.Info;
    }

    protected void Log(object sender, LogLevel level, Exception exception, string message)
    {
        if (level > Worst)
            Worst = level;

        NLog.LogManager.GetLogger((sender ?? "Null").ToString()).Log(level, exception, message);

        if (ThrowOnError && level >= LogLevel.Error)
            throw exception ?? new Exception(message);
    }
}