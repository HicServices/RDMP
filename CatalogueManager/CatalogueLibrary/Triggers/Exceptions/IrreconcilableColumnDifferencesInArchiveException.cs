using System;

namespace CatalogueLibrary.Triggers.Exceptions
{
    public class IrreconcilableColumnDifferencesInArchiveException : Exception
    {
        public IrreconcilableColumnDifferencesInArchiveException(string s):base(s)
        {
            
        }
    }
}