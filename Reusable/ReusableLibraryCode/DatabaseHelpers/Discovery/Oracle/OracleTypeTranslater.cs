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
        protected override string GetStringDataType(int? maxExpectedStringWidth)
        {
            var basic = base.GetStringDataType(maxExpectedStringWidth);

            return basic.Replace("varchar", "varchar2");
        }
        
        protected override string GetDateDateTimeDataType()
        {
            return "DATE";
        }
    }
}
