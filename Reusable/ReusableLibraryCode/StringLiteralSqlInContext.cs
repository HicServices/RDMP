using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode
{
    public class StringLiteralSqlInContext
    {
        public string Sql { get; private set; }
        public bool IsDynamic { get; private set; }

        public StringLiteralSqlInContext(string sql, bool isDynamicAlready)
        {
            Sql = sql;
            IsDynamic = isDynamicAlready;
        }

        public void Escape(IQuerySyntaxHelper helper)
        {
            if(IsDynamic)
                throw new InvalidOperationException("query is already in dynamic sql context");

            Sql = helper.Escape(Sql);
            IsDynamic = true;
        }
    }
}
 