using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Smo;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public class DiscoveredDataType
    {
        private readonly DiscoveredColumn Column; 

        private const string StringSizeRegexPattern = @"\(([0-9]+)\)";
        private const string DecimalsBeforeAndAfterPattern = @"\(([0-9]+),([0-9]+)\)";

        public string SQLType { get; set; }

        public int GetLengthIfString()
        {
            if (string.IsNullOrWhiteSpace(SQLType))
                return -1;

            if (SQLType.Contains("(max)") || SQLType.Equals("text"))
                return int.MaxValue;

            if (SQLType.Contains("char"))
            {
                Match match = Regex.Match(SQLType, StringSizeRegexPattern);
                if (match.Success)
                    return int.Parse(match.Groups[1].Value);
            }

            return -1;
        }

        public Pair<int, int> GetDigitsBeforeAndAfterDecimalPointIfDecimal()
        {
            if (string.IsNullOrWhiteSpace(SQLType))
                return null;

            Match match = Regex.Match(SQLType, DecimalsBeforeAndAfterPattern);

            if (match.Success)
            {
                int precision = int.Parse(match.Groups[1].Value);
                int scale = int.Parse(match.Groups[2].Value);

                return new Pair<int, int>(precision - scale,scale);

            }

            return null;
        }

        public Dictionary<string,object> ProprietaryDatatype = new Dictionary<string, object>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r">All the values in r will be copied into the Dictionary property of this class called ProprietaryDatatype</param>
        /// <param name="sqlType">Your infered SQL data type for it e.g. varchar(50)</param>
        /// <param name="column">The column it belongs to, can be null e.g. if your datatype belongs to a DiscoveredParameter instead</param>
        public DiscoveredDataType(DbDataReader r, string sqlType, DiscoveredColumn column)
        {
            SQLType = sqlType;
            Column = column;
            
            for (int i = 0; i < r.FieldCount; i++)
                ProprietaryDatatype.Add(r.GetName(i), r.GetValue(i));
        }

        public override string ToString()
        {
            return SQLType;
        }


        public void Resize(int newSize, IManagedTransaction managedTransaction = null)
        {
            int toReplace = GetLengthIfString();
            
            if(newSize == toReplace)
                throw new NotSupportedException("Why are you trying to resize a column that is already " + newSize + " long (" + SQLType+")?");

            if(newSize < toReplace)
                throw new NotSupportedException("You can only grow columns, you cannot shrink them with this method.  You asked to turn the current datatype from " + SQLType + " to reduced size " + newSize );

            var newType = SQLType.Replace(toReplace.ToString(), newSize.ToString());

            AlterTypeTo(newType, managedTransaction);
        }


        /// <summary>
        /// VERY IMPORTANT: if you want decimal(4,2) then pass 2,2 because that is what it translates to in sane land (2 decimals before point, 2 after).  If this is confusing to you
        /// lookup the concepts of precision, scale
        /// </summary>
        /// <param name="numberOfDigitsBeforeDecimalPoint"></param>
        /// <param name="numberOfDigitsAfterDecimalPoint"></param>
        /// <param name="managedTransaction"></param>
        public void Resize(int numberOfDigitsBeforeDecimalPoint, int numberOfDigitsAfterDecimalPoint, IManagedTransaction managedTransaction = null)
        {
            Pair<int, int> toReplace = GetDigitsBeforeAndAfterDecimalPointIfDecimal();

            if (toReplace == null)
                throw new Exception("DataType cannot be resized to decimal because it is of data type " + SQLType);

            if (toReplace.First > numberOfDigitsBeforeDecimalPoint)
                throw new Exception("Cannot shrink column, number of digits before the decimal point is currently " + toReplace.First + " and you asked to set it to " + numberOfDigitsBeforeDecimalPoint + " (Current SQLType is " + SQLType + ")");

            if (toReplace.Second > numberOfDigitsAfterDecimalPoint)
                throw new Exception("Cannot shrink column, number of digits after the decimal point is currently " + toReplace.Second + " and you asked to set it to " + numberOfDigitsAfterDecimalPoint + " (Current SQLType is " + SQLType + ")");

            int newPrecision = numberOfDigitsAfterDecimalPoint + numberOfDigitsBeforeDecimalPoint;
            int newScale = numberOfDigitsAfterDecimalPoint;

            int oldPrecision = toReplace.First + toReplace.Second;
            int oldScale = toReplace.Second;

            string newType = SQLType.Replace(oldPrecision + "," + oldScale, newPrecision + "," + newScale);

            AlterTypeTo(newType, managedTransaction);


        }
        public void AlterTypeTo(string newType, IManagedTransaction managedTransaction = null)
        {
            if(Column == null)
                throw new NotSupportedException("Cannot resize DataType because it does not have a reference to a Column to which it belongs (possibly you are trying to resize a data type associated with a TableValuedFunction Parameter?)");

            using (var connection = Column.Table.Database.Server.GetManagedConnection(managedTransaction))
            {
                string sql = Column.Helper.GetAlterColumnToSql(Column, newType, Column.AllowNulls);
                try
                {
                    DatabaseCommandHelper.GetCommand(sql, connection.Connection, connection.Transaction).ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to send resize SQL:" + sql, e);
                }
            }

            SQLType = newType; 
        }


        protected bool Equals(DiscoveredDataType other)
        {
            return string.Equals(SQLType, other.SQLType);
        }

        public override int GetHashCode()
        {
            return (SQLType != null ? SQLType.GetHashCode() : 0);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DiscoveredDataType)obj);
        }

       
    }
}