using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftSQLTypeTranslater : TypeTranslater
    {
        protected override string GetDateDateTimeDataType()
        {
            return "datetime2";
        }

        protected override bool IsByteArray(string sqlType)
        {
            return base.IsByteArray(sqlType) || sqlType.Contains("image");
        }
    }
}