namespace ReusableLibraryCode
{
    /// <summary>
    /// A ojbect which has a Fully Qualified Name.  Fully Qualified Names are database strings in which the full path (database,name,table,column) are specified.
    /// For example a 'Fully Qualified Name' for a table in Microsoft Sql Server could be '[MyDatabase]..[MyTable]'.  A 'Fully Qualified Name' for a column could
    /// be '[MyDatabase]..[MyTable].[MyColumn]'.  The 'Runtime Name' for the previous 2 examples would be 'MyTable' and 'MyColumn' respectively.
    /// </summary>
    public interface IHasFullyQualifiedNameToo:IHasRuntimeName
    {
        string GetFullyQualifiedName();
    }
}