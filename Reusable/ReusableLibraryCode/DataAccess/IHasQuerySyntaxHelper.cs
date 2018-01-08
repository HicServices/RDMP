using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DataAccess
{
    public interface IHasQuerySyntaxHelper
    {
        IQuerySyntaxHelper GetQuerySyntaxHelper();
    }
}