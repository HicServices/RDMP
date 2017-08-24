using System;

namespace CatalogueLibrary.Reports.Exceptions
{
    public class SourceCodeNotFoundException : Exception
    {
        public SourceCodeNotFoundException(string msg):base(msg)
        {
            
        }
    }
}