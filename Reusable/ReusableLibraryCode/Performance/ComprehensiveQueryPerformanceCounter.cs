using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;

namespace ReusableLibraryCode.Performance
{
    /// <summary>
    /// Stores the location (Stack Trace) of all calls to the database (DbCommands constructed).  This does not include how long they took to run or even the 
    /// final state of the command (which could have parameters or have it's command text modified after construction).  Mostly it is useful for detecting
    /// lines of code that are sending hundreds/thousands of duplicate queries.
    /// 
    /// You can install a ComprehensiveQueryPerformanceCounter by using DatabaseCommandHelper.PerformanceCounter = new ComprehensiveQueryPerformanceCounter()
    /// 
    /// You can view the results of a ComprehensiveQueryPerformanceCounter by using a PerformanceCounterUI/PerformanceCounterResultsUI.
    /// </summary>
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