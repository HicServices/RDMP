using System;

namespace LoadModules.Generic.Mutilators.Dilution.Exceptions
{
    /// <summary>
    /// Thrown when you try to use a DilutionOperation when the target ColumnToDilute has not been set.
    /// </summary>
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