using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DataAccess
{
    /// <summary>
    /// Object which works with a known DatabaseType and therefore has an associated IQuerySyntaxHelper (e.g. DatabaseType.MicrosoftSQLServer and 
    /// MicrosoftQuerySyntaxHelper).  
    /// 
    /// <para>When implementing this class you most likely want to start with 'new QuerySyntaxHelperFactory().Create(DatabaseType);'</para>
    /// </summary>
    public interface IHasQuerySyntaxHelper
    {
        /// <summary>
        /// Returns a <see cref="IQuerySyntaxHelper"/> of the correct <see cref="DatabaseType"/> depending on what remote database the declaring
        /// class is pointed at.
        /// </summary>
        /// <returns></returns>
        IQuerySyntaxHelper GetQuerySyntaxHelper();
    }
}
