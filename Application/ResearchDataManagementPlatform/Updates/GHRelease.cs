// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace ResearchDataManagementPlatform.Updates;

/// <summary>
/// Github Json class
/// </summary>
public class GHRelease
{
    public string url { get; set; }
    public string assets_url { get; set; }
    public string upload_url { get; set; }
    public string html_url { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string tag_name { get; set; }
    public string target_commitish { get; set; }
    public string name { get; set; }
    public bool draft { get; set; }
    public Author author { get; set; }
    public bool prerelease { get; set; }
    public DateTime created_at { get; set; }
    public DateTime published_at { get; set; }
    public Asset[] assets { get; set; }
    public string tarball_url { get; set; }
    public string zipball_url { get; set; }
    public string body { get; set; }
}