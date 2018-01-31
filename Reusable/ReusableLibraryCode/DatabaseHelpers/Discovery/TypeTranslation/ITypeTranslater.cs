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

        DataTypeComputer GetDataTypeComputerFor(DiscoveredColumn discoveredColumn);
    }
}