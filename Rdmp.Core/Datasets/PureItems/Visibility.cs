// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
#nullable enable

namespace Rdmp.Core.Datasets.PureItems;
/// <summary>
/// Internal PURE system class
/// </summary>
public class Visibility
{
    public Visibility(string? key, ENGBWrapper description)
    {
        Key = key;
        Description = description;
    }
    public string? Key { get; set; }
    public ENGBWrapper Description { get; set; }
}