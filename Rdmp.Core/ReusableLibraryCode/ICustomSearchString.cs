// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.ReusableLibraryCode;

/// <summary>
///     Indicates that ToString alone is insufficient for finding this class and that additional text will be useful for
///     distinguishing this object from others
///     during search operations.
/// </summary>
public interface ICustomSearchString
{
    /// <summary>
    ///     Return the full string that should be used for free text search matching during user driven find operations instead
    ///     of the ToString method.
    /// </summary>
    /// <returns>Search string to match tokens against instead of ToString</returns>
    string GetSearchString();
}