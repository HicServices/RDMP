using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;

namespace ReusableLibraryCode.Performance
{
    public class ComprehensiveQueryPerformanceCounter
    {
        public BiDictionary<string,QueryPerformed> DictionaryOfQueries = new BiDictionary<string, QueryPerformed>();

        public int CacheHits;
        public int CacheMisses;

        public ComprehensiveQueryPerformanceCounter()
        {
            
        }

        public void AddAudit(DbCommand cmd, string environmentDotStackTrace)
        {
            
            //is it a novel origin
            if (!DictionaryOfQueries.Firsts.Contains(environmentDotStackTrace))
                DictionaryOfQueries.Add(environmentDotStackTrace, new QueryPerformed(cmd.CommandText));

            var query = DictionaryOfQueries.GetByFirst(environmentDotStackTrace);
            query.IncrementSeenCount();
        }
    }
}