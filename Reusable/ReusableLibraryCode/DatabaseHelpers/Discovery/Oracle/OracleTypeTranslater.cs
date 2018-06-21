using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    public class OracleTypeTranslater:TypeTranslater
    {
        public OracleTypeTranslater(): base(4000, 4000)
        {
            
        }
        protected override string GetStringDataTypeImpl(int maxExpectedStringWidth)
        {
            return "varchar2(" + maxExpectedStringWidth + ")";
        }

        public override string GetStringDataTypeWithUnlimitedWidth()
        {
            return "CLOB";
        }

        protected override bool IsString(string sqlType)
        {
            if (sqlType.Equals("CLOB", StringComparison.InvariantCultureIgnoreCase))
                return true;

            return base.IsString(sqlType);
        }

        public override int GetLengthIfString(string sqlType)
        {
            if (sqlType.Equals("CLOB", StringComparison.InvariantCultureIgnoreCase))
                return int.MaxValue;

            return base.GetLengthIfString(sqlType);
        }

        protected override string GetDateDateTimeDataType()
        {
            return "DATE";
        }
    }
}
