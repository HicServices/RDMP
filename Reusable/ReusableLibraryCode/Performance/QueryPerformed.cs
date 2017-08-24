using System.Diagnostics;
using System.Threading;

namespace ReusableLibraryCode.Performance
{
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