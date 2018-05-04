using System;

namespace CatalogueLibrary.Repositories.Construction.Exceptions
{
    /// <summary>
    /// Exception thrown when ObjectConstructor is unable to find any ConstructorInfos that are compatible with the provided parameters
    /// </summary>
    public class ObjectLacksCompatibleConstructorException : Exception
    {
        /// <summary>
        /// Creates a new exception describing that a Type the user requested does not have any constructors that match the signature requested
        /// </summary>
        /// <param name="msg"></param>
        public ObjectLacksCompatibleConstructorException(string msg):base(msg)
        {
            
        }
    }
}