using System;

namespace HIC.Common.Validation.Dependency.Exceptions
{
    /// <summary>
    /// Thrown when you attempt to delete an object which is referenced in a Catalogues ValidatorXML
    /// </summary>
    public class ValidationXmlDependencyException : Exception
    {
        public ValidationXmlDependencyException(string s):base(s)
        {
            
        }
    }
}