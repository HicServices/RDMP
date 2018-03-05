using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
    /// <summary>
    /// See ITypeTranslater
    /// </summary>
    public class TypeTranslater:ITypeTranslater
    {
        protected const string StringSizeRegexPattern = @"\(([0-9]+)\)";
        protected const string DecimalsBeforeAndAfterPattern = @"\(([0-9]+),([0-9]+)\)";

        public string GetSQLDBTypeForCSharpType(DatabaseTypeRequest request)
        {
            var t = request.CSharpType;

            if (t == typeof(int) || t == typeof(Int32)  || t == typeof(uint) || t == typeof(int?) || t == typeof(uint?))
                return GetIntDataType();

            if (t == typeof (short) || t == typeof (Int16) || t == typeof (ushort) || t == typeof (short?) || t == typeof (ushort?))
                return GetSmallIntDataType();

            if (t == typeof (long) || t == typeof(ulong) || t == typeof(long?) || t == typeof(ulong?))
                return GetBigIntDataType();

            if (t == typeof(bool) || t == typeof(bool?)) 
                return GetBoolDataType();
            
            if (t == typeof(TimeSpan) || t == typeof(TimeSpan?))
                return GetTimeDataType();

            if (t == typeof (string))
                return GetStringDataType(request.MaxWidthForStrings);

            if (t == typeof (DateTime) || t == typeof (DateTime?))
                return GetDateDateTimeDataType();

            if (t == typeof (float) || t == typeof (float?) || t == typeof (double) ||
                t == typeof (double?) || t == typeof (decimal) ||
                t == typeof (decimal?))
                return GetFloatingPointDataType(request.DecimalPlacesBeforeAndAfter);

            if (t == typeof (byte))
                return GetByteDataType();

            if (t == typeof (byte[]))
                return GetByteArrayDataType();


            throw new NotSupportedException("Unsure what SQL Database type to use for Property Type " + t.Name);

        }
        protected virtual string GetByteArrayDataType()
        {
            return "varbinary(max)";
        }

        protected virtual string GetByteDataType()
        {
            return "tinyint";
        }

        protected virtual string GetFloatingPointDataType(Tuple<int, int> decimalPlacesBeforeAndAfter)
        {
            if (decimalPlacesBeforeAndAfter == null)
                return "decimal(20,10)";
            
            return "decimal(" + (decimalPlacesBeforeAndAfter.Item1 + decimalPlacesBeforeAndAfter.Item2) + "," + decimalPlacesBeforeAndAfter.Item2 + ")";
        }

        protected virtual string GetDateDateTimeDataType()
        {
            return "datetime";
        }

        protected virtual string GetStringDataType(int? maxExpectedStringWidth)
        {
            if (maxExpectedStringWidth == null)
                return "varchar(4000)";

            if (maxExpectedStringWidth > 8000)
                return "varchar(max)";
            
            return "varchar(" + maxExpectedStringWidth + ")";
        }

        protected virtual string GetTimeDataType()
        {
            return "time";
        }

        protected virtual string GetBoolDataType()
        {
            return "bit";
        }

        protected string GetSmallIntDataType()
        {
            return "smallint";
        }

        protected virtual string GetIntDataType()
        {
            return "int";
        }
        
        protected virtual string GetBigIntDataType()
        {
            return "bigint";
        }

        public Type GetCSharpTypeForSQLDBType(string sqlType)
        {
            if (IsFloatingPoint(sqlType))
                return typeof (decimal);

            if (IsString(sqlType))
                return typeof (string);

            if (IsDate(sqlType))
                return typeof (DateTime);

            if (IsTime(sqlType))
                return typeof(TimeSpan);

            if (IsInt(sqlType))
                return typeof(int);

            if (IsBit(sqlType))
                return typeof (bool);

            if (IsByte(sqlType))
                return typeof (byte);

            if (IsByteArray(sqlType))
                return typeof (byte[]);

            throw new NotSupportedException("Not sure what type of C# datatype to use for SQL type :" + sqlType);
        }

        public virtual DatabaseTypeRequest GetDataTypeRequestForSQLDBType(string sqlType)
        {
            var cSharpType = GetCSharpTypeForSQLDBType(sqlType);

            Tuple<int, int> digits = GetDigitsBeforeAndAfterDecimalPointIfDecimal(sqlType);

            int lengthIfString = GetLengthIfString(sqlType);

            //lengthIfString should still be populated even for digits etc because it might be that we have to fallback from "1.2" which is decimal(2,1) to varchar(3) if we see "F" appearing
            if (digits != null)
                lengthIfString = Math.Max(lengthIfString, digits.Item1 + digits.Item2 + 1);

            if (cSharpType == typeof(DateTime))
                lengthIfString = GetStringLengthForDateTime();

            if (cSharpType == typeof(TimeSpan))
                lengthIfString = GetStringLengthForTimeSpan();
            
            return new DatabaseTypeRequest(cSharpType,lengthIfString,digits);
        }

        public virtual DataTypeComputer GetDataTypeComputerFor(DiscoveredColumn discoveredColumn)
        {
            var reqType = GetDataTypeRequestForSQLDBType(discoveredColumn.DataType.SQLType);

            return new DataTypeComputer(reqType.CSharpType, reqType.DecimalPlacesBeforeAndAfter, reqType.MaxWidthForStrings??-1);
        }

        public virtual int GetLengthIfString(string sqlType)
        {
            if (string.IsNullOrWhiteSpace(sqlType))
                return -1;

            if (sqlType.ToLower().Contains("(max)") || sqlType.ToLower().Equals("text"))
                return int.MaxValue;

            if (sqlType.ToLower().Contains("char"))
            {
                Match match = Regex.Match(sqlType, StringSizeRegexPattern);
                if (match.Success)
                    return int.Parse(match.Groups[1].Value);
            }

            return -1;
        }

        public Tuple<int, int> GetDigitsBeforeAndAfterDecimalPointIfDecimal(string sqlType)
        {
            if (string.IsNullOrWhiteSpace(sqlType))
                return null;

            Match match = Regex.Match(sqlType, DecimalsBeforeAndAfterPattern);

            if (match.Success)
            {
                int precision = int.Parse(match.Groups[1].Value);
                int scale = int.Parse(match.Groups[2].Value);

                return new Tuple<int, int>(precision - scale, scale);

            }

            return null;
        }

        public string TranslateSQLDBType(string sqlType, ITypeTranslater destinationTypeTranslater)
        {
            //e.g. data_type is datetime2 (i.e. Sql Server), this returns System.DateTime
            DatabaseTypeRequest requested = GetDataTypeRequestForSQLDBType(sqlType);

            //this then returns datetime (e.g. mysql)
            return destinationTypeTranslater.GetSQLDBTypeForCSharpType(requested);
        }


        /// <summary>
        /// Return the number of characters required to not truncate/loose any data when altering a column from time (e.g. TIME etc) to varchar(x).  Return
        /// x such that the column does not loose integrity.  This is needed when dynamically discovering what size to make a column by streaming data into a table. 
        /// if we see many times and nulls we will decide to use a time column then we see strings and have to convert the column to a varchar column without loosing the
        /// currently loaded data.
        /// </summary>
        /// <returns></returns>
        protected virtual int GetStringLengthForTimeSpan()
        {
            /*
             * 
             * To determine this you can run the following SQL:
              
             create table omgTimes (
dt time 
)

insert into omgTimes values (CONVERT(TIME, GETDATE()))

select * from omgTimes

alter table omgTimes alter column dt varchar(100)

select LEN(dt) from omgTimes
             

             * 
             * */
            return 16; //e.g. "13:10:58.2300000"
        }

        /// <summary>
        /// Return the number of characters required to not truncate/loose any data when altering a column from datetime (e.g. datetime2, DATE etc) to varchar(x).  Return
        /// x such that the column does not loose integrity.  This is needed when dynamically discovering what size to make a column by streaming data into a table. 
        /// if we see many dates and nulls we will decide to use a date column then we see strings and have to convert the column to a varchar column without loosing the
        /// currently loaded data.
        /// </summary>
        /// <returns></returns>
        protected virtual int GetStringLengthForDateTime()
        {
            /*
             To determine this you can run the following SQL:

create table omgdates (
dt datetime2 
)

insert into omgdates values (getdate())

select * from omgdates

alter table omgdates alter column dt varchar(100)

select LEN(dt) from omgdates
             */

            return 27; //e.g. "2018-01-30 13:05:45.1266667"
        }

        protected bool IsTime(string sqlType)
        {
            return sqlType.Trim().Equals("time",StringComparison.CurrentCultureIgnoreCase);
        }

        protected virtual bool IsByte(string sqlType)
        {
            return sqlType.ToLower().Equals("tinyint");
        }

        protected virtual bool IsByteArray(string sqlType)
        {
            return sqlType.ToLower().Contains("binary");
        }

        protected virtual bool IsBit(string sqlType)
        {
            return sqlType.Trim().Equals("bit", StringComparison.CurrentCultureIgnoreCase);
        }

        protected virtual bool IsInt(string sqlType)
        {
            return sqlType.Trim().Equals("int",StringComparison.CurrentCultureIgnoreCase);
        }

        protected virtual bool IsDate(string sqlType)
        {
            return sqlType.ToLower().Contains("date");
        }

        protected virtual bool IsString(string sqlType)
        {
            return sqlType.ToLower().Contains("char") || sqlType.ToLower().Contains("text");
        }

        protected virtual bool IsFloatingPoint(string sqlType)
        {
            foreach (var s in new[] { "float","decimal","numeric" })
            {
                if (sqlType.Trim().StartsWith(s, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
