// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.ReusableLibraryCode.Checks;

/// <summary>
/// Describes an error that can surface during RDMPs execution as a result of state e.g. user is trying to extract to a db but the
/// destination table already exists.
/// </summary>
public sealed class ErrorCode
{
    /// <summary>
    /// A fixed code for uniquely identifying this error type
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The string to display to the user in the event that the error occurs.  This is likely to contain one or more
    /// placeholders e.g. {0} that will be populated with relevant info at the time it occurs (e.g. table names)
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// The severity level at which the error is reported.  Can be overridden by <see cref="UserSettings"/>
    /// </summary>
    public CheckResult DefaultTreatment { get; }

    public ErrorCode(string code, string message, CheckResult defaultTreatment)
    {
        Code = code;
        Message = message;
        DefaultTreatment = defaultTreatment;
    }

    public override string ToString() => Code;
}