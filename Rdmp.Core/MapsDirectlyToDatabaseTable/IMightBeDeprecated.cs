// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
///     Interface for database object classes with a deprecated status flag
/// </summary>
public interface IMightBeDeprecated : IRevertable
{
    /// <summary>
    ///     Bit flag indicating whether the object should be considered Deprecated (i.e. do not use anymore).  This is
    ///     preferred to deleting it.  The implications
    ///     of this are that it no longer appears in UIs by default and that warnings may appear when trying to interact with
    ///     it.
    /// </summary>
    bool IsDeprecated { get; set; }
}