using System;

namespace CatalogueLibrary.Data
{
    public class CredentialsInUseException : Exception
    {
        public CredentialsInUseException(string s) : base(s)
        {
        
        }

        public CredentialsInUseException(string s, Exception inner) : base(s, inner)
        {
            
        }
    }
}