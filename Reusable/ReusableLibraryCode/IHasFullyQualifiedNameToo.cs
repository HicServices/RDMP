namespace ReusableLibraryCode
{
    /// <summary>
    /// A ojbect which has a Fully Qualified Name.  Fully Qualified Names are database strings in which the full path (database,name,table,column) are specified.
    /// For example a 'Fully Qualified Name' for a table in Microsoft Sql Server could be '[MyDatabase]..[MyTable]'.  A 'Fully Qualified Name' for a column could
    /// be '[MyDatabase]..[MyTable].[MyColumn]'.  The 'Runtime Name' for the previous 2 examples would be 'MyTable' and 'MyColumn' respectively.
    /// </summary>
    public interface IHasFullyQualifiedNameToo:IHasRuntimeName
    {
        /// <summary>
        /// Returns the fully qualified name of the object including both the database, table and (if applicable) column name e.g. "[MyDatabase]..[MyTable].[MyColumn]".
        /// <para>The returned value should be wraped with the appropriate qualifier characters such that it will be valid in SQL queries even if it has spaces, starts with numbers etc</para>
        /// </summary>
        /// <returns></returns>
        string GetFullyQualifiedName();
    }
}