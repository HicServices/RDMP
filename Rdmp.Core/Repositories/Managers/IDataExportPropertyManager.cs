// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
///     Handles persistence of <see cref="DataExportProperty" /> settings (e.g. what is the hashing algorithmn)
/// </summary>
public interface IDataExportPropertyManager
{
    /// <summary>
    ///     Returns the currently saved value for the given <paramref name="property" /> or null if it's not been set yet
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    string GetValue(DataExportProperty property);

    /// <summary>
    ///     Stores a new <paramref name="value" /> for the given <paramref name="property" /> (and saves to the database)
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    void SetValue(DataExportProperty property, string value);
}