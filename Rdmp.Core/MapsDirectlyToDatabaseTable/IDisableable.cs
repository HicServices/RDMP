// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Class supports a disabled state which should be consulted before running the component
    /// e.g. in a data load.
    /// </summary>
    public interface IDisableable: IRevertable
    {
        /// <summary>
        /// True to skip the component when executing (but still show it at design time).
        /// </summary>
        bool IsDisabled { get; set; }
    }
}