using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlColumnHelper : IDiscoveredColumnHelper
    {

        public string GetTopXSqlForColumn(IHasRuntimeName database, IHasFullyQualifiedNameToo table, IHasRuntimeName column, int topX, bool discardNulls)
        {
            throw new NotImplementedException();
        }
    }
}