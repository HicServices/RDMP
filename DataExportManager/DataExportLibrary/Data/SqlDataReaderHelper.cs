using System;
using System.Data;

namespace DataExportLibrary.Data
{
    class SqlDataReaderHelper
    {
        public static bool HasColumn( IDataRecord dr, string columnName, out string fixedCaps)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    fixedCaps = dr.GetName(i);
                    return true;
                }
                    
            }
            fixedCaps = null;
            return false;
        }
    }
}
