using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DataAccess
{
    /// <summary>
    /// Object which works with a known DatabaseType and therefore has an associated IQuerySyntaxHelper (e.g. DatabaseType.MicrosoftSQLServer and 
    /// MicrosoftQuerySyntaxHelper).  
    /// 
    /// When implementing this class you most likely want to start with 'new QuerySyntaxHelperFactory().Create(DatabaseType);'
    /// </summary>
    public interface IHasQuerySyntaxHelper
    {
        IQuerySyntaxHelper GetQuerySyntaxHelper();
    }
}