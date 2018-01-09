using System.Diagnostics;
using System.Threading;

namespace ReusableLibraryCode.Performance
{
    /// <summary>
    /// Documents an SQL query logged by ComprehensiveQueryPerformanceCounter including the number of times the query was constructed 
    /// 
    /// See ComprehensiveQueryPerformanceCounter / DatabaseCommandHelper for more info.
    /// </summary>
    public class QueryPerformed
    {
        public string QueryText;
        public int TimesSeen = 0;

        public QueryPerformed(string queryText)
        {
            QueryText = queryText;
        }

        public void IncrementSeenCount()
        {
            TimesSeen++;
        }

    }
}