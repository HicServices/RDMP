using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.ObjectVisualisation;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.DataViewing.Collections
{
    public interface IViewSQLAndResultsCollection:IPersistableObjectCollection
    {
        IHasDependencies GetAutocompleteObject();
        void SetupRibbon(RDMPObjectsRibbonUI ribbon);
        IDataAccessPoint GetDataAccessPoint();
        string GetSql();
        string GetTabName();
    }
}
