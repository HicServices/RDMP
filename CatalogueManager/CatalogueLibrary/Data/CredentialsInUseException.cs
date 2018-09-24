using System;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Exception thrown when you attempt to delete an DataAccessCredentials upon which a TableInfo or other class relies upon to access data.
    /// </summary>
    public class CredentialsInUseException : Exception
    {
        /// <inheritdoc>
        ///     <cref>base(string)</cref>
        /// </inheritdoc>
        public CredentialsInUseException(string s) : base(s)
        {
        
        }

        /// <inheritdoc>
        ///     <cref>base(string, Exception)</cref>
        /// </inheritdoc>
        public CredentialsInUseException(string s, Exception inner) : base(s, inner)
        {
            
        }
    }
}