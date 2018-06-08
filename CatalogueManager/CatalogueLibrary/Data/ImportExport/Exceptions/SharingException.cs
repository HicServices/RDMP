using System;

namespace CatalogueLibrary.Data.ImportExport.Exceptions
{
    public class SharingException:Exception
    {
        public SharingException(string msg) : base(msg)
        {
            
        }
        public SharingException(string msg, Exception ex):base(msg,ex)
        {
            
        }
    }
}
