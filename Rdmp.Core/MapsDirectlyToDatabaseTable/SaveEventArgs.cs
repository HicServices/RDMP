// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
///     Event args for the <see cref="IRepository.SaveToDatabase(IMapsDirectlyToDatabaseTable)" /> operation.
///     See also <see cref="IRepository.Saving" />
/// </summary>
public class SaveEventArgs : EventArgs
{
    /// <summary>
    ///     Set to true to prevent the save writing to database/disk.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    ///     The object that is about to be saved to database/disk
    /// </summary>
    public IMapsDirectlyToDatabaseTable BeingSaved { get; }

    public SaveEventArgs(IMapsDirectlyToDatabaseTable o)
    {
        BeingSaved = o;
    }
}