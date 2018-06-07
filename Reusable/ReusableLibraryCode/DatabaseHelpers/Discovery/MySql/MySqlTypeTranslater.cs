using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlTypeTranslater : TypeTranslater
    {
        protected override string GetStringDataType(int? maxExpectedStringWidth)
        {
            if (maxExpectedStringWidth == null)
                return "varchar(4000)";

            if (maxExpectedStringWidth > 8000)
                return "text";

            return "varchar(" + maxExpectedStringWidth + ")";
        }
    }
}