using System;
using System.Globalization;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Extensions;

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

        protected override bool IsLong(string sqlType)
        {
            return base.IsLong(sqlType)
                || sqlType.Equals("int8", StringComparison.CurrentCultureIgnoreCase);
        }

        protected override bool IsByte(string sqlType)
        {
            return base.IsByte(sqlType)
                || sqlType.Equals("int1", StringComparison.CurrentCultureIgnoreCase);
        }

        protected override bool IsSmallInt(string sqlType)
        {

            return base.IsSmallInt(sqlType)
                || sqlType.Equals("int2",StringComparison.CurrentCultureIgnoreCase);
        }
        
        protected override bool IsInt(string sqlType)
        {
            return base.IsInt(sqlType)
                || sqlType.Equals("int3", StringComparison.CurrentCultureIgnoreCase)
                || sqlType.Equals("int4", StringComparison.CurrentCultureIgnoreCase)
                || sqlType.Contains("mediumint", CompareOptions.IgnoreCase);  
        }

        protected override bool IsDate(string sqlType)
        {
            return base.IsDate(sqlType)
                || sqlType.Equals("timestamp", StringComparison.CurrentCultureIgnoreCase);
        }

        protected override bool IsString(string sqlType)
        {
            //yup thats right!, long is string (MEDIUMTEXT)
            //https://dev.mysql.com/doc/refman/8.0/en/other-vendor-data-types.html
            if (sqlType.Equals("long",StringComparison.CurrentCultureIgnoreCase))
                return true;

            return base.IsString(sqlType)
                   || sqlType.Contains("enum", CompareOptions.IgnoreCase)
                   || sqlType.Contains("set", CompareOptions.IgnoreCase);
        }

        protected override bool IsFloatingPoint(string sqlType)
        {
            return base.IsFloatingPoint(sqlType)
                   || sqlType.Equals("dec", StringComparison.CurrentCultureIgnoreCase)
                   || sqlType.Equals("fixed", StringComparison.CurrentCultureIgnoreCase);
        }

        protected override bool IsByteArray(string sqlType)
        {
            return base.IsByteArray(sqlType)
                || sqlType.Contains("binary",CompareOptions.IgnoreCase)
                || sqlType.Contains("blob", CompareOptions.IgnoreCase);

        }

        protected override bool IsBit(string sqlType)
        {
            return base.IsBit(sqlType)
                || sqlType.Equals("tinyint(1)", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}