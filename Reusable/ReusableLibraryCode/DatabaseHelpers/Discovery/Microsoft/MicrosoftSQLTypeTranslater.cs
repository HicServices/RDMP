using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftSQLTypeTranslater : TypeTranslater
    {
        public MicrosoftSQLTypeTranslater() : base(8000, 4000)
        {
        }

        protected override string GetDateDateTimeDataType()
        {
            return "datetime2";
        }
        
        public override string GetStringDataTypeWithUnlimitedWidth()
        {
            return "varchar(max)";
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