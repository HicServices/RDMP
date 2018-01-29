using System;

namespace CatalogueLibrary.Reports.Exceptions
{
    /// <summary>
    /// Thrown if the "SourceCodeForSelfAwareness.zip" file is not present where it is expected.  RDMP bundles it's source code in a flattened zip file called 
    /// "SourceCodeForSelfAwareness.zip" in order to generate dynamic help and display helper information about stack traces (see ExceptionViewerStackTraceWithHyperlinks) 
    /// </summary>
    public class SourceCodeNotFoundException : Exception
    {
        public SourceCodeNotFoundException(string msg):base(msg)
        {
            
        }
    }
}