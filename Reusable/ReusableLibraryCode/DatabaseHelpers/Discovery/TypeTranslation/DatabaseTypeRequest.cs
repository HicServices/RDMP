using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
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