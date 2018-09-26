using System;
using System.Collections.Generic;
using System.Data.Common;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Exceptions;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Cross database type reference to a Data Type string (e.g. varchar(30), varbinary(100) etc) of a Column in a Table
    /// </summary>
    public class DiscoveredDataType
    {
        private readonly DiscoveredColumn Column; 

        public string SQLType { get; set; }

        public int GetLengthIfString()
        {
            return Column.Table.Database.Server.Helper.GetQuerySyntaxHelper().TypeTranslater.GetLengthIfString(SQLType);
        }

        public DecimalSize GetDecimalSize()
        {
            return Column.Table.Database.Server.Helper.GetQuerySyntaxHelper().TypeTranslater.GetDigitsBeforeAndAfterDecimalPointIfDecimal(SQLType);
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
            DecimalSize toReplace = GetDecimalSize();

            if (toReplace == null || toReplace.IsEmpty)
                throw new Exception("DataType cannot be resized to decimal because it is of data type " + SQLType);

            if (toReplace.NumbersBeforeDecimalPlace > numberOfDigitsBeforeDecimalPoint)
                throw new Exception("Cannot shrink column, number of digits before the decimal point is currently " + toReplace.NumbersBeforeDecimalPlace + " and you asked to set it to " + numberOfDigitsBeforeDecimalPoint + " (Current SQLType is " + SQLType + ")");

            if (toReplace.NumbersAfterDecimalPlace> numberOfDigitsAfterDecimalPoint)
                throw new Exception("Cannot shrink column, number of digits after the decimal point is currently " + toReplace.NumbersAfterDecimalPlace + " and you asked to set it to " + numberOfDigitsAfterDecimalPoint + " (Current SQLType is " + SQLType + ")");
            
            var newDataType = Column.Table.GetQuerySyntaxHelper()
                .TypeTranslater.GetSQLDBTypeForCSharpType(new DatabaseTypeRequest(typeof (decimal), null,
                    new DecimalSize(numberOfDigitsBeforeDecimalPoint, numberOfDigitsAfterDecimalPoint)));

            
            AlterTypeTo(newDataType, managedTransaction);


        }
        public void AlterTypeTo(string newType, IManagedTransaction managedTransaction = null,int altertimeout = 500)
        {
            if(Column == null)
                throw new NotSupportedException("Cannot resize DataType because it does not have a reference to a Column to which it belongs (possibly you are trying to resize a data type associated with a TableValuedFunction Parameter?)");

            using (var connection = Column.Table.Database.Server.GetManagedConnection(managedTransaction))
            {
                string sql = Column.Helper.GetAlterColumnToSql(Column, newType, Column.AllowNulls);
                try
                {
                    var cmd = DatabaseCommandHelper.GetCommand(sql, connection.Connection, connection.Transaction);
                    cmd.CommandTimeout = altertimeout;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new AlterFailedException("Failed to send resize SQL:" + sql, e);
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
