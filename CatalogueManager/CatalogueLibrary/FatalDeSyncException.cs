using System;

namespace CatalogueLibrary
{
    public class FatalDeSyncException : Exception
    {
        public FatalDeSyncException(string s):base(s)
        {
            

        }

        public FatalDeSyncException(string s, Exception exception):base(s,exception)
        {
            
        }
    }
}