// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.ReusableLibraryCode.Checks;

/// <summary>
///     ICheckNotifier which accepts a;; ProposedFixes automatically and throws Exceptions on Fail messages (if there
///     wasn't a ProposedFix)
/// </summary>
public class AcceptAllCheckNotifier : ICheckNotifier
{
    /// <summary>
    ///     True to write out all messages seen directly to the console
    /// </summary>
    public bool WriteToConsole { get; init; }

    /// <summary>
    ///     Check handler that throws <see cref="Exception" /> on Failures but otherwise returns true
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public virtual bool OnCheckPerformed(CheckEventArgs args)
    {
        if (WriteToConsole)
            Console.WriteLine($"{args.Result}:{args.Message}");

        //if there is a proposed fix then accept it regardless of whether it was a Fail.
        if (!string.IsNullOrWhiteSpace(args.ProposedFix))
            return true;

        return args.Result == CheckResult.Fail
            ? throw new Exception($"Failed check with message: {args.Message}", args.Ex)
            : true;
    }
}