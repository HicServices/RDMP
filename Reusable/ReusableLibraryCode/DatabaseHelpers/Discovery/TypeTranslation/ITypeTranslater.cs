namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
    public interface ITypeTranslater
    {
        string GetSQLDBTypeForCSharpType(DatabaseTypeRequest request);
    }
}