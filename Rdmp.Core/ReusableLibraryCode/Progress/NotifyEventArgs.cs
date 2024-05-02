// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
///     Event args for IDataLoadEventListener.OnNotify events.  Includes the StackTrace the message was raised from, the
///     ProgressEventType (Error, Warning etc) and
///     Any Exception.
/// </summary>
public class NotifyEventArgs
{
    public ProgressEventType ProgressEventType { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
    public bool Handled { get; set; }

    public string StackTrace { get; private set; }

    public NotifyEventArgs(ProgressEventType progressEventType, string message)
    {
        ProgressEventType = progressEventType;
        Message = message;
        Handled = false;
        try
        {
            StackTrace = Environment.StackTrace;
        }
        catch (Exception)
        {
            //Stack trace not available ah well
        }
    }

    public NotifyEventArgs(ProgressEventType progressEventType, string message, Exception exception)
    {
        ProgressEventType = progressEventType;
        Message = message;
        Exception = exception;
        Handled = false;

        try
        {
            StackTrace = Environment.StackTrace;
        }
        catch (Exception)
        {
            //Stack trace not available ah well
        }
    }

    public CheckEventArgs ToCheckEventArgs()
    {
        var result = ProgressEventType switch
        {
            ProgressEventType.Trace => CheckResult.Success,
            ProgressEventType.Debug => CheckResult.Success,
            ProgressEventType.Information => CheckResult.Success,
            ProgressEventType.Warning => CheckResult.Warning,
            ProgressEventType.Error => CheckResult.Fail,
            _ => throw new ArgumentOutOfRangeException()
        };
        return new CheckEventArgs(Message, result, Exception);
    }
}