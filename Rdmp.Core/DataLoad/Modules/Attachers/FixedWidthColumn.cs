// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
///     Describes a fixed width column in a fixed width file being loaded by the RDMP DLE
/// </summary>
public struct FixedWidthColumn
{
    //order of these must match the order in the flat file!
    /// <summary>
    ///     The character index at which the column start.  This is inclusive and starts at 1.
    /// </summary>
    public int From;

    /// <summary>
    ///     The character index at which the column ends.  This is inclusive and starts at 1.  If <see cref="From" /> and
    ///     <see cref="To" />
    ///     are the same number then a 1 character is allotted for that column
    /// </summary>
    public int To;

    /// <summary>
    ///     The column name to ascribe to the column
    /// </summary>
    public string Field;

    /// <summary>
    ///     The width (should be <see cref="To" /> - <see cref="From" />) + 1.  This exists for validation purposes and is
    ///     usually provided by
    ///     the user.  It helps prevent errors in reading
    /// </summary>
    public int Size;

    /// <summary>
    ///     The format to use when reading dates from the column (if any)
    /// </summary>
    public string DateFormat;
}