using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    public class TopXResponse
    {
        public string SQL { get; set; }
        public QueryComponent Location { get; set; }

        public TopXResponse(string sql, QueryComponent location)
        {
            SQL = sql;
            Location = location;
        }
    }
}
