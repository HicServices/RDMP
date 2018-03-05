using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
    /// <summary>
    ///  Cross database type functionality for translating between database proprietary datatypes e.g. varchar (varchar2 in Oracle) and the C# Type (and vice
    ///  versa).  
    /// 
    /// When translating into a database type from a C# Type you also need to know additonal information e.g. how long is the maximum length of a string, how much
    /// scale/precision should a decimal have.  This is represented by the DatabaseTypeRequest class.
    /// 
    /// </summary>
    public interface ITypeTranslater
    {
        /// <summary>
        ///  DatabaseTypeRequest is turned into the proprietary string e.g. A DatabaseTypeRequest with CSharpType = typeof(DateTime) is translated into 
        /// 'datetime2' in Microsoft SQL Server but 'datetime' in MySql server.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        string GetSQLDBTypeForCSharpType(DatabaseTypeRequest request);

        /// <summary>
        /// Translates a database proprietary type e.g. 'decimal(10,2)' into a C# type 'typeof(String)'
        /// </summary>
        /// <param name="sqlType"></param>
        /// <returns></returns>
        Type GetCSharpTypeForSQLDBType(string sqlType);
        DatabaseTypeRequest GetDataTypeRequestForSQLDBType(string sqlType);

        DataTypeComputer GetDataTypeComputerFor(DiscoveredColumn discoveredColumn);

        int GetLengthIfString(string sqlType);
        Tuple<int, int> GetDigitsBeforeAndAfterDecimalPointIfDecimal(string sqlType);
        
        /// <summary>
        /// Translates the given sqlType which must be an SQL string compatible with this TypeTranslater e.g. varchar(10) into the destination ITypeTranslater
        /// e.g. Varchar2(10) if destinationTypeTranslater was Oracle.  Even if both this and the destination are the same you might find a different datatype
        /// due to translation preference and Type merging e.g. text might change to varchar(max)
        /// </summary>
        /// <param name="sqlType"></param>
        /// <param name="destinationTypeTranslater"></param>
        /// <returns></returns>
        string TranslateSQLDBType(string sqlType, ITypeTranslater destinationTypeTranslater);
    }
}