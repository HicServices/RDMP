using System;

namespace CatalogueLibrary.Triggers.Exceptions
{
    public class TriggerMissingException : Exception
    {
        public TriggerMissingException(string s):base(s)
        {
            
        }
    }
}