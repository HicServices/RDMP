// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.DataExport.ExtractionTime.UserPicks
{
    /// <summary>
    /// Identifies a TableInfo that acts as a Lookup for a given dataset which is being extracted.  Lookup tables can be extracted along with the extracted data
    /// set (See ExtractableDatasetBundle).
    /// </summary>
    public class BundledLookupTable : IBundledLookupTable
    {
        public ITableInfo TableInfo { get; set; }

        public BundledLookupTable(ITableInfo tableInfo)
        {
            if(!tableInfo.IsLookupTable())
                throw new Exception("TableInfo " + tableInfo + " is not a lookup table");

            TableInfo = tableInfo;
        }

        public override string ToString()
        {
            return TableInfo.ToString();
        }
    }
}