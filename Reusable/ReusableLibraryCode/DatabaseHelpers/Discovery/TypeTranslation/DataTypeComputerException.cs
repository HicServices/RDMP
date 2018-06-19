using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
    /// <summary>
    /// Exception thrown when there is a problem with the input data provided to a <see cref="DataTypeComputer"/> that reflects misuse of the class by the programmer
    /// (e.g. feeding mixed Type input into it).
    /// </summary>
    public class DataTypeComputerException:Exception
    {
        public DataTypeComputerException(string message, Exception ex):base(message,ex)
        {
            
        }


        public DataTypeComputerException(string message) : base(message)
        {
            
        }

    }
}
