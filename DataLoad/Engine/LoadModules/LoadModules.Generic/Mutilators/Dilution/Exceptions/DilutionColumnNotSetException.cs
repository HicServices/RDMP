using System;

namespace LoadModules.Generic.Mutilators.Dilution.Exceptions
{
    public class DilutionColumnNotSetException : Exception
    {
        public DilutionColumnNotSetException(string s):base(s)
        {
            
        }

        public DilutionColumnNotSetException()
        {
        }
    }
}