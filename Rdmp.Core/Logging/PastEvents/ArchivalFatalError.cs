// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Logging.PastEvents;

/// <summary>
///     Readonly audit of a historical error which resulted in the failure of the logged activity (which is also a past /
///     readonly event).
/// </summary>
public class ArchivalFatalError : IArchivalLoggingRecordOfPastEvent, IHasSummary
{
    public int ID { get; }
    public DateTime Date { get; internal set; }
    public string Source { get; internal set; }
    public string Description { get; internal set; }
    public string Explanation { get; set; }

    public ArchivalFatalError(DbDataReader r)
    {
        ID = Convert.ToInt32(r["ID"]);
        Date = Convert.ToDateTime(r["time"]);
        Source = r["source"] as string;
        Description = r["description"] as string;
        Explanation = r["explanation"] as string;
    }

    public string ToShortString()
    {
        var s = ToString();
        return s.Length > ArchivalDataLoadInfo.MaxDescriptionLength
            ? $"{s[..ArchivalDataLoadInfo.MaxDescriptionLength]}..."
            : s;
    }

    public override string ToString()
    {
        return
            $"{Source} - {Description}{(string.IsNullOrWhiteSpace(Explanation) ? "(UNRESOLVED)" : $"(RESOLVED:{Explanation})")}";
    }

    public int CompareTo(object obj)
    {
        if (obj is ArchivalFatalError other)
            return Date == other.Date ? 0 : Date > other.Date ? 1 : -1;

        return string.Compare(ToString(), obj.ToString(), StringComparison.Ordinal);
    }

    public void GetSummary(out string title, out string body, out string stackTrace, out CheckResult level)
    {
        title = $"{Source} ({Date})";
        body = Description;
        stackTrace = null;
        level = CheckResult.Fail;
    }
}