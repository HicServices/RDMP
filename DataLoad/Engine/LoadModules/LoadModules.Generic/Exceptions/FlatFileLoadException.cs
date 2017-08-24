using System;

namespace LoadModules.Generic.Exceptions
{
    public class FlatFileLoadException:Exception
    {
        public FlatFileLoadException(string message)
            : base(message)
        {
        }
        public FlatFileLoadException(string message,Exception e)
            : base(message,e)
        {
        }
    }
}