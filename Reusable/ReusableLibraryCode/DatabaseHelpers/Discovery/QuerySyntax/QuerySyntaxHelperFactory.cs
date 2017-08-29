using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    public class QuerySyntaxHelperFactory
    {
        public IQuerySyntaxHelper Create(DatabaseType type)
        {
            return new DatabaseHelperFactory(type).CreateInstance().GetQuerySyntaxHelper();
        }
    }
}
