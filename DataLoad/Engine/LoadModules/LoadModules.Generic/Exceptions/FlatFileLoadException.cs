using System;

namespace LoadModules.Generic.Exceptions
{
    /// <summary>
    /// Thrown by flat file reading components (e.g. DelimitedFlatFileDataFlowSource) when there is a structural problem with the file (e.g. 3 headers and 3 cells
    /// per line but suddenly 4 cells appear on line 30).
    /// </summary>
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