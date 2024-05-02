// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


namespace Rdmp.Core.ReusableLibraryCode.Checks;

//important to keep these in order of severity from least sever to most severe so > operations can be applied to Enum
public enum CheckResult
{
    Success,
    Warning,
    Fail
}

/// <summary>
///     An object that can check its own state for problems and summarise this through the Checking Events system (See
///     CheckEventArgs)
/// </summary>
public interface ICheckable
{
    /// <summary>
    ///     Use the OnCheckPerformed method on the notifier to inform of all the things you are checking and the results,
    ///     possible fixes and severity
    /// </summary>
    /// <param name="notifier">
    ///     The manager that will receive your messages about problems/fixes and decide how/if to present
    ///     them to the user
    /// </param>
    void Check(ICheckNotifier notifier);
}