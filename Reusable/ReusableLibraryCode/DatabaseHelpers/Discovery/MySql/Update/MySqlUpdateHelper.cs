using System;
using System.Collections.Generic;
using System.Linq;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Update;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql.Update
{
    public class MySqlUpdateHelper : UpdateHelper
    {
        protected override string BuildUpdateImpl(DiscoveredTable table1, DiscoveredTable table2, List<CustomLine> lines)
        {
            return string.Format(
@"UPDATE {1} t1
 join  {2} t2 
on 
{3}
SET 
    {0}
WHERE
{4}",
    string.Join(", " + Environment.NewLine ,lines.Where(l=>l.LocationToInsert == QueryComponent.SET).Select(c => c.Text)),
    table1.GetFullyQualifiedName(),
    table2.GetFullyQualifiedName(),
    string.Join(" AND ", lines.Where(l => l.LocationToInsert == QueryComponent.JoinInfoJoin).Select(c => c.Text)),
    string.Join(" AND ", lines.Where(l => l.LocationToInsert == QueryComponent.WHERE).Select(c => c.Text)));

        }
    }
}