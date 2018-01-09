using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
    /// <summary>
    /// Describes a cross platform database field type you want created including maximum width for string based columns and precision/scale for decimals.
    /// 
    /// See ITypeTranslater to see how a DatabaseTypeRequest is turned into the proprietary string e.g. A DatabaseTypeRequest with CSharpType = typeof(DateTime)
    /// is translated into 'datetime2' in Microsoft SQL Server but 'datetime' in MySql server.
    /// </summary>
    public class DatabaseTypeRequest
    {
        public Type CSharpType { get; private set; }
        public int? MaxWidthForStrings { get; private set; }
        public Tuple<int,int> DecimalPlacesBeforeAndAfter { get; private set; }

        public DatabaseTypeRequest(Type cSharpType, int? maxWidthForStrings = null,
            Tuple<int, int> decimalPlacesBeforeAndAfter = null)
        {
            CSharpType = cSharpType;
            MaxWidthForStrings = maxWidthForStrings;
            DecimalPlacesBeforeAndAfter = decimalPlacesBeforeAndAfter;
        }
    }
}