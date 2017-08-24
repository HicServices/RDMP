using System;

namespace LoadModules.Generic.Exceptions
{
    public class MultipleMatchingFilesException : Exception
    {
        public MultipleMatchingFilesException(string s):base(s)
        {
            
        }
    }
}