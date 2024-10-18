// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Datasets.PureItems;
using System;
#nullable enable

namespace Rdmp.Core.Datasets;

/// <summary>
/// Used to mapping Pure publishers from the API into a C# Object.
/// This is not indended for modification within RDMP.
/// </summary>
public class PurePublisher
{
    public int? pureId { get; set; }
    public string? uuid { get; set; }
    public string? createdBy { get; set; }
    public DateTime? createdDate { get; set; }
    public string? modifiedBy { get; set; }
    public DateTime? modifiedDate { get; set; }
    public string? version { get; set; }
    public string? name { get; set; }
    public URITerm? type { get; set; }
    public Workflow? workflow { get; set; }
    public string? systemName { get; set; }

    public PurePublisher() { }
}