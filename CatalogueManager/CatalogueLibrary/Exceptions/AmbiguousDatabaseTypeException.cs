using System;
using FAnsi;

namespace CatalogueLibrary.Exceptions
{
    /// <summary>
    /// Thrown when a piece of code needs to know what <see cref="DatabaseType"/> is being targetted but no determination
    /// can be made either because there are no objects of a known <see cref="DatabaseType"/> or because there are objects
    /// of multiple different <see cref="DatabaseType"/>.
    /// </summary>
    public class AmbiguousDatabaseTypeException : Exception
    {
        public AmbiguousDatabaseTypeException(string s):base(s)
        {
            
        }
    }
}