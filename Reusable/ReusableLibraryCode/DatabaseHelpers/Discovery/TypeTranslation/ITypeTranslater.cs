using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
    public interface ITypeTranslater
    {
        string GetSQLDBTypeForCSharpType(DatabaseTypeRequest request);
        Type GetCSharpTypeForSQLDBType(string sqlType);
    }
}