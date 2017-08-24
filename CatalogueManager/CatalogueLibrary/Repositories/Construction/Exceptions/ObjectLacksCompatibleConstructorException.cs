using System;

namespace CatalogueLibrary.Repositories.Construction.Exceptions
{
    public class ObjectLacksCompatibleConstructorException : Exception
    {
        public ObjectLacksCompatibleConstructorException(string msg):base(msg)
        {
            
        }
    }
}