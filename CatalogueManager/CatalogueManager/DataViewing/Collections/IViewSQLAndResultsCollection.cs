using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.AutoComplete;
using CatalogueManager.ObjectVisualisation;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueManager.DataViewing.Collections
{
    public interface IViewSQLAndResultsCollection:IPersistableObjectCollection, IHasQuerySyntaxHelper
    {
        IHasDependencies GetAutocompleteObject();
        void SetupRibbon(RDMPObjectsRibbonUI ribbon);
        IDataAccessPoint GetDataAccessPoint();
        string GetSql();
        string GetTabName();
        void AdjustAutocomplete(AutoCompleteProvider autoComplete);
    }
}
