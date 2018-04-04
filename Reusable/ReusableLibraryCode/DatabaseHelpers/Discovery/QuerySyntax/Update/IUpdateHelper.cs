using System;
using System.Collections.Generic;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Update
{
    public delegate string UpdateStatementSqlGetter(string table2Alias);

    /// <summary>
    /// Cross Database Type class for turning a collection of arbitrary sql lines (CustomLine) into an UPDATE query where no suitable ANSI solution exists.  For example 
    /// updating a table using a join to another table where the relationship is n..n.
    /// 
    /// <para>Look at UpdateHelper.permissableLocations to determine which CustomLines you are allowed to pass in.</para>
    /// </summary>
    public interface IUpdateHelper
    {
        /// <summary>
        /// Joins the two tables (which could be a self join) and updates table1 using values computed from table2.  The join will be a straight up join i.e. not left/right/inner.
        /// IMPORTANT: All CustomLines should use the table aliases t1 and t2 e.g. t1.MyCol = T2.MyCol
        ///
        /// </summary>
        /// <param name="table1">The table to UPDATE</param>
        /// <param name="table2">The table to join against (which might be a self join)</param>
        /// <param name="lines">All SET, WHERE and JoinInfo lines needed to build the query, columns should be specified using the alias t1.colX and t2.colY instead of the full names of
        ///  table1/table2.  if you have multiple WHERE lines then they will be ANDed.  To avoid this you can concatenate your CustomLines together yourself and serve only one to this 
        /// method(e.g. to use OR) </param>
        /// <returns></returns>
        string BuildUpdate(DiscoveredTable table1, DiscoveredTable table2,List<CustomLine> lines);

    }
}
