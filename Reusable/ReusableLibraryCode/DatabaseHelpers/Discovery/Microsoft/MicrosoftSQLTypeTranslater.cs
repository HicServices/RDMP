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
            var lower = sqlType.ToLower().Trim();

            return base.IsByteArray(sqlType)
                || lower.Contains("image")
                || lower == "timestamp" 
                || lower == "rowversion";

        }
    }
}