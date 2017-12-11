using System;

namespace CatalogueLibrary
{
    /// <summary>
    /// Occurs when attempting to synchronize the RDMP catalogue state with a live database state.  For example dropping a table from your live database which
    /// is referenced by RDMP and attempting to synchronize that reference (See TableInfoSynchronizer)
    /// </summary>
    public class SynchronizationFailedException : Exception
    {
        public SynchronizationFailedException(string s):base(s)
        {
            

        }

        public SynchronizationFailedException(string s, Exception exception):base(s,exception)
        {
            
        }
    }
}