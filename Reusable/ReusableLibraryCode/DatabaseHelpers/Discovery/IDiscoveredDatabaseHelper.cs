using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface IDiscoveredDatabaseHelper
    {

        IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, bool includeViews, DbTransaction transaction = null);
        IEnumerable<DiscoveredTableValuedFunction> ListTableValuedFunctions(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, DbTransaction transaction = null);

        DiscoveredStoredprocedure[] ListStoredprocedures(DbConnectionStringBuilder builder, string database);

        IDiscoveredTableHelper GetTableHelper();
        void DropDatabase(DiscoveredDatabase database);

        Dictionary<string, string> DescribeDatabase(DbConnectionStringBuilder builder, string database);
        
    }
}