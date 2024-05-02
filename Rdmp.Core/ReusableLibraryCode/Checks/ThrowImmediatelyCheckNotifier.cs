// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.ReusableLibraryCode.Checks;

/// <summary>
///     ICheckNotifier which converts failed CheckEventArgs into Exceptions.  Can optionally also throw on Warning
///     messages.  By default all messages are written
///     to the Console.  The use case for this is any time you want to run Checks programmatically (i.e. without user
///     intervention via a UI component) before running
///     and you don't expect any Checks to fail but want to make sure.  Or when you are in a Test and you want to make sure
///     that a specific configuration bombs
///     when Checked with an appropriate failure message.
/// </summary>
public class ThrowImmediatelyCheckNotifier : ICheckNotifier
{
    public static readonly ThrowImmediatelyCheckNotifier Quiet = new(false, false);
    public static readonly ThrowImmediatelyCheckNotifier Noisy = new(true, false);
    public static readonly ThrowImmediatelyCheckNotifier QuietPicky = new(false, true);
    public static readonly ThrowImmediatelyCheckNotifier NoisyPicky = new(true, true);

    private ThrowImmediatelyCheckNotifier(bool write, bool picky)
    {
        WriteToConsole = write;
        ThrowOnWarning = picky;
    }

    [Obsolete("Use the singleton Luke")]
    public ThrowImmediatelyCheckNotifier()
    {
    }

    public virtual bool OnCheckPerformed(CheckEventArgs args)
    {
        if (WriteToConsole)
            Console.WriteLine(args.Message);

        return args.Result switch
        {
            CheckResult.Fail => throw new Exception(args.Message, args.Ex),
            CheckResult.Warning when ThrowOnWarning => throw new Exception(args.Message, args.Ex),
            _ => false
            //do not apply fixes to warnings/success
        };
    }

    /// <summary>
    ///     By default this class will only throw Fail results but if you set this flag then it will also throw warning
    ///     messages
    /// </summary>
    public readonly bool ThrowOnWarning;

    /// <summary>
    ///     By default this class will not log to the Console. Set to true to get a console flood
    /// </summary>
    public readonly bool WriteToConsole;
}