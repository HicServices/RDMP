// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
#nullable enable

using System.Collections.Generic;

namespace Rdmp.Core.Datasets.PureItems;
/// <summary>
/// Internal PURE system class
/// </summary>
public class PurePerson
{
    public string? TypeDiscriminator { get; set; }
    public int? PureId { get; set; }

    public Name? Name { get; set; }
    public URITerm? Role { get; set; }

    public List<PureSystem>? Organizations { get; set; }
}