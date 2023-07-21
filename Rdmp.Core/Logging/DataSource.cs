// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Logging;

/// <summary>
///     A conceptual 'source' which contributed records to a table being loaded during a logged activity (See
///     TableLoadInfo).  This can be as explicit
///     as a flat file 'myfile.csv' or as isoteric as an sql query run on a server (e.g. during extraction we audit the
///     extraction sql with one of these).
/// </summary>
public class DataSource
{
    public DataSource(string source, DateTime originDate)
    {
        Source = source;
        OriginDate = originDate;
        UnknownOriginDate = false;
    }

    public DataSource(string source)
    {
        Source = source;
        UnknownOriginDate = true;
    }

    public int ID { get; internal set; }
    public string Source { get; set; }
    public string Archive { get; set; }
    public DateTime OriginDate { get; internal set; }
    public bool UnknownOriginDate { get; internal set; }

    public byte[] MD5 { get; set; }
}