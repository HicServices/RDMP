using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Contains all the DatabaseType specific implementation logic required by DiscoveredDatabase.
    /// </summary>
    public interface IDiscoveredDatabaseHelper
    {

        IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, bool includeViews, DbTransaction transaction = null);
        IEnumerable<DiscoveredTableValuedFunction> ListTableValuedFunctions(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, DbTransaction transaction = null);

        DiscoveredStoredprocedure[] ListStoredprocedures(DbConnectionStringBuilder builder, string database);

        IDiscoveredTableHelper GetTableHelper();
        void DropDatabase(DiscoveredDatabase database);

        Dictionary<string, string> DescribeDatabase(DbConnectionStringBuilder builder, string database);

        DiscoveredTable CreateTable(DiscoveredDatabase database, string tableName, DataTable dt, DatabaseColumnRequest[] explicitColumnDefinitions = null, bool createEmpty=false);
        DiscoveredTable CreateTable(DiscoveredDatabase database, string tableName, DatabaseColumnRequest[] columns);
    }
}