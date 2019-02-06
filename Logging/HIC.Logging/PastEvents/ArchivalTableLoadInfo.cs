// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace HIC.Logging.PastEvents
{
    /// <summary>
    /// Readonly audit of a table that was loaded as part of a historical data load (See HIC.Logging.ArchivalDataLoadInfo).
    /// </summary>
    public class ArchivalTableLoadInfo : IArchivalLoggingRecordOfPastEvent, IComparable
    {
        public ArchivalDataLoadInfo Parent { get; private set; }

        public ArchivalTableLoadInfo(ArchivalDataLoadInfo parent, int id, DateTime start, object end, string targetTable, object inserts, object updates, object deletes, string notes)
        {
            Parent = parent;
            ID = id;
            Start = start;

            if (end == null || end == DBNull.Value)
                End = null;
            else
                End = (DateTime) end;

            TargetTable = targetTable;


            Inserts = ToNullableInt(inserts);
            Updates = ToNullableInt(updates);
            Deletes = ToNullableInt(deletes);
            Notes = notes;

            DataSources = new List<ArchivalDataSource>();
        }

        private int? ToNullableInt(object i)
        {
            if (i == null || i == DBNull.Value)
                return null;

            return Convert.ToInt32(i);

        }

        public int ID { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime? End { get; internal set; }
        public string TargetTable { get; internal set; }
        public int? Inserts { get; internal set; }
        public int? Deletes { get; internal set; }
        public int? Updates { get; internal set; }
        public string Notes { get; internal set; }

        public List<ArchivalDataSource> DataSources { get; internal set; }
        
        public string ToShortString()
        {

            var s = ToString();
            if (s.Length > ArchivalDataLoadInfo.MaxDescriptionLength)
                return s.Substring(0, ArchivalDataLoadInfo.MaxDescriptionLength) + "...";
            return s;
        }

        public override string ToString()
        {
            return Start + " - " + TargetTable + " (Inserts=" + Inserts + ",Updates=" + Updates + ",Deletes=" + Deletes +")";
        }

        public int CompareTo(object obj)
        {
            var other = obj as ArchivalTableLoadInfo;
            if (other != null)
                if (Start == other.Start)
                    return 0;
                else
                    return Start > other.Start ? 1 : -1;

            return System.String.Compare(ToString(), obj.ToString(), System.StringComparison.Ordinal);
        }
    }
}