// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.ReusableLibraryCode;

/// <summary>
///     Interface for classes that are able to summarise themselves for display to a user
/// </summary>
public interface IHasSummary
{
    /// <summary>
    ///     Gets a user friendly summary of the objects current state
    /// </summary>
    /// <param name="title">Short description (should not have newlines)</param>
    /// <param name="body">Long description which can contain newlines</param>
    /// <param name="stackTrace">Optional stack trace where error occurred (set to null if not applicable)</param>
    /// <param name="level">How servere the situation is/was</param>
    void GetSummary(out string title, out string body, out string stackTrace, out CheckResult level);
}