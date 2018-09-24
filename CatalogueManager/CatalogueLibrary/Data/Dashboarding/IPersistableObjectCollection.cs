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
        /// <summary>
        /// Gets the class you can use to serialize/deserialize values for this collection
        /// </summary>
        PersistStringHelper Helper { get; }

        /// <summary>
        /// A list of all the currently used objects in this collection
        /// </summary>
        List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }
        
        /// <summary>
        /// Serialize any current state information about the collection that is not encapsulated in <see cref="DatabaseObjects"/> e.g. tickboxes, options, selected enums etc.
        /// <para>Returns null if there is no supplemental information to save about the collection</para>
        /// </summary>
        /// <returns></returns>
        string SaveExtraText();

        /// <summary>
        /// Hydrate the <see cref="IPersistableObjectCollection"/> state with a value that was created by <see cref="SaveExtraText"/>.  This does not include populating <see cref="DatabaseObjects"/>
        /// which happens seperately.
        /// </summary>
        /// <param name="s"></param>
        void LoadExtraText(string s);
    }
}
