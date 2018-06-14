using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
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
