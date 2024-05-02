// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.ReusableLibraryCode.Performance;

/// <summary>
///     Stores the location (Stack Trace) of all calls to the database (DbCommands constructed).  This does not include how
///     long they took to run or even the
///     final state of the command (which could have parameters or have its command text modified after construction).
///     Mostly it is useful for detecting
///     lines of code that are sending hundreds/thousands of duplicate queries.
///     <para>
///         You can install a ComprehensiveQueryPerformanceCounter by using DatabaseCommandHelper.PerformanceCounter =
///         new ComprehensiveQueryPerformanceCounter()
///     </para>
///     <para>
///         You can view the results of a ComprehensiveQueryPerformanceCounter by using a
///         PerformanceCounterUI/PerformanceCounterResultsUI.
///     </para>
/// </summary>
public class ComprehensiveQueryPerformanceCounter
{
    public readonly Dictionary<string, QueryPerformed> DictionaryOfQueries = new();

    public void AddAudit(DbCommand cmd, string environmentDotStackTrace)
    {
        //is it a novel origin
        if (!DictionaryOfQueries.ContainsKey(environmentDotStackTrace))
            DictionaryOfQueries.Add(environmentDotStackTrace, new QueryPerformed(cmd.CommandText));

        var query = DictionaryOfQueries[environmentDotStackTrace];
        query.IncrementSeenCount();
    }
}