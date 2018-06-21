using System;

namespace CatalogueLibrary.Triggers.Exceptions
{
    /// <summary>
    /// Thrown when an _Archive table does not match the live table (See TriggerImplementer)
    /// </summary>
    public class IrreconcilableColumnDifferencesInArchiveException : TriggerException
    {
        public IrreconcilableColumnDifferencesInArchiveException(string s):base(s)
        {
            
        }
    }
}