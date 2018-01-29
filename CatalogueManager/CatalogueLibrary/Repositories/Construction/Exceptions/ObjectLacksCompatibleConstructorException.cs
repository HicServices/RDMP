using System;

namespace CatalogueLibrary.Repositories.Construction.Exceptions
{
    /// <summary>
    /// Exception thrown when ObjectConstructor is unable to find any ConstructorInfos that are compatible with the provided parameters
    /// </summary>
    public class ObjectLacksCompatibleConstructorException : Exception
    {
        public ObjectLacksCompatibleConstructorException(string msg):base(msg)
        {
            
        }
    }
}