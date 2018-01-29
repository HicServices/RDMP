using System;

namespace LoadModules.Generic.Exceptions
{
    /// <summary>
    /// Thrown when a file pattern expected to identify only a single file matches multiple files.
    /// </summary>
    public class MultipleMatchingFilesException : Exception
    {
        public MultipleMatchingFilesException(string s):base(s)
        {
            
        }
    }
}