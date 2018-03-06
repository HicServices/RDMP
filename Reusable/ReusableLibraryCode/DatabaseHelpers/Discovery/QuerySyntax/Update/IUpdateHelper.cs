using System;
using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Update
{
    public delegate string UpdateStatementSqlGetter(string table2Alias);

    public interface IUpdateHelper
    {
        /// <summary>
        /// Joins the two tables (which could be a self join) and updates table1 using values computed from table2
        /// </summary>
        /// <param name="table1">The table to UPDATE</param>
        /// <param name="table2">The table to join against (which might be a self join)</param>
        /// <param name="lines">All SET, WHERE and JoinInfo lines needed to build the query, columns should be specified using the alias t1.colX and t2.colY instead of the full names of table1/table2</param>
        /// <returns></returns>
        string BuildUpdate(DiscoveredTable table1, DiscoveredTable table2,List<CustomLine> lines);

    }
}