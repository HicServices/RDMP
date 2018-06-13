using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlTypeTranslater : TypeTranslater
    {
        public MySqlTypeTranslater() : base(4000, 4000)
        {
        }

        public override string GetStringDataTypeWithUnlimitedWidth()
        {
            return "text";
        }
    }
}