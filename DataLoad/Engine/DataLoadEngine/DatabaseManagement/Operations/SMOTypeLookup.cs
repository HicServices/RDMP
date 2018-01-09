using System;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Smo;

namespace DataLoadEngine.DatabaseManagement.Operations
{
    /// <summary>
    /// Maps between sql types (bigint, varchar etc) and SMO types (DataType.SmallInt etc).
    /// </summary>
    public class SMOTypeLookup
    {
        public DataType GetSMODataTypeForSqlStringDataType(string sqlDataType)
        {
            int i;
            int j;

            if(string.IsNullOrWhiteSpace(sqlDataType))
                throw new ArgumentNullException("sqlDataType");

            sqlDataType = sqlDataType.ToLower().Trim();

            //get rid of anything that isn't a letter, number, bracket or comma
            sqlDataType = Regex.Replace(sqlDataType, @"[^a-z0-9,\(\)]", "");

            //Simple types not requiring length / scale / precision

            if (sqlDataType.Equals("bigint"))
                return DataType.BigInt;

            if (sqlDataType.Equals("bit"))
                return DataType.Bit;

            if (sqlDataType.Equals("smallint"))
                return DataType.SmallInt;

            if (sqlDataType.Equals("int"))
                return DataType.Int;

            if (sqlDataType.Equals("tinyint"))
                return DataType.TinyInt;

            if (sqlDataType.Equals("money"))
                return DataType.Money;

            if (sqlDataType.Equals("smallmoney"))
                return DataType.SmallMoney;
            
            if (sqlDataType.Equals("bit"))
                return DataType.Bit;

            if (sqlDataType.Equals("date"))
                return DataType.Date;

            if (sqlDataType.Equals("smalldatetime"))
                return DataType.SmallDateTime;

            if (sqlDataType.Equals("datetime"))
                return DataType.DateTime;

            if (IsMatchTwo(@"^decimal\(([0-9]+),([0-9]+)\)", sqlDataType, out i, out j))
                return DataType.Decimal(j, i);

            if (IsMatchTwo(@"^numeric\(([0-9]+),([0-9]+)\)", sqlDataType, out i, out j))
                return DataType.Numeric(j, i);

            if (sqlDataType.Equals("time"))
                return new DataType(SqlDataType.Time);

            if (sqlDataType.Equals("datetimeoffset"))
                return new DataType(SqlDataType.DateTimeOffset);
            
            if (sqlDataType.Equals("datetime2"))
                return new DataType(SqlDataType.DateTime2);

            if (IsMatch(@"^datetime2\(([0-9]+)\)",sqlDataType, out i))
                return DataType.DateTime2(i);
            
            //Binary Datatypes
            if (IsMatch(@"^varbinary\(([0-9]+)\)",sqlDataType,out i))
                return DataType.VarBinary(i);

            if (IsMatch(@"^binary\(([0-9]+)\)",sqlDataType,out i))
                return DataType.Binary(i);

            if (sqlDataType.Equals("varbinary(max)"))
                return DataType.VarBinaryMax;
            
            //Character datatypes
            if (IsMatch(@"^varchar\(([0-9]+)\)",sqlDataType,out i))
                return DataType.VarChar(i);
            
            if (sqlDataType.Equals("varchar(max)"))
                return DataType.VarCharMax;
            
            if(IsMatch(@"^char\(([0-9]+)\)",sqlDataType,out i))
                return DataType.Char(i);

            if(sqlDataType.Equals("text"))
                return DataType.Text;
            
            if (IsMatch(@"^nvarchar\(([0-9]+)\)",sqlDataType,out i))
                return DataType.NVarChar(i);

            if (sqlDataType.Equals("nvarchar(max)"))
                return DataType.NVarCharMax; 
            
            if (IsMatch(@"^nchar\(([0-9]+)\)",sqlDataType,out i))
                return DataType.NChar(i);
            
            if (sqlDataType.Equals("ntext"))
                return DataType.NText;
            
            throw new NotSupportedException("Did not know what SMO type to use for input SQL datatype '" +sqlDataType + "'");
        }

        private bool IsMatchTwo(string regexPattern, string sqlDataType, out int i, out int j)
        {
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            var m = regex.Match(sqlDataType);

            if (m.Success)
            {
                i = int.Parse(m.Groups[1].Value);
                j = int.Parse(m.Groups[2].Value);
                return true;
            }

            i = -1;
            j = -1;
            return false;
        }

        private bool IsMatch(string regexPattern, string sqlDataType, out int i)
        {
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            var m = regex.Match(sqlDataType);

            if(m.Success)
            {
                i = int.Parse(m.Groups[1].Value);
                return true;
            }

            i = -1;
            return false;
        }
    }
}