// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
///     IDataLoadEventListener that ignores all OnProgress messages but responds to OnNotify events of
///     ProgressEventType.Error (and optionally Warning) by
///     raising an Exception.  Use this if you need an IDataLoadEventListener and don't care about the messages it sends
///     (unless they are errors).
/// </summary>
public sealed class ThrowImmediatelyDataLoadEventListener : IDataLoadEventListener
{
    /// <summary>
    ///     By default this class will only throw Fail results but if you set this flag then it will also throw warning
    ///     messages
    /// </summary>
    public bool ThrowOnWarning { get; init; }

    public bool WriteToConsole { get; init; }

    public static readonly ThrowImmediatelyDataLoadEventListener Quiet = new(false, false);
    public static readonly ThrowImmediatelyDataLoadEventListener Noisy = new(true, false);
    public static readonly ThrowImmediatelyDataLoadEventListener QuietPicky = new(false, true);
    public static readonly ThrowImmediatelyDataLoadEventListener NoisyPicky = new(true, true);

    private ThrowImmediatelyDataLoadEventListener(bool write, bool picky)
    {
        WriteToConsole = write;
        ThrowOnWarning = picky;
    }

    [Obsolete("Use the singleton Luke")]
    public ThrowImmediatelyDataLoadEventListener()
    {
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        if (WriteToConsole)
            Console.WriteLine($"{sender}:{e.Message}");

        if (e.ProgressEventType == ProgressEventType.Error ||
            (e.ProgressEventType == ProgressEventType.Warning && ThrowOnWarning))
            throw new Exception(e.Message, e.Exception);
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
    }
}