using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Dashboarding
{
    /// <summary>
    /// A collection of database objects used to drive an IObjectCollectionControl which is a user interface tab that requires multiple root objects in order to be created
    /// persisted and mainted.  All tabs in RDMP are either IObjectCollectionControl, IRDMPSingleDatabaseObjectControl or RDMPCollectionUI.  Try to avoid using collections if
    /// it is possible to hydrate the UI from one database object
    /// 
    /// <para>A good example of an IObjectCollectionControl (which are driven by IPersistableObjectCollection) is CohortSummaryAggregateGraph which requires both a graph and a cohort
    /// and the UI shows a summary graph adjusted to match only records in the cohort.</para>
    /// </summary>
    public interface IPersistableObjectCollection
    {
        PersistStringHelper Helper { get; }

        List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }
        
        string SaveExtraText();
        void LoadExtraText(string s);
    }
}
