// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.UI.SimpleDialogs;


namespace Rdmp.UI.Progress;

/// <summary>
/// Represents a single <see cref="NotifyEventArgs"/> with supplemental data such as the date and sender.  This is the object
/// shown in <see cref="ProgressUI"/>.
/// </summary>
public class ProgressUIEntry
{
    public string Sender { get; private set; }
    public string Message { get; private set; }
    public ProgressEventType ProgressEventType { get; private set; }
    public DateTime EventDate { get; private set; }

    public NotifyEventArgs Args { get;private set; }
    public Exception Exception { get; set; }

    public ProgressUIEntry(object sender,DateTime eventDate, NotifyEventArgs args)
    {
        Sender = FormatSender(sender);
        Message = args.Message;
        ProgressEventType = args.ProgressEventType;
        EventDate = eventDate;
        Exception = args.Exception;
        Args = args;
    }
        
    private static string FormatSender(object sender)
    {
        if (sender == null)
            return "Unknown";
            
        return sender as string ?? sender.GetType().Name;
    }

    public WideMessageBoxTheme GetTheme()
    {
        switch (ProgressEventType)
        {
            case ProgressEventType.Trace:
                return WideMessageBoxTheme.Help;
            case ProgressEventType.Debug:
                return WideMessageBoxTheme.Help;
            case ProgressEventType.Information:
                return WideMessageBoxTheme.Help;
            case ProgressEventType.Warning:
                return WideMessageBoxTheme.Warning;
            case ProgressEventType.Error:
                return WideMessageBoxTheme.Exception;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}