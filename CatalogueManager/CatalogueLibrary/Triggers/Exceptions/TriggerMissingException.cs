using System;

namespace CatalogueLibrary.Triggers.Exceptions
{
    /// <summary>
    /// Exception thrown when the DLE live table does not have the expected backup trigger that moves old (overwritten) records into the
    /// shadow archive table
    /// </summary>
    public class TriggerMissingException : Exception
    {
        public TriggerMissingException(string s):base(s)
        {
            
        }
    }
}